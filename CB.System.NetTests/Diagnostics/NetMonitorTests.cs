using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Xunit;



namespace CB.System.Net.Diagnostics {
  public class NetMonitorTests {
    private const string PING_IP = "9.9.9.9";



    private static void Ping(string address)
      => new Ping().Send(address);



    [Fact(Timeout = 10000)]
    public async Task DataLengthTest_FilterBuilder_ExpectPacketReceived() {
      var pingIp = IPAddress.Parse(PING_IP);
      var filterBuilder = new PcapFilterBuilder()
        .AppendIpDst(pingIp);

      var receivedBytes = await new TaskFactory().StartNew(
                            () => {
                              var monitor = new NetMonitor(filterBuilder);
                              monitor.Start();

                              Ping(PING_IP);
                              Thread.Sleep(1000);

                              monitor.Stop();
                              return monitor.ReceivedBytes;
                            }
                          );

      Assert.True(receivedBytes > 0);
    }



    [Fact(Timeout = 10000)]
    public async Task DataLengthTest_FilterString_ExpectPacketReceived() {
      var receivedBytes = await new TaskFactory().StartNew(
                            () => {
                              var monitor = new NetMonitor("dst host " + PING_IP);
                              monitor.Start();

                              Ping(PING_IP);
                              Thread.Sleep(1000);

                              monitor.Stop();
                              return monitor.ReceivedBytes;
                            }
                          );
      Assert.True(receivedBytes > 0);
    }



    [Fact(Timeout = 2000)]
    public async Task StartStopTest() {
      await new TaskFactory().StartNew(
        () => {
          var monitor = new NetMonitor();
          monitor.Start();
          monitor.Stop();
        }
      );
    }
  }
}
