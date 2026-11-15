namespace CB.Reactive;

using global::System;
using global::System.Linq;
using global::System.Reactive.Linq;
using global::System.Reactive.Threading.Tasks;
using global::System.Threading.Tasks;
using Xunit;



public class PollTests {
  private sealed class PollHelper {
    private int _i;

    public int Poll() => _i++;
  }



  [Fact(Timeout = 5000)]
  public async Task Poll_WithTenIterations_ExpectValues0To9() {
    var helper = new PollHelper();

    var values = await ObservableX
                       .Poll(helper.Poll, TimeSpan.Zero)
                       .Take(10)
                       .ToList()
                       .ToTask(TestContext.Current.CancellationToken);

    Assert.Equal(Enumerable.Range(0, 10), values);
  }
}
