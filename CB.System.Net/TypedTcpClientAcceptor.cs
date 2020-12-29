using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CB.System.IO;



namespace CB.System.Net {
  [Obsolete("Use TypedTcpClient instead")]
  public class TypedTcpClientAcceptor<TData> : IDisposable {
    private readonly IFormatter<TData> _serializer;
    private readonly Task _clientFetcher;

    public readonly TcpListener Listener;

    public IPEndPoint LocalEndPoint => (IPEndPoint)Listener.LocalEndpoint;

    public bool Started => Listener.Server.IsBound;


    public event EventHandler<TypedTcpClient<TData>>? ClientAccepted;

    private readonly CancellationTokenSource _cancelSource;



    public TypedTcpClientAcceptor(IPEndPoint localEp,
                                  IFormatter<TData> serializer) {
      _serializer = serializer;
      _cancelSource = new CancellationTokenSource();
      Listener = new TcpListener(localEp);
      _clientFetcher = new Task(DoFetchClients, _cancelSource.Token);
    }



    public TypedTcpClientAcceptor(IPAddress localAddress,
                                  int port,
                                  IFormatter<TData> serializer
    )
      : this(new IPEndPoint(localAddress, port), serializer) { }



    public void Start() {
      Listener.Start();
      _clientFetcher.Start();
    }



    private async void DoFetchClients() {
      try {
        while (Started) {
          var tcpClient = await Task.Run(() => Listener.AcceptTcpClientAsync(), _cancelSource.Token);
          var vrClient = new TypedTcpClient<TData>(tcpClient, _serializer);
          ClientAccepted?.Invoke(this, vrClient);
        }
      } finally {
        Listener.Stop();
      }

      _cancelSource.Cancel();
    }



    public void Dispose() {
      _cancelSource.Cancel();
      _cancelSource.Dispose();
    }
  }
}
