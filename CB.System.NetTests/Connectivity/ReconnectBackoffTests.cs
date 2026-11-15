namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Linq;
using Xunit;



public class ReconnectBackoffTests {
  private static readonly ReconnectOptions _options = new() {
    InitialDelay = TimeSpan.FromSeconds(1),
    MaxDelay = TimeSpan.FromSeconds(30),
    Multiplier = 2,
    Jitter = 0.5,
    StableAfter = TimeSpan.FromSeconds(30)
  };



  [Fact]
  public void DelaysGrowExponentiallyUpToTheCap() {
    // random = 1.0 → the top of each jitter window, i.e. the base delay itself.
    var backoff = new ReconnectBackoff(_options, random: () => 1.0);

    var delays = Enumerable.Range(0, 8).Select(_ => backoff.NextDelay().TotalSeconds).ToArray();

    Assert.Equal([1, 2, 4, 8, 16, 30, 30, 30], delays);
  }



  [Fact]
  public void JitterKeepsEveryDelayWithinTheUpperHalfOfItsWindow() {
    var backoff = new ReconnectBackoff(_options, random: () => 0.0);

    // random = 0 → the bottom of the window: half the base delay, never zero.
    Assert.Equal(TimeSpan.FromMilliseconds(500), backoff.NextDelay());
    Assert.Equal(TimeSpan.FromSeconds(1), backoff.NextDelay());
  }



  [Fact]
  public void TwoClientsOfTheSameServerDoNotRetryAtTheSameInstants() {
    var a = new ReconnectBackoff(_options, random: new Random(1).NextDouble);
    var b = new ReconnectBackoff(_options, random: new Random(2).NextDouble);

    var delaysA = Enumerable.Range(0, 5).Select(_ => a.NextDelay()).ToArray();
    var delaysB = Enumerable.Range(0, 5).Select(_ => b.NextDelay()).ToArray();

    Assert.NotEqual(delaysA, delaysB);
  }



  [Fact]
  public void AConnectionThatDropsAtOnceKeepsEscalating() {
    var time = new ManualTimeProvider();
    var backoff = new ReconnectBackoff(_options, time, () => 1.0);

    // Accepted and dropped three times in a row: a refused certificate, a licence refusal, a duplicate MQTT
    // client id. Resetting on "connected" would reconnect straight after every drop.
    backoff.NextDelay();
    for (var i = 0; i < 3; i++) {
      backoff.Connected();
      time.Advance(TimeSpan.FromSeconds(1));
      Assert.False(backoff.Disconnected());
      backoff.NextDelay();
    }

    Assert.Equal(TimeSpan.FromSeconds(16), backoff.NextDelay());
  }



  [Fact]
  public void AConnectionThatStayedUpStartsTheBackoffOver() {
    var time = new ManualTimeProvider();
    var backoff = new ReconnectBackoff(_options, time, () => 1.0);

    for (var i = 0; i < 4; i++) {
      backoff.NextDelay();
    }

    backoff.Connected();
    time.Advance(TimeSpan.FromSeconds(31));

    Assert.True(backoff.Disconnected());
    Assert.Equal(0, backoff.Attempt);
    Assert.Equal(TimeSpan.FromSeconds(1), backoff.NextDelay());
  }



  [Fact]
  public void AVeryLongOutageDoesNotOverflow() {
    var backoff = new ReconnectBackoff(_options, random: () => 1.0);

    for (var i = 0; i < 10_000; i++) {
      backoff.NextDelay();
    }

    Assert.Equal(TimeSpan.FromSeconds(30), backoff.NextDelay());
  }
}
