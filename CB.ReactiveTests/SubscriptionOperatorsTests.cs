namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Reactive.Linq;
using global::System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Xunit;



public class SubscriptionOperatorsTests {
  [Fact(Timeout = 5000)]
  public void SubscribeGuarded_WithAThrowingHandler_ExpectTheSubscriptionKeptAndTheErrorLogged() {
    var source = new Subject<int>();
    var logger = new ListLogger();
    var received = new List<int>();
    var failure = new InvalidOperationException("handler failed");

    using var subscription = source.SubscribeGuarded(
      value => {
        received.Add(value);
        if (value == 1) {
          throw failure;
        }
      },
      "Test.Context",
      logger
    );

    source.OnNext(1);
    source.OnNext(2);

    Assert.Equal([1, 2], received);
    var entry = Assert.Single(logger.Entries);
    Assert.Equal(LogLevel.Error, entry.Level);
    Assert.Same(failure, entry.Exception);
    Assert.Contains("Test.Context", entry.Message, StringComparison.Ordinal);
  }



  [Fact(Timeout = 5000)]
  public void SubscribeGuarded_WithAFaultingSource_ExpectTheFaultLoggedAndNothingThrown() {
    var source = new Subject<int>();
    var logger = new ListLogger();
    var failure = new InvalidOperationException("stream failed");
    using var subscription = source.SubscribeGuarded(_ => { }, "Test.Context", logger);

    source.OnError(failure);

    var entry = Assert.Single(logger.Entries);
    Assert.Equal(LogLevel.Error, entry.Level);
    Assert.Same(failure, entry.Exception);
  }



  [Fact(Timeout = 5000)]
  public void SubscribeGuarded_WithoutALogger_ExpectTheErrorSwallowed() {
    var source = new Subject<int>();
    using var subscription = source.SubscribeGuarded(_ => throw new InvalidOperationException(), "Test.Context");

    source.OnNext(1);
    source.OnNext(2);
  }



  [Fact(Timeout = 5000)]
  public void TrackSubscription_WithSubscribersComingAndGoing_ExpectHasObserversToFollow() {
    var source = new Subject<int>();
    var tracked = source.TrackSubscription(out var hasObservers);
    var states = new List<bool>();
    using var stateSubscription = hasObservers.Subscribe(states.Add);

    var first = tracked.Subscribe();
    var second = tracked.Subscribe();
    first.Dispose();
    second.Dispose();

    Assert.Equal([false, true, false], states);
  }



  [Fact(Timeout = 5000)]
  public void Share_WithTwoSubscribers_ExpectOneSubscriptionToTheSourceUntilTheLastLeaves() {
    var subscriptions = 0;
    var disposals = 0;
    var source = Observable.Create<int>(_ => {
      subscriptions++;
      return () => disposals++;
    }
    );
    var shared = source.Share();

    var first = shared.Subscribe();
    var second = shared.Subscribe();
    Assert.Equal(1, subscriptions);

    first.Dispose();
    Assert.Equal(0, disposals);

    second.Dispose();
    Assert.Equal(1, disposals);
  }



  [Fact(Timeout = 5000)]
  public void FireAndForget_WithDisposableItems_ExpectEachDisposedAfterTheObserverHasSeenIt() {
    var source = new Subject<Tracked>();
    var disposedWhenDelivered = new List<bool>();
    using var subscription = source.FireAndForget().Subscribe(item => disposedWhenDelivered.Add(item.IsDisposed));
    var tracked = new Tracked();

    source.OnNext(tracked);

    Assert.Equal([false], disposedWhenDelivered);
    Assert.True(tracked.IsDisposed);
  }



  [Fact(Timeout = 5000)]
  public void FireAndForget_WithASelector_ExpectWhatItReturnsDisposedAfterDelivery() {
    var source = new Subject<int>();
    var tracked = new Tracked();
    var disposedWhenDelivered = false;
    using var subscription = source.FireAndForget(_ => tracked).Subscribe(_ => disposedWhenDelivered = tracked.IsDisposed);

    source.OnNext(1);

    Assert.False(disposedWhenDelivered);
    Assert.True(tracked.IsDisposed);
  }



  [Fact(Timeout = 5000)]
  public void FireAndForget_WithAnObserverThatThrows_ExpectTheItemStillDisposed() {
    var source = new Subject<Tracked>();
    var tracked = new Tracked();
    using var subscription = source.FireAndForget().Subscribe(_ => throw new InvalidOperationException());

    try {
      source.OnNext(tracked);
    } catch (InvalidOperationException) {
      // The observer's exception is the observer's; what matters is the item.
    }

    Assert.True(tracked.IsDisposed);
  }



  [Fact(Timeout = 5000)]
  public void Trace_WithALogger_ExpectSubscribeValueAndCompletionLoggedUnderTheName() {
    var logger = new ListLogger();

    _ = Observable.Return(5).Trace("probe", logger).ToList().Wait();

    var messages = new List<string>();
    foreach (var entry in logger.Entries) {
      messages.Add(entry.Message);
    }

    Assert.Equal("probe#1: Subscribe()", messages[0]);
    Assert.Equal("probe#1: OnNext(5)", messages[1]);
    Assert.Equal("probe#1: OnCompleted()", messages[2]);
  }



  [Fact(Timeout = 5000)]
  public void Trace_WithTwoSubscriptions_ExpectEachItsOwnNumber() {
    var logger = new ListLogger();
    var traced = Observable.Never<int>().Trace("probe", logger);

    using var first = traced.Subscribe();
    using var second = traced.Subscribe();

    Assert.Contains(logger.Entries, e => e.Message == "probe#1: Subscribe()");
    Assert.Contains(logger.Entries, e => e.Message == "probe#2: Subscribe()");
  }



  [Fact(Timeout = 20000)]
  public void RatePerSecond_WithAnItemPerTick_ExpectTheCountOfTheLastSecond() {
    var rates = Observable.Range(0, 3).RatePerSecond().ToList().Wait();

    // Three items in well under a second: the rate climbs 1, 2, 3 per second.
    Assert.Equal([1d, 2d, 3d], rates);
  }



  private sealed class Tracked : IDisposable {
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
  }
}
