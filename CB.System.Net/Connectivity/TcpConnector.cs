namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Net.Sockets;
using global::System.Threading;
using global::System.Threading.Tasks;



/// <summary>
///   Opens a TCP connection with one deadline covering name resolution and connect, through
///   <see cref="HostResolver" />, trying every resolved address in turn.
/// </summary>
/// <remarks>
///   <c>TcpClient.Connect(host, port)</c> blocks for the OS connect timeout (about 21 s on Windows, about two
///   minutes on Linux) and re-resolves the name every time; <c>ConnectAsync(host, port, token)</c> with only a
///   shutdown token is the same wait without the blocked thread. See <c>docs/engine/Resilience-Rules.md</c>,
///   R2 and R4.
/// </remarks>
public static class TcpConnector {
  #region methods and others

  /// <summary>
  ///   Connects to <paramref name="host" />:<paramref name="port" /> within <paramref name="timeout" />.
  /// </summary>
  /// <param name="configure">
  ///   Applied to the client before the connect (buffer sizes, socket options).
  /// </param>
  /// <param name="resolver"><see cref="HostResolver.Default" /> when omitted.</param>
  /// <exception cref="TimeoutException">Resolution and connect did not complete within the deadline.</exception>
  public static async Task<TcpClient> ConnectAsync(string host,
                                                   int port,
                                                   TimeSpan timeout,
                                                   Action<TcpClient>? configure = null,
                                                   HostResolver? resolver = null,
                                                   CancellationToken cancellationToken = default) {
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    if (timeout != Timeout.InfiniteTimeSpan) {
      cts.CancelAfter(timeout);
    }

    var client = new TcpClient();
    try {
      configure?.Invoke(client);
      var resolved = await (resolver ?? HostResolver.Default).ResolveAsync(host, timeout, cts.Token)
                                                             .ConfigureAwait(false);
      await client.ConnectAsync(resolved.Addresses, port, cts.Token).ConfigureAwait(false);
      return client;
    } catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested) {
      client.Dispose();
      throw new TimeoutException($"Connecting to {host}:{port} did not complete within {timeout}.", ex);
    } catch {
      client.Dispose();
      throw;
    }
  }

  #endregion
}
