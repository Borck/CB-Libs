using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using NUnit.Framework;



namespace CB.System.Net.Diagnostics {
  [TestFixture]
  public class NetMonitorTests {
    private const string PING_IP = "9.9.9.9";



    private static void Ping(string address)
      => new Ping().Send( address );



    [Test]
    [MaxTime( 10000 )]
    public void DataLengthTest_FilterBuilder_ExpectPacketReceived() {
      var pingIp = IPAddress.Parse( PING_IP );
      var filterBuilder = new PcapFilterBuilder()
        .AppendIpDst( pingIp );

      var monitor = new NetMonitor( filterBuilder );
      monitor.Start();

      Ping( PING_IP );
      Thread.Sleep( 1000 );

      monitor.Stop();
      Assert.That( monitor.ReceivedBytes, Is.Positive );
    }



    [Test]
    [MaxTime( 10000 )]
    public void DataLengthTest_FilterString_ExpectPacketReceived() {
      var monitor = new NetMonitor( "dst host " + PING_IP );
      monitor.Start();

      Ping( PING_IP );
      Thread.Sleep( 1000 );

      monitor.Stop();
      Assert.That( monitor.ReceivedBytes, Is.Positive );
    }



    [Test]
    [MaxTime( 2000 )]
    public void StartStopTest() {
      var monitor = new NetMonitor();
      monitor.Start();
      monitor.Stop();
    }
  }
}
