namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Reactive.Linq;
using global::System.Reactive.Threading.Tasks;
using global::System.Threading;
using global::System.Threading.Tasks;
using Xunit;



public class ProducerTests {
  [Fact(Timeout = 5000)]
  public async Task TestProducerWithArrayAndExpectHasObservable() {
    int[] expectedValues = [0, 8, 1, 5];
    using var producer = new Producer<int>(expectedValues);
    var tcs = new TaskCompletionSource<int[]>(TaskCreationOptions.RunContinuationsAsynchronously);
    var collected = new List<int>();
    producer.Subscribe(
      collected.Add,
      ex => tcs.TrySetException(ex),
      () => tcs.TrySetResult([.. collected])
    );
    producer.SetEnabled();

    Assert.Equal(expectedValues, await tcs.Task.WaitAsync(TestContext.Current.CancellationToken));
  }



  [Fact(Timeout = 20000)]
  public async Task TestProducerWithAwaitFirstExpectValueAndNoObserversAfterwards() {
    using var cts = new CancellationTokenSource();
    using var producer = new Producer<bool>(_ => true, cts.Token);

    var ct = TestContext.Current.CancellationToken;

    // Waited for, not slept for: the producer reports observers from its own thread, so a fixed delay before the
    // assertion raced it (and read a plain List that thread was still writing). HasObservers does not replay, so the
    // wait is subscribed before anything can happen: true once subscribed, then false after the unsubscribe.
    var trueThenFalse = producer.HasObservers.SkipWhile(has => !has).Where(has => !has).FirstAsync().ToTask(ct);
    var valueTask = producer.Take(1).RunAsync(ct);
    producer.SetEnabled();

    Assert.True(await valueTask);
    Assert.False(await trueThenFalse.WaitAsync(ct));
    cts.Cancel();
  }



  [Fact(Timeout = 5000)]
  public async Task TestProducerWithEnabledExpectFiresAtLeastOnce() {
    using var cts = new CancellationTokenSource();
    using var producer = new Producer<bool>(_ => true, cts.Token);
    var valueTask = producer.Take(1).RunAsync(TestContext.Current.CancellationToken);
    producer.SetEnabled();
    await Task.Delay(50, cts.Token); // Allow producer task to start
    Assert.True(await valueTask);
    cts.Cancel();
  }



  [Fact(Timeout = 5000)]
  public async Task TestProducerWithSubscribeAndExpectHasObservable() {
    using var producer = new Producer<bool>(_ => true, CancellationToken.None);
    var hasObserver = producer.HasObservers.Take(1).RunAsync(TestContext.Current.CancellationToken);
    using var d = producer.Subscribe();
    producer.SetEnabled();

    Assert.True(await hasObserver);
  }



  [Fact(Timeout = 5000)]
  public async Task TestProducerDisableThenEnableResumesProducing() {
    using var producer = new Producer<int>(_ => 1, CancellationToken.None);

    var first = producer.Take(1).RunAsync(TestContext.Current.CancellationToken);
    producer.SetEnabled();
    Assert.Equal(1, await first);

    producer.SetDisabled();

    var second = producer.Take(1).RunAsync(TestContext.Current.CancellationToken);
    producer.SetEnabled();
    Assert.Equal(1, await second);
  }



  [Fact(Timeout = 5000)]
  public async Task TestProducerCaptureThrowsOnceThenRecoversWithoutOnError() {
    var callCount = 0;

    bool Capture(CancellationToken _) {
      callCount++;
      if (callCount == 1) {
        throw new InvalidOperationException("transient capture failure");
      }

      return true;
    }

    using var producer = new Producer<bool>(Capture, CancellationToken.None);
    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var errorSeen = false;
    using var sub = producer.Subscribe(v => tcs.TrySetResult(v), _ => errorSeen = true);

    producer.SetEnabled();

    Assert.True(await tcs.Task.WaitAsync(TestContext.Current.CancellationToken));
    Assert.False(errorSeen);
  }



  [Fact(Timeout = 5000)]
  public async Task TestProducerRapidToggleDoesNotDeadlockOrThrow() {
    using var producer = new Producer<int>(_ => 1, CancellationToken.None);
    using var sub = producer.Subscribe(_ => { });

    for (var i = 0; i < 20; i++) {
      producer.SetEnabled();
      producer.SetDisabled();
    }

    var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
    using var sub2 = producer.Subscribe(v => tcs.TrySetResult(v));
    producer.SetEnabled();

    Assert.Equal(1, await tcs.Task.WaitAsync(TestContext.Current.CancellationToken));
  }
}
