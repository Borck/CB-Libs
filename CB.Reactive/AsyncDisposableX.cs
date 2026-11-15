// ReSharper disable once CheckNamespace: deliberately in Rx's own namespace, so that no call site needs a using for these.
namespace System.Reactive.Disposables;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;



/// <summary>
///   Bridges <see cref="IAsyncDisposable" /> into Rx's synchronous <see cref="IDisposable" /> world: the async clean-up is
///   started when the synchronous disposable is disposed, not awaited, and its failure is logged rather than lost.
/// </summary>
public static partial class AsyncDisposableX {
  [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Async cleanup failed")]
  private static partial void LogAsyncCleanupFailed(ILogger logger, Exception? exception);



  [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Async dispose failed")]
  private static partial void LogAsyncDisposeFailed(ILogger logger, Exception? exception);



  /// <summary>
  ///   Adds an asynchronous clean-up to the composite: when the composite is disposed, <paramref name="asyncFunc" /> is started
  ///   (fire-and-forget) and a failure is logged.
  /// </summary>
  /// <param name="composite">The composite to add to.</param>
  /// <param name="asyncFunc">The clean-up to start on disposal.</param>
  /// <param name="logger">Receives a failure of the clean-up; it is swallowed without a trace if null.</param>
  public static void AddAsync(this CompositeDisposable composite, Func<Task> asyncFunc, ILogger? logger = null) {
    ArgumentNullException.ThrowIfNull(composite);
    ArgumentNullException.ThrowIfNull(asyncFunc);
    var log = logger ?? NullLogger.Instance;

    composite.Add(
      Disposable.Create(() =>
        _ = asyncFunc()
          .ContinueWith(
            t => {
              if (t.IsFaulted) {
                LogAsyncCleanupFailed(log, t.Exception);
              }
            },
            TaskScheduler.Default
          )
      )
    );
  }



  /// <summary>
  ///   Wraps an <see cref="IAsyncDisposable" /> as an <see cref="IDisposable" />: disposing it calls
  ///   <see cref="IAsyncDisposable.DisposeAsync" /> (fire-and-forget) and logs a failure.
  /// </summary>
  /// <param name="asyncDisposable">The async disposable to wrap.</param>
  /// <param name="logger">Receives a failure of the dispose; it is swallowed without a trace if null.</param>
  public static IDisposable AsDisposable(this IAsyncDisposable asyncDisposable, ILogger? logger = null) {
    ArgumentNullException.ThrowIfNull(asyncDisposable);
    var log = logger ?? NullLogger.Instance;

    return Disposable.Create(() =>
      _ = asyncDisposable
          .DisposeAsync()
          .AsTask()
          .ContinueWith(
            t => {
              if (t.IsFaulted) {
                LogAsyncDisposeFailed(log, t.Exception);
              }
            },
            TaskScheduler.Default
          )
    );
  }



  /// <summary>Adds an <see cref="IAsyncDisposable" /> to the composite; see <see cref="AsDisposable" />.</summary>
  /// <param name="composite">The composite to add to.</param>
  /// <param name="asyncDisposable">The async disposable to wrap.</param>
  /// <param name="logger">Receives a failure of the dispose; it is swallowed without a trace if null.</param>
  public static void AddAsync(this CompositeDisposable composite, IAsyncDisposable asyncDisposable, ILogger? logger = null) {
    ArgumentNullException.ThrowIfNull(composite);
    ArgumentNullException.ThrowIfNull(asyncDisposable);
    composite.Add(asyncDisposable.AsDisposable(logger));
  }



  /// <summary>Adds the async disposable, wrapped as a synchronous one, to a collection; returns the item.</summary>
  /// <param name="item">The async disposable.</param>
  /// <param name="compositeDisposable">The collection that disposes it later.</param>
  /// <param name="logger">Receives a failure of the dispose; it is swallowed without a trace if null.</param>
  public static T DisposeWithAsync<T>(this T item, ICollection<IDisposable> compositeDisposable, ILogger? logger = null)
    where T : IAsyncDisposable {
    ArgumentNullException.ThrowIfNull(compositeDisposable);
    compositeDisposable.Add(item.AsDisposable(logger));
    return item;
  }
}
