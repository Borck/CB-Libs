#if !NET6_0_OR_GREATER
namespace CB.System.Net.Diagnostics;

using global::System.Net;
using global::System.Net.NetworkInformation;
using global::System.Threading;
using global::System.Threading.Tasks;
using Xunit;

// Packet capture is the fallback of the targets without a native counter (net48, netstandard2.0); the native one has its own tests.
public class NetMonitorTests {
  private const string PING_IP = "9.9.9.9";



  private static void Ping(string address)
    => new Ping().Send(address);



  [Fact(Timeout = 10000, SkipUnless = nameof(PcapEnvironment.IsCaptureAvailable), SkipType = typeof(PcapEnvironment), Skip = PcapEnvironment.SKIP_REASON)]
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
                          },
                          TestContext.Current.CancellationToken
                        );

    Assert.True(receivedBytes > 0);
  }



  [Fact(Timeout = 10000, SkipUnless = nameof(PcapEnvironment.IsCaptureAvailable), SkipType = typeof(PcapEnvironment), Skip = PcapEnvironment.SKIP_REASON)]
  public async Task DataLengthTest_FilterString_ExpectPacketReceived() {
    var receivedBytes = await new TaskFactory().StartNew(
                          () => {
                            var monitor = new NetMonitor("dst host " + PING_IP);
                            monitor.Start();

                            Ping(PING_IP);
                            Thread.Sleep(1000);

                            monitor.Stop();
                            return monitor.ReceivedBytes;
                          },
                          TestContext.Current.CancellationToken
                        );
    Assert.True(receivedBytes > 0);
  }



  [Fact(Timeout = 2000, SkipUnless = nameof(PcapEnvironment.IsCaptureAvailable), SkipType = typeof(PcapEnvironment), Skip = PcapEnvironment.SKIP_REASON)]
  public async Task StartStopTest() => await new TaskFactory().StartNew(
      () => {
        var monitor = new NetMonitor();
        monitor.Start();
        monitor.Stop();
      },
      TestContext.Current.CancellationToken
    );
}
#endif
