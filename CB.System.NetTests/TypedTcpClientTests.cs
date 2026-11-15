namespace CB.System.Net;

using global::System.Linq;
using global::System.Net;
using global::System.Net.NetworkInformation;
using global::System.Net.Sockets;
using global::System.Threading.Tasks;
using CB.System.Net.Helper;
using Xunit;

public class TypedTcpClientTests {
  private static IPAddress[] GetAllLocalIpAddresses() => NetworkInterface
           .GetAllNetworkInterfaces()
           .Where(netInterface => netInterface.OperationalStatus == OperationalStatus.Up)
           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
           .Select(addrInfo => addrInfo.Address)
           .ToArray();



  [Fact(Timeout = 10000)]
  public async Task TestConnect_ConnectClientToServer_ExpectClientIsConnected() {
    var (vrClient, unused) = await NetworkTestHelper.ConnectTwoClientsAsync<object>(cancellationToken: TestContext.Current.CancellationToken);
    Assert.True(vrClient.Connected);
  }



  [Fact(Timeout = 10000)]
  public async Task TestConnect_ConnectOverAllLocalIpAddresses_ExpectAcceptedClientsAreConnected() {
    var (vrClient, vrAcceptedClient) =
      await NetworkTestHelper.ConnectTwoClientsAsync<object>(cancellationToken: TestContext.Current.CancellationToken);

    try {
      Assert.True(vrAcceptedClient.Connected);
    } finally {
      vrClient.Dispose();
      vrAcceptedClient.Dispose();
    }
  }



  [Fact(Timeout = 10000)]
  public async Task TestConnectAllLocalIpV4Addresses_ConnectClientsToServers_ExpectAcceptedClientIsConnected() {
    var ipAddrs = GetAllLocalIpAddresses();
    foreach (var ipAddress in ipAddrs.Where(addr => addr.AddressFamily == AddressFamily.InterNetwork)) {
      var localEp = new IPEndPoint(ipAddress, 0);
      var (vrClient, vrAcceptedClient) =
        await NetworkTestHelper.ConnectTwoClientsAsync<object>(localEp, cancellationToken: TestContext.Current.CancellationToken);

      try {
        Assert.True(vrClient.Connected);
        Assert.True(vrAcceptedClient.Connected);
      } finally {
        vrClient.Dispose();
        vrAcceptedClient.Dispose();
      }
    }
  }



  [Fact(Timeout = 10000)]
  public async Task TestSend_SendOnePackageFromVrClientToVrServer_ExpectReceivedVrIdOfPackageIsCorrect() {
    var dataExpected = NetworkTestHelper.CreateProtoBufTestData();

    var (vrClient, vrAcceptedClient) =
      await NetworkTestHelper.ConnectTwoClientsAsync<ITestProtoContract>(cancellationToken: TestContext.Current.CancellationToken);

    try {
      vrClient.Send(dataExpected);

      var dataReceived = vrAcceptedClient.Receive();
      Assert.Equal(dataExpected, dataReceived);
    } finally {
      vrClient.Dispose();
      vrAcceptedClient.Dispose();
    }
  }



  [Fact(Timeout = 10000)]
  public async Task TestSend_SendTwoPackagesFromVrClientToVrServer_ExpectFirstReceivedPackageIsCorrect() {
    var data3Expected = NetworkTestHelper.CreateProtoBufTestData(3);
    var data4Expected = NetworkTestHelper.CreateProtoBufTestData(4);

    var (vrClient, vrAcceptedClient) =
      await NetworkTestHelper.ConnectTwoClientsAsync<ITestProtoContract>(cancellationToken: TestContext.Current.CancellationToken);

    try {
      vrClient.Send(data3Expected);
      vrClient.Send(data4Expected);

      var data3Received = vrAcceptedClient.Receive();
      Assert.Equal(data3Expected, data3Received);
    } finally {
      vrClient.Dispose();
      vrAcceptedClient.Dispose();
    }
  }



  [Fact(Timeout = 10000)]
  public async Task TestSend_SendTwoPackagesFromVrClientToVrServer_ExpectSecondReceivedPackageIsCorrect() {
    var data3Expected = NetworkTestHelper.CreateProtoBufTestData(3);
    var data4Expected = NetworkTestHelper.CreateProtoBufTestData(4);

    var (vrClient, vrAcceptedClient) =
      await NetworkTestHelper.ConnectTwoClientsAsync<ITestProtoContract>(cancellationToken: TestContext.Current.CancellationToken);

    try {
      vrClient.Send(data3Expected);
      vrClient.Send(data4Expected);

      var unused = vrAcceptedClient.Receive();
      var data4Received = vrAcceptedClient.Receive();
      Assert.Equal(data4Expected, data4Received);
    } finally {
      vrClient.Dispose();
      vrAcceptedClient.Dispose();
    }
  }
}
