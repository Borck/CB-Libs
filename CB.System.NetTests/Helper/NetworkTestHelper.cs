namespace CB.System.Net.Helper;

using global::System;
using global::System.Net;
using global::System.Net.Sockets;
using global::System.Threading.Tasks;
using CB.System.IO;
using CB.System.Runtime.Serialization;

public static class NetworkTestHelper {
  public static IFormatter<T> CreateProtoBufSerializer<T>()
    => new ProtoBufSerializer<T>();



  public static ITestProtoContract CreateProtoBufTestData()
    => TestProtoContract.Factory.CreateSample();



  public static ITestProtoContract CreateProtoBufTestData(int deep)
    => TestProtoContract.Factory.CreateSample(deep);



  public static IFormatter<T> CreatePredefinedFormatter<T>(Formatter formatter) {
    return formatter switch {
      Formatter.ProtoBuf => CreateProtoBufSerializer<T>(),
      _ => throw new ArgumentOutOfRangeException(nameof(formatter), formatter, null),
    };
  }



  public static async Task<(TypedTcpClient<T>, TypedTcpClient<T>)>
    ConnectTwoClientsAsync<T>(IPEndPoint localEp, Func<IFormatter<T>> newFormatter) {
    var listener = new TcpListener(localEp);
    listener.Start();

    var taskAcceptClient = Task.Factory.StartNew(
      () => {
        var tcpClient = listener.AcceptTcpClient();
        var serializer = newFormatter.Invoke();
        return new TypedTcpClient<T>(tcpClient, serializer);
      },
      TaskCreationOptions.LongRunning
    );

    var serverEp = (IPEndPoint)listener.LocalEndpoint;

    var vrClient = new TypedTcpClient<T>(newFormatter.Invoke());
    vrClient.Connect(serverEp);

    var vrAcceptedClient = await taskAcceptClient.ConfigureAwait(false);

    return (vrClient, vrAcceptedClient);
  }



  public static Task<(TypedTcpClient<T>, TypedTcpClient<T>)>
    ConnectTwoClientsAsync<T>(IPEndPoint localEp, Formatter formatter = Formatter.ProtoBuf) => ConnectTwoClientsAsync(localEp, () => CreatePredefinedFormatter<T>(formatter));



  /// <summary>
  ///   Connects two tcp client locally with a loopback IP address and using the default formatter for data serialization.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="formatter"></param>
  /// <returns></returns>
  public static Task<(TypedTcpClient<T>, TypedTcpClient<T>)>
    ConnectTwoClientsAsync<T>(Formatter formatter = Formatter.ProtoBuf) => ConnectTwoClientsAsync(
      new IPEndPoint(IPAddress.Loopback, 0),
      () => CreatePredefinedFormatter<T>(formatter)
    );



  public enum Formatter {
    ProtoBuf
  }
}
