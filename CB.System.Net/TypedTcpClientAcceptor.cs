namespace CB.System.Net;

using global::System;
using global::System.Net;
using global::System.Net.Sockets;
using global::System.Threading;
using global::System.Threading.Tasks;
using CB.System.IO;

[Obsolete("Use TypedTcpClient instead")]
public class TypedTcpClientAcceptor<TData> : IDisposable {
  private readonly IFormatter<TData> _serializer;
  private readonly CancellationTokenSource _cancelSource;
  private readonly CancellationToken _cancellation;
#if NET9_0_OR_GREATER
  private readonly Lock _gate = new();
#else
  private readonly object _gate = new();
#endif
  private Task? _clientFetcher;
  private bool _disposed;

  public readonly TcpListener Listener;

  public IPEndPoint LocalEndPoint => (IPEndPoint)Listener.LocalEndpoint;

  public bool Started => Listener.Server.IsBound;


  public event EventHandler<TypedTcpClient<TData>>? ClientAccepted;



  public TypedTcpClientAcceptor(IPEndPoint localEp,
                                IFormatter<TData> serializer) {
    _serializer = serializer;
    _cancelSource = new();
    _cancellation = _cancelSource.Token;
    Listener = new(localEp);
  }



  public TypedTcpClientAcceptor(IPAddress localAddress,
                                int port,
                                IFormatter<TData> serializer
  )
    : this(new IPEndPoint(localAddress, port), serializer) { }



  public void Start() {
    lock (_gate) {
      ObjectDisposedException.ThrowIf(_disposed, this);
      Listener.Start();
      _clientFetcher = Task.Run(FetchClientsAsync);
    }
  }



  // A Task, not async void: an exception out of async void ends the process. Stopping is the normal way out of the loop - the
  // pending accept is cancelled or its listener is closed under it - and is not reported as a failure.
  private async Task FetchClientsAsync() {
    try {
      while (Started && !_cancellation.IsCancellationRequested) {
        var tcpClient =
#if NET6_0_OR_GREATER
          await Listener.AcceptTcpClientAsync(_cancellation).ConfigureAwait(false);
#else
          await Task.Run(() => Listener.AcceptTcpClientAsync(), _cancellation).ConfigureAwait(false);
#endif
        ClientAccepted?.Invoke(this, new(tcpClient, _serializer));
      }
    } catch (Exception ex) when (_cancellation.IsCancellationRequested && ex is OperationCanceledException or ObjectDisposedException or SocketException) {
      // Disposed while waiting for a client.
    } finally {
      Listener.Stop();
    }
  }



  public void Dispose() {
    Task? fetcher;
    lock (_gate) {
      if (_disposed) {
        return;
      }

      _disposed = true;
      fetcher = _clientFetcher;
    }

    _cancelSource.Cancel();
    // Closing the listener releases an accept that cannot be cancelled (net48, netstandard2.0).
    Listener.Stop();

    // The source is disposed once the loop that reads its token has ended, never under it.
    if (fetcher is null) {
      _cancelSource.Dispose();
    } else {
      _ = fetcher.ContinueWith(static (_, state) => ((CancellationTokenSource)state!).Dispose(), _cancelSource, TaskScheduler.Default);
    }

    GC.SuppressFinalize(this);
  }
}
