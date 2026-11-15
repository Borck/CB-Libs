namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Reactive.Disposables;
using global::System.Threading;
using global::System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;



public class AsyncDisposableXTests {
  [Fact(Timeout = 10000)]
  public async Task AddAsync_WithAnAsyncFunc_ExpectItStartedWhenTheCompositeIsDisposed() {
    var composite = new CompositeDisposable();
    var started = new TaskCompletionSource<bool>();
    composite.AddAsync(() => {
      started.SetResult(true);
      return Task.CompletedTask;
    }
    );

    Assert.False(started.Task.IsCompleted);
    composite.Dispose();

    await started.Task.WaitAsync(TestContext.Current.CancellationToken);
  }



  [Fact]
  public void AddAsync_WithAFailingAsyncFunc_ExpectTheFailureLoggedAndNotThrown() {
    var logger = new ListLogger();
    var composite = new CompositeDisposable();
    composite.AddAsync(() => Task.FromException(new InvalidOperationException("cleanup failed")), logger);

    composite.Dispose();

    Assert.True(SpinWait.SpinUntil(() => logger.Entries.Count == 1, TimeSpan.FromSeconds(5)));
    Assert.Equal(LogLevel.Error, logger.Entries.Single().Level);
  }



  [Fact(Timeout = 10000)]
  public async Task AsDisposable_WithAnAsyncDisposable_ExpectDisposeAsyncCalledOnDispose() {
    var asyncDisposable = new Recording();

    asyncDisposable.AsDisposable().Dispose();

    await asyncDisposable.Disposed.Task.WaitAsync(TestContext.Current.CancellationToken);
  }



  [Fact]
  public void AsDisposable_WithAFailingDispose_ExpectTheFailureLogged() {
    var logger = new ListLogger();

    new Recording { Failure = new InvalidOperationException("dispose failed") }.AsDisposable(logger).Dispose();

    Assert.True(SpinWait.SpinUntil(() => logger.Entries.Count == 1, TimeSpan.FromSeconds(5)));
    Assert.Equal(LogLevel.Error, logger.Entries.Single().Level);
  }



  [Fact(Timeout = 10000)]
  public async Task AddAsync_WithAnAsyncDisposable_ExpectItDisposedWithTheComposite() {
    var asyncDisposable = new Recording();
    var composite = new CompositeDisposable();
    composite.AddAsync(asyncDisposable);

    composite.Dispose();

    await asyncDisposable.Disposed.Task.WaitAsync(TestContext.Current.CancellationToken);
  }



  [Fact(Timeout = 10000)]
  public async Task DisposeWithAsync_WithACollection_ExpectTheItemReturnedAndDisposedWithTheCollection() {
    var asyncDisposable = new Recording();
    var collection = new List<IDisposable>();

    var returned = asyncDisposable.DisposeWithAsync(collection);

    Assert.Same(asyncDisposable, returned);
    foreach (var disposable in collection) {
      disposable.Dispose();
    }

    await asyncDisposable.Disposed.Task.WaitAsync(TestContext.Current.CancellationToken);
  }



  private sealed class Recording : IAsyncDisposable {
    public TaskCompletionSource<bool> Disposed { get; } = new();

    public Exception? Failure { get; init; }

    public ValueTask DisposeAsync() {
      Disposed.TrySetResult(true);
      return Failure is null ? default : new(Task.FromException(Failure));
    }
  }
}
