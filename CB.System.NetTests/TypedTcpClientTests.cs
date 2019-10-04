using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using CB.System.Net.Helper;
using NUnit.Framework;



namespace CB.System.Net {
  [TestFixture]
  public class TypedTcpClientTests {
    private static IPAddress[] GetAllLocalIpAddresses() {
      return NetworkInterface
             .GetAllNetworkInterfaces()
             .Where( netInterface => netInterface.OperationalStatus == OperationalStatus.Up )
             .SelectMany( netInterface => netInterface.GetIPProperties().UnicastAddresses )
             .Select( addrInfo => addrInfo.Address )
             .ToArray();
    }



    [Test]
    [MaxTime( 10000 )]
    public async Task TestConnect_ConnectClientToServer_ExpectClientIsConnected() {
      var (vrClient, unused) = await NetworkTestHelper.ConnectTwoClientsAsync<object>();
      Assert.That( vrClient.Connected, Is.True );
    }



    [Test]
    [MaxTime( 10000 )]
    public async Task TestConnect_ConnectOverAllLocalIpAddresses_ExpectAcceptedClientsAreConnected() {
      var (unused, vrAcceptedClient) =
        await NetworkTestHelper.ConnectTwoClientsAsync<object>();
      Assert.That( vrAcceptedClient.Connected, Is.True );
    }



    [Test]
    [MaxTime( 10000 )]
    public async Task TestConnectAllLocalIpV4Addresses_ConnectClientsToServers_ExpectAcceptedClientIsConnected() {
      var ipAddrs = GetAllLocalIpAddresses();
      foreach (var ipAddress in ipAddrs.Where( addr => addr.AddressFamily == AddressFamily.InterNetwork )) {
        var localEp = new IPEndPoint( ipAddress, 0 );
        var (vrClient, vrAcceptedClient) =
          await NetworkTestHelper.ConnectTwoClientsAsync<object>( localEp );
        Assert.That( vrClient.Connected, Is.True );
        Assert.That( vrAcceptedClient.Connected, Is.True );
      }
    }



    [Test]
    [MaxTime( 10000 )]
    public async Task TestSend_SendOnePackageFromVrClientToVrServer_ExpectReceivedVrIdOfPackageIsCorrect() {
      var dataExpected = NetworkTestHelper.CreateProtoBufTestData();

      var (vrClient, vrAcceptedClient) =
        await NetworkTestHelper.ConnectTwoClientsAsync<ITestProtoContract>();
      vrClient.Send( dataExpected );

      var dataReceived = vrAcceptedClient.Receive();
      Assert.That( dataReceived, Is.EqualTo( dataExpected ) );
    }



    [Test]
    [MaxTime( 10000 )]
    public async Task TestSend_SendTwoPackagesFromVrClientToVrServer_ExpectFirstReceivedPackageIsCorrect() {
      var data3Expected = NetworkTestHelper.CreateProtoBufTestData( 3 );
      var data4Expected = NetworkTestHelper.CreateProtoBufTestData( 4 );

      var (vrClient, vrAcceptedClient) =
        await NetworkTestHelper.ConnectTwoClientsAsync<ITestProtoContract>();
      vrClient.Send( data3Expected );
      vrClient.Send( data4Expected );

      var data3Received = vrAcceptedClient.Receive();
      Assert.That( data3Received, Is.EqualTo( data3Expected ) );
    }



    [Test]
    [MaxTime( 10000 )]
    public async Task TestSend_SendTwoPackagesFromVrClientToVrServer_ExpectSecondReceivedPackageIsCorrect() {
      var data3Expected = NetworkTestHelper.CreateProtoBufTestData( 3 );
      var data4Expected = NetworkTestHelper.CreateProtoBufTestData( 4 );

      var (vrClient, vrAcceptedClient) =
        await NetworkTestHelper.ConnectTwoClientsAsync<ITestProtoContract>();
      vrClient.Send( data3Expected );
      vrClient.Send( data4Expected );

      var unused = vrAcceptedClient.Receive();
      var data4Received = vrAcceptedClient.Receive();
      Assert.That( data4Received, Is.EqualTo( data4Expected ) );
    }
  }
}
