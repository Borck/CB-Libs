namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Threading;



/// <summary>
///   The retry delay of one connection: exponential, capped, jittered, and reset only once a connection has
///   proved stable (<see cref="ReconnectOptions.StableAfter" />). One instance per connection; thread-safe, so
///   the connect loop and a disconnect callback may call it from different threads.
/// </summary>
/// <remarks>
///   <para>
///     Every connection used to carry its own delay: a flat 5 s for peer links, a caller-supplied flat throttle
///     for <c>BindTcpClient</c>, and a deterministic 2 s → 30 s copied into four plugins. Two failure shapes came
///     from that. Deterministic delays make every client of a restarted server retry at exactly the same
///     instants. And resetting the delay on <em>connected</em> rather than on <em>stayed connected</em> turns a
///     link that is accepted and immediately dropped into a reconnect storm, because the loop reconnects
///     straight after every drop.
///   </para>
///   <para>
///     Usage: <see cref="NextDelay" /> before every wait, <see cref="Connected" /> when a connection is up,
///     <see cref="Disconnected" /> when it ends. See <c>docs/engine/Resilience-Rules.md</c>, R3.
///   </para>
/// </remarks>
public sealed class ReconnectBackoff {
  #region fields and properties

  private readonly Lock _gate = new();
  private readonly Func<double> _random;
  private readonly TimeProvider _time;

  // A monotonic timestamp, not a wall-clock time: an NTP step must not make a fresh link look stable, or a stable one
  // look new.
  private long? _connectedAt;

  /// <summary>The options this back-off was created with.</summary>
  public ReconnectOptions Options { get; }

  /// <summary>
  ///   Number of delays handed out since the last stable connection — the "attempt" number of the next retry
  ///   minus one.
  /// </summary>
  public int Attempt {
    get {
      lock (_gate) {
        return field;
      }
    }
    private set;
  }

  #endregion

  #region constructors

  /// <param name="options">Timing; <see cref="ReconnectOptions.Default" /> when omitted.</param>
  /// <param name="timeProvider">Clock used to judge stability; <see cref="TimeProvider.System" /> when omitted.</param>
  /// <param name="random">Source of <c>[0, 1)</c> for the jitter; <see cref="Random.Shared" /> when omitted.</param>
  public ReconnectBackoff(ReconnectOptions? options = null,
                          TimeProvider? timeProvider = null,
                          Func<double>? random = null) {
    Options = options ?? ReconnectOptions.Default;
    _time = timeProvider ?? TimeProvider.System;
    _random = random ?? Random.Shared.NextDouble;
  }

  #endregion

  #region methods and others

  /// <summary>
  ///   The delay to wait before the next attempt. Each call escalates the back-off by one step.
  /// </summary>
  public TimeSpan NextDelay() {
    int attempt;
    lock (_gate) {
      attempt = ++Attempt;
    }

    var ceiling = BaseDelay(attempt);
    var jitter = Math.Clamp(Options.Jitter, 0, 1);
    var factor = 1 - jitter + jitter * Math.Clamp(_random(), 0, 1);
    return TimeSpan.FromTicks((long)(ceiling.Ticks * factor));
  }



  /// <summary>
  ///   The un-jittered upper bound of the delay for the given 1-based attempt.
  /// </summary>
  public TimeSpan BaseDelay(int attempt) {
    var initial = Math.Max(Options.InitialDelay.Ticks, 0);
    var max = Math.Max(Options.MaxDelay.Ticks, initial);
    var multiplier = Math.Max(Options.Multiplier, 1);

    // Computed in double and clamped before converting, so a long outage cannot overflow the tick count.
    var ticks = initial * Math.Pow(multiplier, Math.Max(attempt - 1, 0));
    return TimeSpan.FromTicks((long)Math.Min(ticks, max));
  }



  /// <summary>Records that a connection is up. Does not reset the back-off by itself.</summary>
  public void Connected() {
    lock (_gate) {
      _connectedAt = _time.GetTimestamp();
    }
  }



  /// <summary>
  ///   Records that the connection ended, and resets the back-off if it had stayed up for
  ///   <see cref="ReconnectOptions.StableAfter" />.
  /// </summary>
  /// <returns><c>true</c> when the connection was stable and the back-off starts over.</returns>
  public bool Disconnected() {
    lock (_gate) {
      var stable = _connectedAt is { } since && _time.GetElapsedTime(since) >= Options.StableAfter;
      _connectedAt = null;
      if (stable) {
        Attempt = 0;
      }

      return stable;
    }
  }



  /// <summary>Starts the back-off over, e.g. after the configured endpoint changed.</summary>
  public void Reset() {
    lock (_gate) {
      Attempt = 0;
      _connectedAt = null;
    }
  }

  #endregion
}
