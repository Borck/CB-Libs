namespace CB.System.Diagnostics;

using global::System;
using Xunit;



public class SlidingWindowRateCounterTests {
  private const long _frequency = 1000; // clock ticks per second: a tick is a millisecond

  private long _now;



  private SlidingWindowRateCounter CreateCounter(double windowSeconds) =>
    new(TimeSpan.FromSeconds(windowSeconds), () => _now, _frequency);



  [Fact]
  public void Tick_WithEventsInsideTheWindow_ExpectCountPerSecond() {
    var counter = CreateCounter(1);

    _now = 0;
    Assert.Equal(1, counter.Tick());
    _now = 100;
    Assert.Equal(2, counter.Tick());
    _now = 200;
    Assert.Equal(3, counter.Tick());
  }



  [Fact]
  public void Tick_WithEventsOlderThanTheWindow_ExpectThemDropped() {
    var counter = CreateCounter(1);
    _now = 0;
    counter.Tick();
    _now = 100;
    counter.Tick();
    _now = 200;
    counter.Tick();

    // The window is now (100, 1100]: the event at 0 is out, the one exactly on the border stays.
    _now = 1100;

    Assert.Equal(3, counter.Tick());
  }



  [Fact]
  public void Tick_WithALongerWindow_ExpectCountDividedByWindowLength() {
    var counter = CreateCounter(2);

    _now = 0;
    counter.Tick();
    _now = 500;
    counter.Tick();
    _now = 1000;

    Assert.Equal(1.5, counter.Tick());
  }



  [Fact]
  public void Tick_AfterALongPause_ExpectOnlyTheLatestEvent() {
    var counter = CreateCounter(1);
    _now = 0;
    counter.Tick();
    counter.Tick();

    _now = 60_000;

    Assert.Equal(1, counter.Tick());
  }



  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void Constructor_WithNonPositiveWindow_ExpectArgumentOutOfRange(double seconds) =>
    Assert.Throws<ArgumentOutOfRangeException>(() => new SlidingWindowRateCounter(TimeSpan.FromSeconds(seconds)));



  [Fact]
  public void Tick_WithTheRealClock_ExpectAPositiveRate() =>
    Assert.True(new SlidingWindowRateCounter(TimeSpan.FromSeconds(1)).Tick() > 0);
}
