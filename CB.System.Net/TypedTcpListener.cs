namespace CB.System.Net;

using global::System;
using global::System.Net;
using global::System.Net.Sockets;
using global::System.Threading;
using global::System.Threading.Tasks;
using CB.System.IO;

/// <summary>
///   Like a <see cref="TcpListener" />, but for <see cref="TypedTcpClient{TData}" />
/// </summary>
/// <typeparam name="TData"></typeparam>
public class TypedTcpListener<TData> {
  private readonly IFormatter<TData> _serializer;

  public readonly TcpListener Listener;

  public IPEndPoint LocalEndPoint => (IPEndPoint)Listener.LocalEndpoint;

  public bool Started => Listener.Server.IsBound;



  public TypedTcpListener(IPEndPoint localEp,
                          IFormatter<TData> serializer) {
    _serializer = serializer;
    Listener = new TcpListener(localEp);
  }



  public TypedTcpListener(IPAddress localAddress,
                          int port,
                          IFormatter<TData> serializer
  )
    : this(new IPEndPoint(localAddress, port), serializer) { }



  public void Start() => Listener.Start();



  public TypedTcpClient<TData> AcceptTcpClient() =>
    new(Listener.AcceptTcpClient(), _serializer);



  public async Task<TypedTcpClient<TData>> AcceptTcpClientAsync() =>
    new(await Listener.AcceptTcpClientAsync(), _serializer);



  /// <summary>Accepts the next client, giving up when <paramref name="cancellationToken" /> is cancelled.</summary>
  /// <remarks>
  ///   Before .NET 5 a pending accept cannot be cancelled: on net48 and netstandard2.0 this stops waiting and throws, and a client
  ///   the abandoned accept yields later is closed instead of being leaked or handed to the next caller half way.
  /// </remarks>
  /// <exception cref="OperationCanceledException">The token was cancelled before a client arrived.</exception>
  public async Task<TypedTcpClient<TData>> AcceptTcpClientAsync(CancellationToken cancellationToken) {
#if NET6_0_OR_GREATER
    return new(await Listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false), _serializer);
#else
    var accept = Listener.AcceptTcpClientAsync();
    try {
      return new(await accept.WaitAsync(cancellationToken).ConfigureAwait(false), _serializer);
    } catch (OperationCanceledException) {
      _ = accept.ContinueWith(
        static completed => completed.Result.Dispose(),
        CancellationToken.None,
        TaskContinuationOptions.OnlyOnRanToCompletion,
        TaskScheduler.Default
      );
      throw;
    }
#endif
  }
}
