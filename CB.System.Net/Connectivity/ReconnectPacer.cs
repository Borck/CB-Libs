namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Threading;
using global::System.Threading.Tasks;
using Microsoft.Extensions.Logging;



/// <summary>
///   The pacing half of a reconnect loop that owns its own connect and liveness logic: back-off with jitter and
///   flap guard (<see cref="ReconnectBackoff" />), once-per-cause logging (<see cref="ConnectionFailureLog" />),
///   and a wait a returning network cuts short (<see cref="NetworkChangeSignal" />). For a loop that fits the
///   connect → run → reconnect shape exactly, <see cref="ReconnectLoop" /> packages all of it instead.
/// </summary>
/// <example>
///   <code>
/// var pacer = new ReconnectPacer(logger, $"MQTT adapter '{name}' ({host}:{port})");
/// while (!ct.IsCancellationRequested) {
///   try {
///     await client.ConnectAsync(options, ct);
///     pacer.Connected();
///     await disconnected;               // however this connection reports its end
///     await pacer.DisconnectedAsync(ct); // waits only if the link did not stay up
///   } catch (Exception ex) when (!ct.IsCancellationRequested) {
///     await pacer.FailedAsync(ex, ct);
///   }
/// }
///   </code>
/// </example>
/// <remarks>
///   The loops this serves (MQTT, OPC UA, PLC, TimeSeries) each carried a copy of the same delay
///   (<c>Math.Min(30, Math.Pow(2, attempt))</c>, deterministic), reset it to zero on every connect, logged every
///   failed attempt with its stack trace, and reconnected straight after every drop — a reconnect storm against a
///   broker that kicks a duplicate client id. See <c>docs/engine/Resilience-Rules.md</c>, R3 and R5.
/// </remarks>
public sealed class ReconnectPacer {
  #region fields and properties

  /// <summary>The retry delay of this connection.</summary>
  public ReconnectBackoff Backoff { get; }

  /// <summary>The failure log of this connection.</summary>
  public ConnectionFailureLog Failures { get; }

  #endregion

  #region constructors

  public ReconnectPacer(ILogger logger,
                        string target,
                        ReconnectOptions? options = null,
                        TimeProvider? timeProvider = null) {
    Backoff = new(options, timeProvider);
    Failures = new(logger, target, options, timeProvider);
  }

  #endregion

  #region methods and others

  /// <summary>The connection is up.</summary>
  public void Connected() {
    Backoff.Connected();
    Failures.Connected();
  }



  /// <summary>
  ///   The connection ended without an error. Returns at once if it had stayed up; otherwise waits out the next
  ///   back-off step, so a link that is dropped right after it is established is not re-dialled in a tight loop.
  /// </summary>
  /// <returns>The time waited.</returns>
  public async Task<TimeSpan> DisconnectedAsync(CancellationToken cancellationToken) {
    if (Backoff.Disconnected()) {
      return TimeSpan.Zero;
    }

    var delay = Backoff.NextDelay();
    await NetworkChangeSignal.DelayAsync(delay, cancellationToken).ConfigureAwait(false);
    return delay;
  }



  /// <summary>An attempt failed, or a live connection failed: logs it (once per cause) and waits out the back-off.</summary>
  /// <returns>The time waited.</returns>
  public async Task<TimeSpan> FailedAsync(Exception exception, CancellationToken cancellationToken) {
    Backoff.Disconnected(); // a connection that had stayed up starts the back-off over
    var delay = Backoff.NextDelay();
    Failures.Failed(exception, delay);
    await NetworkChangeSignal.DelayAsync(delay, cancellationToken).ConfigureAwait(false);
    return delay;
  }

  #endregion
}
