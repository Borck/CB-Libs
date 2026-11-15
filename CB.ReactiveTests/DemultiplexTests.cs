namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Reactive.Subjects;
using Xunit;



public class DemultiplexTests {
  [Fact(Timeout = 5000)]
  public void Demultiplex_WithAKeySelector_ExpectEachStreamToGetItsKeysItemsAndTheFallbackTheRest() {
    var source = new Subject<(string Key, int Value)>();
    using var demux = source.Demultiplex(item => item.Key);
    var a = new List<int>();
    var fallback = new List<int>();
    using var aSubscription = demux.GetStream("a").Subscribe(item => a.Add(item.Value));
    using var fallbackSubscription = demux.Fallback.Subscribe(item => fallback.Add(item.Value));

    source.OnNext(("a", 1));
    source.OnNext(("b", 2));
    source.OnNext(("a", 3));

    Assert.Equal([1, 3], a);
    Assert.Equal([2], fallback);
  }



  [Fact(Timeout = 5000)]
  public void Demultiplex_WithAStreamNobodyObserves_ExpectItsItemsToGoToTheFallback() {
    var source = new Subject<(string Key, int Value)>();
    using var demux = source.Demultiplex(item => item.Key);
    var fallback = new List<int>();
    _ = demux.GetStream("a");
    using var fallbackSubscription = demux.Fallback.Subscribe(item => fallback.Add(item.Value));

    source.OnNext(("a", 1));

    Assert.Equal([1], fallback);
  }



  [Fact(Timeout = 5000)]
  public void Demultiplex_WithAFailingKeySelector_ExpectTheErrorOnEveryStream() {
    var source = new Subject<int>();
    using var demux = source.Demultiplex(FailingKey);
    Exception? onStream = null;
    Exception? onFallback = null;
    using var streamSubscription = demux.GetStream(1).Subscribe(_ => { }, ex => onStream = ex);
    using var fallbackSubscription = demux.Fallback.Subscribe(_ => { }, ex => onFallback = ex);

    source.OnNext(7);

    Assert.IsType<InvalidOperationException>(onStream);
    Assert.IsType<InvalidOperationException>(onFallback);

    static int FailingKey(int _) => throw new InvalidOperationException("no key");
  }



  [Fact(Timeout = 5000)]
  public void Demultiplex_WithTheSourceCompleting_ExpectEveryStreamCompleted() {
    var source = new Subject<int>();
    using var demux = source.Demultiplex(item => item % 2);
    var streamCompleted = false;
    var fallbackCompleted = false;
    using var streamSubscription = demux.GetStream(0).Subscribe(_ => { }, () => streamCompleted = true);
    using var fallbackSubscription = demux.Fallback.Subscribe(_ => { }, () => fallbackCompleted = true);

    source.OnCompleted();

    Assert.True(streamCompleted);
    Assert.True(fallbackCompleted);
  }



  [Fact(Timeout = 5000)]
  public void DemultiplexByType_WithMixedItems_ExpectEachTypesStreamToGetItsItems() {
    var source = new Subject<object>();
    using var demux = source.Demultiplex();
    var numbers = new List<int>();
    var texts = new List<string>();
    using var numberSubscription = demux.GetStream<int>().Subscribe(numbers.Add);
    using var textSubscription = demux.GetStream<string>().Subscribe(texts.Add);

    source.OnNext(1);
    source.OnNext("a");
    source.OnNext(2);

    Assert.Equal([1, 2], numbers);
    Assert.Equal(["a"], texts);
  }
}
