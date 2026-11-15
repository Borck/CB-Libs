namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Threading;
using Microsoft.Extensions.Logging;



/// <summary>
///   Logs the failures of one connection once per cause instead of once per attempt, and the recovery with the
///   outage it ended. Thread-safe.
/// </summary>
/// <remarks>
///   <para>
///     Every connection loop used to log each failed attempt as a <c>Warning</c> carrying the full exception —
///     every 5 s for a peer, every 2–30 s for a plugin — so ten minutes of a switched-off controller was hundreds
///     of identical stack traces, and the line an operator actually needs ("it is back, after 7 minutes") did
///     not exist. This keeps the first failure and every change of cause at <c>Warning</c> with the exception
///     and an actionable reason, repeats of the same cause at <c>Debug</c>, a one-line reminder every
///     <see cref="ReminderInterval" /> while the outage lasts, and an <c>Information</c> line on recovery.
///   </para>
///   <para>
///     A connection that is up again only ends the outage once it has stayed up for
///     <see cref="ReconnectOptions.StableAfter" />: a link that keeps dropping right after it is established
///     is one outage that logs its recovery once, not a warning and a recovery line per flap.
///   </para>
///   <para>See <c>docs/engine/Resilience-Rules.md</c>, R5.</para>
/// </remarks>
public sealed partial class ConnectionFailureLog {
  #region fields and properties

  private readonly Lock _gate = new();
  private readonly ILogger _logger;
  private readonly TimeSpan _stableAfter;
  private readonly TimeProvider _time;

  private int _failures;
  private NetworkFailureKind? _lastKind;
  private DateTimeOffset _lastWarning = DateTimeOffset.MinValue;
  private DateTimeOffset? _outageStart;
  private bool _recoveryLogged;
  private DateTimeOffset? _upSince;

  /// <summary>What the connection points at, as it appears in every line (e.g. <c>"10.0.0.5:4545"</c>).</summary>
  public string Target { get; }

  /// <summary>How often a still-failing connection is mentioned again at <c>Warning</c>.</summary>
  public TimeSpan ReminderInterval { get; init; } = TimeSpan.FromMinutes(5);

  /// <summary>Consecutive failures of the current outage.</summary>
  public int Failures {
    get {
      lock (_gate) {
        return _failures;
      }
    }
  }

  /// <summary>Whether an outage is in progress (a failure was recorded and no stable connection followed).</summary>
  public bool InOutage {
    get {
      lock (_gate) {
        return _outageStart != null;
      }
    }
  }

  #endregion

  #region constructors

  public ConnectionFailureLog(ILogger logger,
                              string target,
                              ReconnectOptions? options = null,
                              TimeProvider? timeProvider = null) {
    _logger = logger;
    Target = target;
    _stableAfter = (options ?? ReconnectOptions.Default).StableAfter;
    _time = timeProvider ?? TimeProvider.System;
  }

  #endregion

  #region methods and others

  /// <summary>
  ///   Records a failed attempt (or a lost connection).
  /// </summary>
  /// <param name="exception">What failed.</param>
  /// <param name="retryIn">When the next attempt is due, if known.</param>
  /// <returns>The classified cause.</returns>
  public NetworkFailureKind Failed(Exception exception, TimeSpan? retryIn = null) {
    var kind = NetworkFailure.Classify(exception);
    var now = _time.GetUtcNow();

    lock (_gate) {
      // The previous connection counts as a recovery only if it stayed up; then this failure starts a new outage
      // and deserves a full warning even when its cause is the same as last time.
      if (_upSince is { } upSince && now - upSince >= _stableAfter) {
        _outageStart = null;
        _failures = 0;
        _lastKind = null;
        _recoveryLogged = false;
      }

      _upSince = null;
      _outageStart ??= now;
      _failures++;

      var retry = retryIn is { } delay ? $" Retrying in {Format(delay)}." : string.Empty;

      if (kind != _lastKind) {
        _lastKind = kind;
        _lastWarning = now;
        LogFailed(_logger, Target, NetworkFailure.Describe(kind), retry, exception);
      } else if (now - _lastWarning >= ReminderInterval) {
        _lastWarning = now;
        LogStillFailing(_logger, Target, Format(now - _outageStart.Value), _failures, NetworkFailure.Describe(kind), retry);
      } else {
        LogFailedAgain(_logger, Target, _failures, kind, exception.Message, retry);
      }
    }

    return kind;
  }



  /// <summary>
  ///   Records that the connection is up. Logs the recovery — once per outage — when there was one.
  /// </summary>
  public void Connected() {
    var now = _time.GetUtcNow();

    lock (_gate) {
      _upSince = now;

      if (_outageStart is not { } start) {
        return;
      }

      if (!_recoveryLogged) {
        _recoveryLogged = true;
        LogRecovered(_logger, Target, Format(now - start), _failures);
      } else {
        LogReconnectedWhileFlapping(_logger, Target, _failures);
      }
    }
  }



  private static string Format(TimeSpan span) =>
    span.TotalSeconds < 1 ? $"{span.TotalMilliseconds:0} ms" :
    span.TotalMinutes < 1 ? $"{span.TotalSeconds:0.#} s" :
    span.TotalHours < 1 ? $"{(int)span.TotalMinutes} min {span.Seconds} s" :
    $"{(int)span.TotalHours} h {span.Minutes} min";



  [LoggerMessage(LogLevel.Warning, "Connection to {Target} failed: {Reason}.{Retry}")]
  private static partial void LogFailed(ILogger logger, string target, string reason, string retry, Exception exception);



  [LoggerMessage(
    LogLevel.Warning,
    "Connection to {Target} still failing after {Outage} ({Failures} attempts): {Reason}.{Retry}"
  )]
  private static partial void LogStillFailing(
    ILogger logger,
    string target,
    string outage,
    int failures,
    string reason,
    string retry);



  [LoggerMessage(LogLevel.Debug, "Connection to {Target} failed again (attempt {Failures}, {Kind}): {Message}{Retry}")]
  private static partial void LogFailedAgain(
    ILogger logger,
    string target,
    int failures,
    NetworkFailureKind kind,
    string message,
    string retry);



  [LoggerMessage(
    LogLevel.Information,
    "Connection to {Target} established again after an outage of {Outage} ({Failures} failed attempts)."
  )]
  private static partial void LogRecovered(ILogger logger, string target, string outage, int failures);



  [LoggerMessage(
    LogLevel.Debug,
    "Connection to {Target} established again, but it has not stayed up since the outage began " +
    "({Failures} failures so far)."
  )]
  private static partial void LogReconnectedWhileFlapping(ILogger logger, string target, int failures);

  #endregion
}
