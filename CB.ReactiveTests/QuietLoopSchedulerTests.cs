namespace CB.Reactive;

using global::System;
using global::System.Reactive.Concurrency;
using global::System.Threading;
using Xunit;



public class QuietLoopSchedulerTests {
  [Fact(Timeout = 10000)]
  public void Schedule_WithALiveLoop_ExpectTheWorkRunOnTheLoop() {
    using var loop = new EventLoopScheduler();
    var quiet = new QuietLoopScheduler(loop);
    using var ran = new ManualResetEventSlim();

    using var scheduled = quiet.Schedule(0, (_, _) => {
      ran.Set();
      return global::System.Reactive.Disposables.Disposable.Empty;
    }
    );

    Assert.True(ran.Wait(TimeSpan.FromSeconds(5)));
  }



  [Fact(Timeout = 10000)]
  public void Schedule_WithADisposedLoop_ExpectTheWorkDroppedWhereTheLoopItselfThrows() {
    var loop = new EventLoopScheduler();
    var quiet = new QuietLoopScheduler(loop);
    loop.Dispose();
    var ran = false;

    Assert.Throws<ObjectDisposedException>(() => loop.Schedule(() => ran = true));

    using var scheduled = quiet.Schedule(0, (_, _) => {
      ran = true;
      return global::System.Reactive.Disposables.Disposable.Empty;
    }
    );
    using var delayed = quiet.Schedule(0, TimeSpan.FromMilliseconds(1), (_, _) => global::System.Reactive.Disposables.Disposable.Empty);
    using var absolute = quiet.Schedule(0, DateTimeOffset.UtcNow, (_, _) => global::System.Reactive.Disposables.Disposable.Empty);

    Assert.False(ran);
  }



  [Fact(Timeout = 10000)]
  public void Now_ExpectTheLoopsClock() {
    using var loop = new EventLoopScheduler();

    Assert.True(Math.Abs((new QuietLoopScheduler(loop).Now - loop.Now).TotalSeconds) < 1);
  }
}
