#if NET6_0_OR_GREATER
namespace CB.System.Net.Diagnostics;

using global::System;
using Xunit;



/// <summary>The native <see cref="NetMonitor" />: the adapters' byte counters, with a fake counter and clock.</summary>
public class NetMonitorNativeTests {
  private const long _frequency = 1000; // clock ticks per second: a tick is a millisecond

  private long _bytes;
  private long _now;



  private NetMonitor CreateMonitor() => new("", () => _bytes, () => _now, _frequency);



  [Fact]
  public void Stop_WithBytesReceivedSinceStart_ExpectTheCountAndTheRate() {
    _bytes = 1_000_000; // traffic from before Start does not count
    var monitor = CreateMonitor();

    monitor.Start();
    _bytes += 5_000_000;
    _now += 2000;
    monitor.Stop();

    Assert.False(monitor.Started);
    Assert.Equal(5_000_000, monitor.ReceivedBytes);
    Assert.Equal(2_500_000, monitor.BytesPerSecond);
  }



  [Fact]
  public void Stop_WithACounterThatWentBackwards_ExpectZeroBytes() {
    _bytes = 8_000_000;
    var monitor = CreateMonitor();

    monitor.Start();
    _bytes = 1_000; // an adapter was removed or reset
    _now += 1000;
    monitor.Stop();

    Assert.Equal(0, monitor.ReceivedBytes);
    Assert.Equal(0, monitor.BytesPerSecond);
  }



  [Fact]
  public void Stop_WithNoTimeElapsed_ExpectNoRate() {
    var monitor = CreateMonitor();

    monitor.Start();
    _bytes += 1_000;
    monitor.Stop();

    Assert.Equal(1_000, monitor.ReceivedBytes);
    Assert.Equal(-1, monitor.BytesPerSecond);
  }



  [Fact]
  public void Stop_AfterTwoRuns_ExpectOnlyTheLastRunsBytes() {
    var monitor = CreateMonitor();

    monitor.Start();
    _bytes += 4_000;
    _now += 1000;
    monitor.Stop();
    monitor.Start();
    _bytes += 600;
    _now += 1000;
    monitor.Stop();

    Assert.Equal(600, monitor.ReceivedBytes);
  }



  [Fact]
  public void Stop_WithoutStart_ExpectInvalidOperationException() =>
    Assert.Throws<InvalidOperationException>(() => CreateMonitor().Stop());



  [Fact]
  public void SupportsFilter_ExpectFalseWhereTheCountersAreNative() => Assert.False(NetMonitor.SupportsFilter);



  [Theory]
  [InlineData("dst host 9.9.9.9")]
  [InlineData("tcp port 80")]
  public void Constructor_WithAFilter_ExpectPlatformNotSupportedException(string filter) =>
    Assert.Throws<PlatformNotSupportedException>(() => new NetMonitor(filter));



  [Fact]
  public void Constructor_WithAFilterBuilder_ExpectPlatformNotSupportedException() =>
    Assert.Throws<PlatformNotSupportedException>(() => new NetMonitor(new PcapFilterBuilder().AppendTcp()));



  [Fact]
  public void Constructor_WithANullFilterBuilder_ExpectArgumentNullException() =>
    Assert.Throws<ArgumentNullException>(() => new NetMonitor((PcapFilterBuilder)null!));



  [Fact]
  public void StartStop_WithTheRealCounters_ExpectNoFailureAndNothingNegative() {
    using var monitor = new NetMonitor();

    monitor.Start();
    monitor.Stop();

    Assert.True(monitor.ReceivedBytes >= 0);
    Assert.True(monitor.BytesPerSecond >= -1);
  }
}
#endif
