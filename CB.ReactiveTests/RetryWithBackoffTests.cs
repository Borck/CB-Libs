namespace CB.Reactive;

using global::System;
using global::System.Linq;
using global::System.Reactive.Linq;
using global::System.Reactive.Threading.Tasks;
using global::System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;



public class RetryWithBackoffTests {
  private static readonly TimeSpan _short = TimeSpan.FromMilliseconds(1);



  [Fact(Timeout = 10000)]
  public async Task RetryWithBackoff_WithAnObservableThatFailsTwice_ExpectTheThirdAttemptsValueAndTwoWarnings() {
    var attempts = 0;
    var logger = new ListLogger();
    var source = Observable.Defer(() => {
      attempts++;
      return attempts < 3 ? Observable.Throw<int>(new InvalidOperationException("transient")) : Observable.Return(42);
    }
    );

    var value = await source.RetryWhen(ObservableX.RetryWithBackoff("read device", _short, TimeSpan.FromMilliseconds(5), logger)).FirstAsync().ToTask(TestContext.Current.CancellationToken);

    Assert.Equal(42, value);
    Assert.Equal(3, attempts);
    Assert.Equal(2, logger.Entries.Count(e => e.Level == LogLevel.Warning && e.Message.Contains("read device", StringComparison.Ordinal)));
  }



  [Fact]
  public void RetryWithBackoff_WithAnActionThatFailsTwice_ExpectItRunThreeTimes() {
    var calls = 0;
    var logger = new ListLogger();

    ObservableX.RetryWithBackoff(
      () => {
        if (++calls < 3) {
          throw new InvalidOperationException("busy");
        }
      },
      "start device",
      3,
      _short,
      _short,
      logger
    );

    Assert.Equal(3, calls);
    Assert.Equal(2, logger.Entries.Count);
  }



  [Fact]
  public void RetryWithBackoff_WithAnActionThatAlwaysFails_ExpectTheLastExceptionAfterMaxAttempts() {
    var calls = 0;

    var exception = Assert.Throws<InvalidOperationException>(() =>
      ObservableX.RetryWithBackoff(() => throw new InvalidOperationException($"attempt {++calls}"), "start device", 2, _short, _short)
    );

    Assert.Equal(2, calls);
    Assert.Equal("attempt 2", exception.Message);
  }
}
