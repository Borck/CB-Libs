namespace CB.System.Net;

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



#if NET6_0_OR_GREATER
  public async Task<TypedTcpClient<TData>> AcceptTcpClientAsync(CancellationToken cancellationToken) =>
    new(await Listener.AcceptTcpClientAsync(cancellationToken), _serializer);
#endif
}
