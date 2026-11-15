namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Reactive.Subjects;
using Xunit;



public class DisposePreviousTests {
  private sealed class TrackedDisposable : IDisposable {
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
  }



  /// <summary>
  ///   <c>DisposePrevious</c> releases the superseded item, the last one on completion, and the last one when
  ///   the subscription goes away. It used to gate all three on its <c>disposeOnException</c> flag, so the
  ///   default overload disposed nothing at all and every caller kept its predecessor alive and running
  ///   alongside the replacement.
  /// </summary>
  [Fact(Timeout = 5000)]
  public void DisposePrevious_ReleasesTheSupersededItem_AndTheLastOneOnTeardown() {
    var source = new Subject<TrackedDisposable>();
    var emitted = new List<TrackedDisposable>();
    var subscription = source.DisposePrevious().Subscribe(emitted.Add);

    var first = new TrackedDisposable();
    var second = new TrackedDisposable();

    source.OnNext(first);
    Assert.False(first.IsDisposed);

    source.OnNext(second);
    Assert.True(first.IsDisposed);
    Assert.False(second.IsDisposed);

    subscription.Dispose();
    Assert.True(second.IsDisposed);
    Assert.Equal([first, second], emitted);

    source.Dispose();
  }



  [Fact(Timeout = 5000)]
  public void DisposePrevious_ReleasesTheLastItem_WhenTheSourceCompletes() {
    var source = new Subject<TrackedDisposable>();
    using var subscription = source.DisposePrevious().Subscribe();

    var only = new TrackedDisposable();
    source.OnNext(only);
    source.OnCompleted();

    Assert.True(only.IsDisposed);
    source.Dispose();
  }
}
