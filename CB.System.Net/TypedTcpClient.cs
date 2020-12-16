using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CB.System.IO;



namespace CB.System.Net {
  public class TypedTcpClient<TData> : IDisposable {
    public readonly TcpClient Client;

    private readonly IFormatter<TData> _serializer;

    public IPEndPoint LocalEndPoint => (IPEndPoint)Client.Client.LocalEndPoint;

    public IPEndPoint? RemoteEndPoint => Connected
                                           ? (IPEndPoint)Client.Client.RemoteEndPoint
                                           : null;

    public bool Connected => Client.Connected;



    public TypedTcpClient(TcpClient client, IFormatter<TData> serializer) {
      Client = client;
      _serializer = serializer;
    }



    public TypedTcpClient(IFormatter<TData> serializer)
      : this(
        new TcpClient(),
        serializer
      ) { }



    public TypedTcpClient(IPEndPoint localEp, IFormatter<TData> serializer)
      : this(new TcpClient(localEp), serializer) { }



    public void Connect(IPEndPoint sensorServerEp) {
      Client.Connect(sensorServerEp);
    }



    public void Close() {
      Client.Close();
    }



    public void CloseStream()
      => Client.GetStream().Close();



    public void Send(TData data)
      => _serializer.Serialize(Client.GetStream(), data);



    public void SendAll(params TData[] dataList) {
      foreach (var data in dataList) {
        Send(data);
      }
    }



    public IEnumerable<TData> GetReceivingEnumerable() {
      var stream = Client.GetStream();
      while (true) {
        yield return _serializer.Deserialize(stream);
      }
    }



    public TData Receive()
      => _serializer.Deserialize(Client.GetStream());



    public void Dispose() {
      (Client as IDisposable).Dispose();
    }



    public override string ToString()
      => new ToStringBuilder<TypedTcpClient<TData>>(this)
         .Append(x => x.Client.Client)
         .ToString();
  }
}
