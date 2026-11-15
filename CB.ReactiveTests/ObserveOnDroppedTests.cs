namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Reactive.Linq;
using global::System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using Xunit;



/// <summary>
///   Deterministic contract for <see cref="ObservableX.ObserveOnDisposingDropped{T}" /> and the
///   teardown fix in <see cref="ObservableX.DropWhenBusy{T}" />: items queued on the scheduler when the
///   subscription is disposed must be released, never silently dropped, while delivered items are left
///   untouched (the downstream owns them).
/// </summary>
public class ObserveOnDisposingDroppedTests {
  private sealed class Tracked : IDisposable {
    public int DisposeCount { get; private set; }
    public bool Disposed => DisposeCount > 0;

    public void Dispose() => DisposeCount++;
  }



  [Fact]
  public void DeliversInOrder_OnScheduler() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var a = new Tracked();
    var b = new Tracked();
    var c = new Tracked();
    var received = new List<Tracked>();

    using var sub = source.ObserveOnDisposingDropped(scheduler, x => x).Subscribe(received.Add);

    source.OnNext(a);
    source.OnNext(b);
    source.OnNext(c);

    // Nothing delivered until the scheduler runs the queued drain.
    Assert.Empty(received);

    scheduler.Start();

    Assert.Equal(new[] { a, b, c }, received);
    // Delivered items are the downstream's to dispose; the hop must not touch them.
    Assert.False(a.Disposed);
    Assert.False(b.Disposed);
    Assert.False(c.Disposed);
  }



  [Fact]
  public void DisposeWithQueuedItems_DisposesEveryQueuedItem() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var items = new[] { new Tracked(), new Tracked(), new Tracked() };
    var received = new List<Tracked>();

    var sub = source.ObserveOnDisposingDropped(scheduler, x => x).Subscribe(received.Add);

    foreach (var item in items) {
      source.OnNext(item);
    }

    // Do NOT advance the scheduler: every item is still queued in the hop.
    sub.Dispose();

    Assert.Empty(received);
    Assert.All(items, item => Assert.True(item.Disposed));

    // Running the scheduler afterwards must neither deliver nor double-dispose.
    scheduler.Start();
    Assert.Empty(received);
    Assert.All(items, item => Assert.Equal(1, item.DisposeCount));
  }



  /// <summary>
  ///   With a bound, a consumer that falls behind loses the OLDEST queued items, each released exactly once,
  ///   and keeps the newest in order - instead of holding one more item for every item it falls behind.
  /// </summary>
  [Fact]
  public void Bounded_OverflowReleasesTheOldest_AndKeepsTheNewestInOrder() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var items = new[] { new Tracked(), new Tracked(), new Tracked(), new Tracked(), new Tracked() };
    var received = new List<Tracked>();
    var overflows = 0;

    using var sub = source.ObserveOnDisposingDropped(scheduler, x => x, 3, () => overflows++)
                          .Subscribe(received.Add);

    foreach (var item in items) {
      source.OnNext(item);
    }

    scheduler.Start();

    Assert.Equal(2, overflows);
    Assert.Equal(new[] { items[2], items[3], items[4] }, received);
    Assert.Equal(1, items[0].DisposeCount);
    Assert.Equal(1, items[1].DisposeCount);
    Assert.All(received, item => Assert.False(item.Disposed));
  }



  [Fact]
  public void Bounded_WithinTheBound_IsLosslessAndNeverReportsOverflow() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var items = new[] { new Tracked(), new Tracked(), new Tracked() };
    var received = new List<Tracked>();
    var overflows = 0;

    using var sub = source.ObserveOnDisposingDropped(scheduler, x => x, 3, () => overflows++)
                          .Subscribe(received.Add);

    foreach (var item in items) {
      source.OnNext(item);
    }

    scheduler.Start();

    Assert.Equal(0, overflows);
    Assert.Equal(items, received);
  }



  [Fact]
  public void DeliveredItems_AreNotDisposedByTheHop() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var a = new Tracked();

    var sub = source.ObserveOnDisposingDropped(scheduler, x => x).Subscribe(_ => { });

    source.OnNext(a);
    scheduler.Start();     // deliver a
    sub.Dispose();         // teardown after delivery

    Assert.False(a.Disposed);
  }



  [Fact]
  public void ItemArrivingAfterCompletion_IsReleased() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var received = new List<Tracked>();

    using var sub = source.ObserveOnDisposingDropped(scheduler, x => x).Subscribe(received.Add);

    var a = new Tracked();
    source.OnNext(a);
    source.OnCompleted();
    scheduler.Start();

    Assert.Equal(new[] { a }, received);
    Assert.False(a.Disposed);
  }



  [Fact]
  public void UpstreamError_DeliversPendingItemsBeforeError() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var received = new List<Tracked>();
    Exception? error = null;

    using var sub = source.ObserveOnDisposingDropped(scheduler, x => x)
                          .Subscribe(received.Add, ex => error = ex);

    var a = new Tracked();
    var b = new Tracked();
    var boom = new InvalidOperationException("boom");
    source.OnNext(a);
    source.OnNext(b);
    source.OnError(boom);

    // Error is queued behind the pending items: the downstream Catch must still see a and b first.
    Assert.Null(error);
    Assert.Empty(received);

    scheduler.Start();

    Assert.Equal(new[] { a, b }, received);
    Assert.Same(boom, error);
    Assert.False(a.Disposed);
    Assert.False(b.Disposed);
  }



  [Fact]
  public void DropWhenBusy_ReleasesPendingItemOnUnsubscribe() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var dropped = new List<Tracked>();

    var sub = source.DropWhenBusy(scheduler, dropped.Add).Subscribe(_ => { });

    var a = new Tracked();
    source.OnNext(a);   // accepted and scheduled, but the scheduler has not run it yet

    // Tearing down before the scheduled delivery runs must report the in-flight item as dropped.
    sub.Dispose();

    Assert.Contains(a, dropped);

    // The now-cancelled scheduled delivery must not also fire.
    scheduler.Start();
    Assert.Single(dropped);
  }



  [Fact]
  public void Conflating_UnderBackpressure_DeliversOnlyLatest_AndReleasesDisplaced() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var a = new Tracked();
    var b = new Tracked();
    var c = new Tracked();
    var received = new List<Tracked>();

    using var sub = source.ObserveOnConflatingDropped(scheduler, x => x).Subscribe(received.Add);

    // Three arrivals while the scheduler is held: the slot conflates to the newest, releasing the older two.
    source.OnNext(a);
    source.OnNext(b);
    source.OnNext(c);

    Assert.Empty(received);
    Assert.True(a.Disposed);
    Assert.True(b.Disposed);
    Assert.False(c.Disposed);

    scheduler.Start();

    // Only the latest is delivered, exactly once, and the downstream owns it (not disposed by the hop).
    Assert.Equal(new[] { c }, received);
    Assert.False(c.Disposed);
    Assert.Equal(1, a.DisposeCount);
    Assert.Equal(1, b.DisposeCount);
  }



  [Fact]
  public void Conflating_DisposeWithPendingItem_ReleasesIt() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var a = new Tracked();
    var received = new List<Tracked>();

    var sub = source.ObserveOnConflatingDropped(scheduler, x => x).Subscribe(received.Add);

    source.OnNext(a);   // buffered in the single slot, scheduler not advanced

    sub.Dispose();

    Assert.Empty(received);
    Assert.Equal(1, a.DisposeCount);

    // Running the scheduler afterwards must neither deliver nor double-dispose.
    scheduler.Start();
    Assert.Empty(received);
    Assert.Equal(1, a.DisposeCount);
  }



  [Fact]
  public void Conflating_KeepsUp_WhenConsumerDrainsBetweenArrivals() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var received = new List<Tracked>();

    using var sub = source.ObserveOnConflatingDropped(scheduler, x => x).Subscribe(received.Add);

    // Each item is drained before the next arrives, so nothing is conflated: all are delivered in order.
    var a = new Tracked();
    source.OnNext(a);
    scheduler.Start();
    var b = new Tracked();
    source.OnNext(b);
    scheduler.Start();

    Assert.Equal(new[] { a, b }, received);
    Assert.False(a.Disposed);
    Assert.False(b.Disposed);
  }



  [Fact]
  public void Conflating_ItemArrivingAfterCompletion_IsReleased() {
    var scheduler = new TestScheduler();
    var source = new Subject<Tracked>();
    var received = new List<Tracked>();

    using var sub = source.ObserveOnConflatingDropped(scheduler, x => x).Subscribe(received.Add);

    var a = new Tracked();
    source.OnNext(a);
    source.OnCompleted();
    scheduler.Start();

    // The pending item is delivered before completion; the downstream owns it.
    Assert.Equal(new[] { a }, received);
    Assert.False(a.Disposed);
  }
}
