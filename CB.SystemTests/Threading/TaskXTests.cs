namespace CB.System.Threading;

using global::System;
using global::System.Threading;
using global::System.Threading.Tasks;
using Xunit;



public class TaskXTests {
  [Fact]
  public void WaitWithDeadline_WithAnOperationThatFinishes_ExpectCompletion() {
    var ran = false;

    var completed = TaskX.WaitWithDeadline(RunAsync, TimeSpan.FromSeconds(10));

    async Task RunAsync() {
      await Task.Yield();
      ran = true;
    }

    Assert.True(completed);
    Assert.True(ran);
  }



  [Fact]
  public void WaitWithDeadline_WithAnOperationThatOverruns_ExpectReturnAtTheDeadline() {
    using var release = new ManualResetEventSlim();

    var completed = TaskX.WaitWithDeadline(() => Task.Run(release.Wait), TimeSpan.FromMilliseconds(50));

    Assert.False(completed);
    release.Set();
  }



  [Fact]
  public void WaitWithDeadline_WithAFailure_ExpectTheOriginalException() {
    var thrown = Assert.Throws<InvalidOperationException>(() =>
      TaskX.WaitWithDeadline(() => throw new InvalidOperationException("stop failed"), TimeSpan.FromSeconds(10))
    );

    Assert.Equal("stop failed", thrown.Message);
  }



  [Fact]
  public void WaitWithDeadline_WithNullOperation_ExpectArgumentNullException() =>
    Assert.Throws<ArgumentNullException>(() => TaskX.WaitWithDeadline(null!, TimeSpan.FromSeconds(1)));



  /// <summary>
  ///   The point of the helper: a synchronous <c>Dispose</c> on a thread with a single-threaded context (an asset's behaviour
  ///   loop, a UI thread) must not deadlock against a stop whose continuation wants that same thread.
  /// </summary>
  [Fact]
  public void WaitWithDeadline_OnASingleThreadedContext_ExpectNoDeadlock() {
    var completed = false;
    var thread = new Thread(WaitOnABlockedContext);
    thread.Start();

    Assert.True(thread.Join(TimeSpan.FromSeconds(15)), "The wait deadlocked against its own context.");
    Assert.True(completed);

    void WaitOnABlockedContext() {
      SynchronizationContext.SetSynchronizationContext(new SingleThreadContext());
      completed = TaskX.WaitWithDeadline(async () => await Task.Delay(10), TimeSpan.FromSeconds(10));
    }
  }



  /// <summary>A context that queues continuations onto a thread that is busy waiting, i.e. never runs them.</summary>
  private sealed class SingleThreadContext : SynchronizationContext {
    public override void Post(SendOrPostCallback d, object? state) { }
  }
}
