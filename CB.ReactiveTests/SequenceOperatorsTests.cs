namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Reactive;
using global::System.Reactive.Disposables;
using global::System.Reactive.Linq;
using global::System.Reactive.Subjects;
using Xunit;



public class SequenceOperatorsTests {
  private static readonly int[] _rising = [1, 3, 2, 5];
  private static readonly int?[] _nullableValues = [1, null, 3];
  private static readonly bool[] _transitions = [false, true, true, false, true, false];
  private static readonly bool[] _pair = [true, false];
  private static readonly bool[] _mixedBooleans = [true, false, true, true, false];

  [Fact]
  public void PairWithPrevious_WithThreeValues_ExpectTwoPairs() {
    var pairs = Observable.Range(1, 3).PairWithPrevious().ToList().Wait();

    Assert.Equal([(1, 2), (2, 3)], pairs);
  }



  [Fact]
  public void PairWithPrevious_WithAColdSource_ExpectTheSourceSubscribedOnce() {
    var subscriptions = 0;
    var source = Observable.Defer(() => {
      subscriptions++;
      return Observable.Range(1, 3);
    }
    );

    var pairs = source.PairWithPrevious().ToList().Wait();

    Assert.Equal(2, pairs.Count);
    Assert.Equal(1, subscriptions);
  }



  [Fact]
  public void WhereConsecutive_WithAPredicateOnPreviousAndCurrent_ExpectTheCurrentOfMatchingPairs() {
    var rising = Observable.Range(0, 1).Concat(_rising.ToObservable()).WhereConsecutive((previous, current) => current > previous);

    Assert.Equal([1, 3, 5], rising.ToList().Wait());
  }



  [Fact]
  public void WhereConsecutive_WithAColdSource_ExpectTheSourceSubscribedOnce() {
    var subscriptions = 0;
    var source = Observable.Defer(() => {
      subscriptions++;
      return Observable.Range(1, 3);
    }
    );

    _ = source.WhereConsecutive((_, _) => true).ToList().Wait();

    Assert.Equal(1, subscriptions);
  }



  [Fact]
  public void TrackStatusFlags_WithSetsAndAReset_ExpectSnapshotsThatDoNotChangeLater() {
    var flags = new Subject<string>();
    var reset = new Subject<Unit>();
    var snapshots = new List<ISet<string>>();
    using var subscription = flags.TrackStatusFlags(reset).Subscribe(snapshots.Add);

    flags.OnNext("A");
    flags.OnNext("B");
    reset.OnNext(Unit.Default);
    flags.OnNext("C");

    Assert.Equal(4, snapshots.Count);
    Assert.Equal(["A"], snapshots[0].OrderBy(f => f));
    Assert.Equal(["A", "B"], snapshots[1].OrderBy(f => f));
    Assert.Empty(snapshots[2]);
    Assert.Equal(["C"], snapshots[3].OrderBy(f => f));
  }



  [Fact]
  public void DoOnPrevious_WithOnPreviousAndOnLast_ExpectEachValueReportedWhenItsSuccessorArrivesAndTheLastOnCompletion() {
    var seen = new List<string>();

    _ = Observable.Range(1, 3).DoOnPrevious(p => seen.Add($"previous {p}"), l => seen.Add($"last {l}")).ToList().Wait();

    Assert.Equal(["previous 1", "previous 2", "last 3"], seen);
  }



  [Fact]
  public void DoOnPrevious_WithASingleHandler_ExpectItUsedForPreviousAndLast() {
    var seen = new List<int>();

    _ = Observable.Range(1, 3).DoOnPrevious(seen.Add).ToList().Wait();

    Assert.Equal([1, 2, 3], seen);
  }



  [Fact]
  public void DoOnPrevious_WithAnEmptySource_ExpectNoHandlerCalled() {
    var calls = 0;

    _ = Observable.Empty<int>().DoOnPrevious(_ => calls++, _ => calls++).ToList().Wait();

    Assert.Equal(0, calls);
  }



  [Fact]
  public void DoOnPrevious_WithAnErrorHandler_ExpectTheLastValueAndTheException() {
    var failure = new InvalidOperationException("boom");
    var reported = new List<(Exception Error, int? Previous)>();
    var source = Observable.Return(7).Concat(Observable.Throw<int>(failure));

    var result = Assert.Throws<InvalidOperationException>(() =>
      source.DoOnPrevious(_ => { }, _ => { }, (error, previous) => reported.Add((error, previous))).ToList().Wait()
    );

    Assert.Same(failure, result);
    Assert.Equal([(failure, 7)], reported);
  }



