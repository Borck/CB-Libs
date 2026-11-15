namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Diagnostics;
using global::System.IO;
using global::System.Threading;
using global::System.Threading.Tasks;



/// <summary>
///   An established connection that ended before it had stayed up for
///   <see cref="ReconnectOptions.StableAfter" /> — the remote accepts and then drops it. Classified as
///   <see cref="NetworkFailureKind.ConnectionLost" />.
/// </summary>
public sealed class ConnectionClosedException(string message) : IOException(message);



/// <summary>
///   The connect → run → wait → reconnect loop every long-lived connection needs, following
///   <c>docs/engine/Resilience-Rules.md</c> R2–R5: a deadline on every attempt, the shared back-off with jitter
///   and flap guard, log once per cause, and a wait that a returning network cuts short.
/// </summary>
public static class ReconnectLoop {
  #region methods and others

  /// <summary>
  ///   Keeps a connection up until <paramref name="cancellationToken" /> is cancelled.
  /// </summary>
  /// <param name="connectAsync">
  ///   One attempt. Receives a token that fires at <see cref="ReconnectOptions.ConnectTimeout" />; a cancellation
  ///   through it is reported as a <see cref="TimeoutException" />.
  /// </param>
  /// <param name="runAsync">
  ///   Uses the connection and returns (or throws) when it ends. Its token only fires on shutdown.
  /// </param>
  /// <param name="failures">Where failures and the recovery are logged.</param>
  /// <param name="options">Timing; <see cref="ReconnectOptions.Default" /> when omitted.</param>
  /// <param name="backoff">A back-off to share with the caller; a new one from <paramref name="options" /> when omitted.</param>
  /// <param name="cancellationToken">Ends the loop.</param>
  /// <remarks>
  ///   The connection is disposed after <paramref name="runAsync" /> returns, if it is
  ///   <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />. The loop never throws for a connection
  ///   failure; it returns when cancelled.
  /// </remarks>
  public static async Task RunAsync<TConnection>(Func<CancellationToken, Task<TConnection>> connectAsync,
                                                 Func<TConnection, CancellationToken, Task> runAsync,
                                                 ConnectionFailureLog failures,
                                                 ReconnectOptions? options = null,
                                                 ReconnectBackoff? backoff = null,
                                                 CancellationToken cancellationToken = default) {
    options ??= ReconnectOptions.Default;
    backoff ??= new(options);

    while (!cancellationToken.IsCancellationRequested) {
      TimeSpan delay;
      var connected = false;
      TConnection? connection = default;

      try {
        connection = await WithDeadlineAsync(connectAsync, options.ConnectTimeout, "Connecting", cancellationToken)
                       .ConfigureAwait(false);
        connected = true;
        backoff.Connected();
        failures.Connected();

        var since = Stopwatch.GetTimestamp();
        await runAsync(connection, cancellationToken).ConfigureAwait(false);

        var stable = backoff.Disconnected();
        connected = false;
        delay = backoff.NextDelay();
        if (!stable && !cancellationToken.IsCancellationRequested) {
          failures.Failed(
            new ConnectionClosedException(
              $"The connection closed {TimeProvider.System.GetElapsedTime(since).TotalSeconds:0.#} s after it was established."
            ),
            delay
          );
        }
      } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        return;
      } catch (Exception ex) {
        if (connected) {
          backoff.Disconnected();
        }

        delay = backoff.NextDelay();
        failures.Failed(ex, delay);
      } finally {
        await DisposeAsync(connection).ConfigureAwait(false);
      }

      try {
        await NetworkChangeSignal.DelayAsync(delay, cancellationToken).ConfigureAwait(false);
      } catch (OperationCanceledException) {
        return;
      }
    }
  }



  /// <summary>
  ///   Runs <paramref name="operation" /> with a token that fires after <paramref name="timeout" />, and reports
  ///   that as a <see cref="TimeoutException" /> rather than a bare cancellation (which reads like a shutdown).
  /// </summary>
  public static async Task<T> WithDeadlineAsync<T>(Func<CancellationToken, Task<T>> operation,
                                                   TimeSpan timeout,
                                                   string what,
                                                   CancellationToken cancellationToken = default) {
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    if (timeout != Timeout.InfiniteTimeSpan) {
      cts.CancelAfter(timeout);
    }

    try {
      return await operation(cts.Token).ConfigureAwait(false);
    } catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested) {
      throw new TimeoutException($"{what} did not complete within {timeout}.", ex);
    }
  }



  private static async ValueTask DisposeAsync<T>(T? connection) {
    try {
      switch (connection) {
        case IAsyncDisposable asyncDisposable:
          await asyncDisposable.DisposeAsync().ConfigureAwait(false);
          break;
        case IDisposable disposable:
          disposable.Dispose();
          break;
      }
    } catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException) {
      // Tearing down a connection that already failed; nothing further to report.
    }
  }

  #endregion
}
