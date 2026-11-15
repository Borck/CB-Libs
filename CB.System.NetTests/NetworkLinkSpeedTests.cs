namespace CB.System.Net;

using global::System.Net.NetworkInformation;
using Xunit;



/// <summary>Which adapter bounds the machine's traffic, over adapters the test describes.</summary>
public class NetworkLinkSpeedTests {
  private static NetworkCard Card(
    string name,
    long bitsPerSecond,
    bool gateway = true,
    string description = "",
    OperationalStatus status = OperationalStatus.Up,
    NetworkInterfaceType type = NetworkInterfaceType.Ethernet,
    bool route = false) =>
    new(name, description, bitsPerSecond, status, type, gateway, route);



  [Theory]
  [InlineData(10_000_000_000L, 1250f)]
  [InlineData(2_500_000_000L, 312.5f)]
  [InlineData(1_000_000_000L, 125f)]
  public void SelectMBps_IsTheCardsSpeedInDecimalMegabytes(long bitsPerSecond, float expected) =>
    Assert.Equal(expected, NetworkLinkSpeed.SelectMBps([Card("Ethernet", bitsPerSecond)]));



  /// <summary>
  ///   The reported symptom: every host read 1250 regardless of its real card, because a virtual adapter
  ///   (Hyper-V, WSL, Docker) reports a nominal 10 Gbit/s and "fastest" picked it.
  /// </summary>
  [Fact]
  public void SelectMBps_IgnoresVirtualAdaptersThatClaimTenGigabit() {
    var mbps = NetworkLinkSpeed.SelectMBps([
      Card("Ethernet", 2_500_000_000L),
      Card("vEthernet (WSL)", 10_000_000_000L, gateway: false),
      Card("Ethernet 3", 10_000_000_000L, gateway: false, description: "Hyper-V Virtual Ethernet Adapter"),
      Card("docker0", 10_000_000_000L, gateway: false),
      Card("VMware Network Adapter VMnet1", 10_000_000_000L, gateway: false)
    ]);

    Assert.Equal(312.5f, mbps);
  }



  /// <summary>The OS routes through the 2.5 Gbit/s card even though a gateway-less 10 Gbit/s card exists.</summary>
  [Fact]
  public void SelectMBps_UsesTheAdapterTheOperatingSystemRoutesThrough() {
    var mbps = NetworkLinkSpeed.SelectMBps([
      Card("Ethernet", 10_000_000_000L, gateway: true),
      Card("Ethernet 2", 2_500_000_000L, gateway: false, route: true)
    ]);

    Assert.Equal(312.5f, mbps);
  }



  [Fact]
  public void SelectMBps_ANamedAdapter_WinsOverTheRoute() {
    var cards = new[] {
      Card("Ethernet", 2_500_000_000L, route: true),
      Card("Cluster", 10_000_000_000L, gateway: false)
    };

    Assert.Equal(1250f, NetworkLinkSpeed.SelectMBps(cards, "cluster"));
    Assert.Equal(312.5f, NetworkLinkSpeed.SelectMBps(cards, "does-not-exist")); // unknown name: carry on
  }



  [Fact]
  public void SelectMBps_PrefersTheAdapterWithAGateway_OverAFasterOneWithout() {
    var mbps = NetworkLinkSpeed.SelectMBps([
      Card("Ethernet", 2_500_000_000L, gateway: true),
      Card("Ethernet 2", 10_000_000_000L, gateway: false)
    ]);

    Assert.Equal(312.5f, mbps);
  }



  [Fact]
  public void SelectMBps_WithoutAGateway_TakesTheFastestRemainingCard() {
    var mbps = NetworkLinkSpeed.SelectMBps([
      Card("Ethernet", 1_000_000_000L, gateway: false),
      Card("Ethernet 2", 10_000_000_000L, gateway: false)
    ]);

    Assert.Equal(1250f, mbps);
  }



  [Fact]
  public void SelectMBps_SkipsCardsThatAreDownLoopbackOrReportNoSpeed() {
    var mbps = NetworkLinkSpeed.SelectMBps([
      Card("Ethernet", 10_000_000_000L, status: OperationalStatus.Down),
      Card("lo", 100_000_000_000L, type: NetworkInterfaceType.Loopback),
      Card("Wi-Fi", -1L, type: NetworkInterfaceType.Wireless80211)
    ]);

    Assert.Equal(0f, mbps);
  }



  [Fact]
  public void SelectMBps_WithNoCards_ExpectZero() => Assert.Equal(0f, NetworkLinkSpeed.SelectMBps([]));



  [Fact]
  public void GetMBps_OnThisMachine_ExpectNoFailureAndNeverNegative() => Assert.True(NetworkLinkSpeed.GetMBps() >= 0f);
}
