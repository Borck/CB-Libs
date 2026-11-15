namespace CB.System.Net.Connectivity;

using global::System.Linq;
using global::System.Threading.Tasks;
using Xunit;



public class InFlightLimiterTests {
  [Fact]
  public void RequestsBeyondTheBoundAreDroppedAndTheFirstDropIsReportedOnce() {
    var limiter = new InFlightLimiter(2);

    Assert.True(limiter.TryEnter(out var dropping1));
    Assert.True(limiter.TryEnter(out var dropping2));
    Assert.False(limiter.TryEnter(out var dropping3));
    Assert.False(limiter.TryEnter(out var dropping4));

    Assert.False(dropping1 || dropping2);
    Assert.True(dropping3);
    Assert.False(dropping4);
    Assert.Equal(2, limiter.Dropped);
    Assert.Equal(2, limiter.InFlight);
  }



  [Fact]
  public void AFreedSlotIsUsableAgainAndReportsTheRecovery() {
    var limiter = new InFlightLimiter(1);

    Assert.True(limiter.TryEnter(out _));
    Assert.False(limiter.TryEnter(out _));
    Assert.True(limiter.Exit());   // it had been dropping: the remote caught up
    Assert.True(limiter.TryEnter(out var dropping));
    Assert.False(dropping);
    Assert.False(limiter.Exit());  // nothing dropped since
  }



  [Fact]
  public async Task TheBoundHoldsUnderConcurrency() {
    var limiter = new InFlightLimiter(4);
    var peak = 0;
    var gate = new object();

    await Task.WhenAll(Enumerable.Range(0, 200).Select(i => Task.Run(async () => {
      if (!limiter.TryEnter(out _)) {
        return;
      }

      lock (gate) {
        peak = global::System.Math.Max(peak, limiter.InFlight);
      }

      await Task.Delay(1);
      limiter.Exit();
    })));

    Assert.InRange(peak, 1, 4);
    Assert.Equal(0, limiter.InFlight);
  }
}
