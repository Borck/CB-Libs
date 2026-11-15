namespace CB.System.Net;

using Xunit;



public class NetworkThroughputMeterTests {
  private const long _frequency = 1000; // clock ticks per second: a tick is a millisecond

  private long _bytes;
  private long _now;



  private NetworkThroughputMeter CreateMeter() => new(() => _bytes, () => _now, _frequency);



  [Fact]
  public void NextMBps_FirstCall_ExpectOnlyABaseline() {
    _bytes = 10_000_000;

    Assert.Equal(0f, CreateMeter().NextMBps());
  }



  [Fact]
  public void NextMBps_WithBytesMovedBetweenTwoCalls_ExpectDecimalMegabytesPerSecond() {
    var meter = CreateMeter();
    _ = meter.NextMBps();

    _bytes += 5_000_000;
    _now += 2000; // two seconds

    Assert.Equal(2.5f, meter.NextMBps(), 3);
  }



  [Fact]
  public void NextMBps_WithACounterThatWentBackwards_ExpectZeroForThatIntervalAndRecoveryAfter() {
    _bytes = 8_000_000;
    var meter = CreateMeter();
    _ = meter.NextMBps();

    _bytes = 1_000; // an adapter was removed or reset
    _now += 1000;
    Assert.Equal(0f, meter.NextMBps());

    _bytes += 3_000_000;
    _now += 1000;
    Assert.Equal(3f, meter.NextMBps(), 3);
  }



  [Fact]
  public void NextMBps_WithNoTimeElapsed_ExpectZero() {
    var meter = CreateMeter();
    _ = meter.NextMBps();

    _bytes += 1_000_000;

    Assert.Equal(0f, meter.NextMBps());
  }



  [Theory]
  [InlineData(true)]
  [InlineData(false)]
  public void NextMBps_WithTheRealAdapters_ExpectNeverNegative(bool incoming) {
    var meter = new NetworkThroughputMeter(incoming);

    _ = meter.NextMBps();

    Assert.True(meter.NextMBps() >= 0f);
  }
}
