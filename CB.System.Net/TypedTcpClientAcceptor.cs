using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CB.System.IO;
using JetBrains.Annotations;



namespace CB.System.Net {
  public class TypedTcpClientAcceptor<TData> : IDisposable {
    private readonly IFormatter<TData> _serializer;
    private readonly Thread _clientFetcher;

    [NotNull]
    public readonly TcpListener Listener;

    public IPEndPoint LocalEndPoint => (IPEndPoint) Listener.LocalEndpoint;

    public bool Started => Listener.Server.IsBound;


    public event EventHandler<TypedTcpClient<TData>> ClientAccepted;



    public TypedTcpClientAcceptor(IPEndPoint localEp, IFormatter<TData> serializer) {
      _serializer = serializer;
      Listener = new TcpListener( localEp );
      _clientFetcher = new Thread( DoFetchClients ) {
        IsBackground = true,
        Name = GetType().Name
      };
    }



    public TypedTcpClientAcceptor(IPAddress localaddr, int port, IFormatter<TData> serializer)
      : this( new IPEndPoint( localaddr, port ), serializer ) { }



    public void Start() {
      Listener.Start();
      _clientFetcher.Start();
    }



    private void DoFetchClients() {
      while (Started) {
        var tcpClient = Listener.AcceptTcpClient();
        var vrClient = new TypedTcpClient<TData>( tcpClient, _serializer );
        ClientAccepted?.Invoke( this, vrClient );
      }
    }



    public void Dispose() {
      Listener.Stop();
      _clientFetcher.Abort();
    }
  }
}
