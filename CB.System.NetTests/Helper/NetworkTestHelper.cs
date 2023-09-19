using CB.System.IO;
using CB.System.Runtime.Serialization;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;



namespace CB.System.Net.Helper {
  public static class NetworkTestHelper {
    public static IFormatter<T> CreateProtoBufSerializer<T>()
      => new ProtoBufSerializer<T>();



    public static IRecursiveProtoContract CreateProtoBufTestData()
      => RecursiveProtoContract.Factory.CreateRecursiveSample();



    public static IRecursiveProtoContract CreateProtoBufTestData(int deep)
      => RecursiveProtoContract.Factory.CreateRecursiveSample(deep);



    public static IFormatter<T> CreatePredefinedFormatter<T>(Formatter formatter) {
      switch (formatter) {
        case Formatter.ProtoBuf:
          return CreateProtoBufSerializer<T>();
        default:
          throw new ArgumentOutOfRangeException(nameof(formatter), formatter, null);
      }
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
      ConnectTwoClientsAsync<T>(IPEndPoint localEp, Formatter formatter = Formatter.ProtoBuf) {
      return ConnectTwoClientsAsync(localEp, () => CreatePredefinedFormatter<T>(formatter));
    }



    /// <summary>
    ///   Connects two tcp client locally with a loopback IP address and using the default formatter for data serialization.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="formatter"></param>
    /// <returns></returns>
    public static Task<(TypedTcpClient<T>, TypedTcpClient<T>)>
      ConnectTwoClientsAsync<T>(Formatter formatter = Formatter.ProtoBuf) {
      return ConnectTwoClientsAsync(
        new IPEndPoint(IPAddress.Loopback, 0),
        () => CreatePredefinedFormatter<T>(formatter)
      );
    }



    public enum Formatter {
      ProtoBuf
    }
  }
}