  [Fact]
  public void WhereTrueAndWhereFalse_WithMixedValues_ExpectTheMatchingOnes() {
    var values = _mixedBooleans.ToObservable();

    Assert.Equal(3, values.WhereTrue().ToList().Wait().Count);
    Assert.Equal(2, values.WhereFalse().ToList().Wait().Count);
  }



  [Theory]
  [InlineData(BoolToCommandTrigger.FromFalseToTrue, 2)]
  [InlineData(BoolToCommandTrigger.FromTrueToFalse, 2)]
  [InlineData(BoolToCommandTrigger.FromAnyToTrue, 3)]
  [InlineData(BoolToCommandTrigger.FromAnyToFalse, 3)]
  [InlineData(BoolToCommandTrigger.Changed, 4)]
  [InlineData(BoolToCommandTrigger.Equal, 1)]
  public void ToCommand_WithTrigger_ExpectOneCommandPerMatchingTransition(BoolToCommandTrigger trigger, int expectedCommands) {
    // false -> true -> true -> false -> true -> false : pairs (f,t) (t,t) (t,f) (f,t) (t,f)
    var states = _transitions.ToObservable();

    Assert.Equal(expectedCommands, states.ToCommand(trigger).ToList().Wait().Count);
  }



  [Fact]
  public void ToCommand_WithAnUndefinedTrigger_ExpectNoCommand() =>
    Assert.Empty(_pair.ToObservable().ToCommand((BoolToCommandTrigger)99).ToList().Wait());



  [Fact]
  public void WhereHasValue_WithNullableValues_ExpectTheUnderlyingValuesOfTheNonNullOnes() {
    var values = _nullableValues.ToObservable();

    Assert.Equal([1, 3], values.WhereHasValue().ToList().Wait());
  }



  [Fact]
  public void WhereNotDisposed_WithASelector_ExpectOnlyItemsThatAreNotDisposed() {
    var alive = new Item();
    var gone = new Item { IsDisposed = true };

    var kept = new[] { alive, gone }.ToObservable().WhereNotDisposed(item => item).ToList().Wait();

    Assert.Equal([alive], kept);
  }



  [Fact]
  public void WhereNotDisposed_WithCancelableItems_ExpectOnlyItemsThatAreNotDisposed() {
    var alive = new BooleanDisposable();
    var gone = new BooleanDisposable();
    gone.Dispose();

    Assert.Equal([alive], new[] { alive, gone }.ToObservable().WhereNotDisposed().ToList().Wait());
  }



  [Fact]
  public void WhereNotNullAndNotDisposed_WithASelector_ExpectNeitherNullNorDisposedItems() {
    var alive = new Item();
    var gone = new Item { IsDisposed = true };

    var kept = new Item?[] { null, gone, alive }.ToObservable().WhereNotNullAndNotDisposed(item => item).ToList().Wait();

    Assert.Equal([alive], kept);
  }



  [Fact]
  public void WhereNotNullAndNotDisposed_WithCancelableItems_ExpectNeitherNullNorDisposedItems() {
    var alive = new BooleanDisposable();
    var gone = new BooleanDisposable();
    gone.Dispose();

    var kept = new BooleanDisposable?[] { null, gone, alive }.ToObservable().WhereNotNullAndNotDisposed().ToList().Wait();

    Assert.Equal([alive], kept);
  }



  [Fact]
  public void ThrowIfDisposed_WithADisposedItem_ExpectObjectDisposedExceptionWithTheMessage() {
    var gone = new BooleanDisposable();
    gone.Dispose();
    Exception? error = null;

    using var subscription = Observable.Return(gone).ThrowIfDisposed("the item is gone").Subscribe(_ => { }, ex => error = ex);

    var exception = Assert.IsType<ObjectDisposedException>(error);
    Assert.Contains("the item is gone", exception.Message, StringComparison.Ordinal);
  }



  [Fact]
  public void ThrowIfDisposed_WithASelectorAndLiveItems_ExpectTheItemsUnchanged() {
    var items = new[] { new Item(), new Item() };

    Assert.Equal(items, items.ToObservable().ThrowIfDisposed(item => item).ToList().Wait());
  }



  [Fact]
  public void ThrowIfDisposed_WithASelectorAndADisposedItem_ExpectObjectDisposedException() {
    var gone = new Item { IsDisposed = true };

    Assert.Throws<ObjectDisposedException>(() => Observable.Return(gone).ThrowIfDisposed(item => item).ToList().Wait());
  }



  private sealed class Item : ICancelable {
    public bool IsDisposed { get; set; }

    public void Dispose() => IsDisposed = true;
  }
}
