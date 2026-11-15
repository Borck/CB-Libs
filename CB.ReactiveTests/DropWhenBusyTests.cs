namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using Xunit;



public class DropWhenBusyTests {
  [Fact(Timeout = 20000)]
  public void DropWhenBusy_WithAnItemWhileAnotherIsPending_ExpectTheLaterOneDropped() {
    var scheduler = new TestScheduler();
    var source = new Subject<int>();
    var delivered = new List<int>();
    var dropped = new List<int>();
    using var subscription = source.DropWhenBusy(scheduler, dropped.Add).Subscribe(delivered.Add);

    source.OnNext(1);
    source.OnNext(2);
    scheduler.AdvanceBy(1);

    Assert.Equal([1], delivered);
    Assert.Equal([2], dropped);
  }



  [Fact(Timeout = 20000)]
  public void DropWhenBusy_WithAnItemAfterTheLastWasDelivered_ExpectItDelivered() {
    var scheduler = new TestScheduler();
    var source = new Subject<int>();
    var delivered = new List<int>();
    using var subscription = source.DropWhenBusy(scheduler).Subscribe(delivered.Add);

    source.OnNext(1);
    scheduler.AdvanceBy(1);
    source.OnNext(2);
    scheduler.AdvanceBy(1);

    Assert.Equal([1, 2], delivered);
  }



  [Fact(Timeout = 20000)]
  public void DropWhenBusy_WithAnItemPendingWhenTheSubscriptionEnds_ExpectItReportedDroppedAndNotDelivered() {
    var scheduler = new TestScheduler();
    var source = new Subject<int>();
    var delivered = new List<int>();
    var dropped = new List<int>();
    var subscription = source.DropWhenBusy(scheduler, dropped.Add).Subscribe(delivered.Add);

    source.OnNext(1);
    subscription.Dispose();
    scheduler.AdvanceBy(1);

    Assert.Empty(delivered);
    Assert.Equal([1], dropped);
  }
}
