namespace CB.Reactive;

using CB.System.Diagnostics;
using global::System;
using global::System.Collections.Generic;
using global::System.Diagnostics.CodeAnalysis;
using global::System.Linq;
using global::System.Reactive;
using global::System.Reactive.Concurrency;
using global::System.Reactive.Disposables;
using global::System.Reactive.Linq;
using global::System.Reactive.Subjects;
using global::System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;



[SuppressMessage(
  "Naming",
  "CA1708:Identifiers should differ by more than case",
  Justification =
    "C# 14 extension blocks. Every `extension(...)` container the compiler generates for this class is " +
    "named \"extension\", so CA1708 reports the class as soon as it declares more than one - the colliding " +
    "names are not spelled anywhere in the source, and the only code-level alternative is one static class " +
    "per extension block. Still present in Microsoft.CodeAnalysis.NetAnalyzers 9.0.0 and 10.0.0."
)]
public static partial class ObservableX {
  #region static fields and constants

  /// <summary>
  ///   How many items the scheduler hops (<see cref="ObserveOnDisposingDropped{T}" /> /
  ///   <see cref="ObserveOnConflatingDropped{T}" />) deliver per scheduled work item before yielding the
  ///   thread back to the pool.
  /// </summary>
  /// <remarks>
  ///   The trade is dispatch count against fairness. Draining one item per dispatch - the original shape -
  ///   is maximally fair and maximally expensive: every flow connection performs this hop, so a record
  ///   crossing N edges paid N thread-pool dispatches. Draining without a bound would remove all of them
  ///   but let one busy edge hold a pool thread for as long as its producer keeps up. 64 is deliberately
  ///   well above any realistic per-tick burst on a frame edge (which conflates to one item anyway) while
  ///   still bounding how long a single work item can run.
  /// </remarks>
  private const int _maxDrainBatch = 64;

  #endregion

  #region logging

  [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "{Action} failed (attempt {Attempt}); retrying in {Delay}.")]
  private static partial void LogRetrying(ILogger logger, Exception exception, string action, int attempt, TimeSpan delay);



  [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Warning,
    Message = "{Action} failed (attempt {Attempt}/{MaxAttempts}); retrying in {Delay}."
  )]
  private static partial void LogRetryingBlocking(ILogger logger,
                                                  Exception exception,
                                                  string action,
                                                  int attempt,
                                                  int maxAttempts,
                                                  TimeSpan delay);



  [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Error,
    Message = "Unhandled error in {Context}; the value was dropped but the subscription is kept"
  )]
  private static partial void LogGuardedHandlerFailed(ILogger logger, Exception exception, string context);



  [LoggerMessage(
    EventId = 4,
    Level = LogLevel.Error,
    Message = "Control stream {Context} faulted; it will no longer receive values"
  )]
  private static partial void LogGuardedStreamFaulted(ILogger logger, Exception exception, string context);



  [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "{Name}#{Id}: {Event}({Value})")]
  private static partial void LogTrace(ILogger logger, string name, int id, string @event, object? value);

  #endregion

  #region methods and others

  /// <summary>
  ///   Creates an observable sequence that periodically polls a function and emits its results.
  /// </summary>
  /// <typeparam name="T">The type of values emitted by the observable.</typeparam>
  /// <param name="pollFunction">The function to poll for values.</param>
  /// <param name="period">The time interval between polls.</param>
  /// <param name="scheduler">The scheduler used for timing.</param>
  /// <returns>An observable sequence that emits values returned by the poll function.</returns>
  public static IObservable<T> Poll<T>(Func<T> pollFunction, TimeSpan period, IScheduler scheduler) =>
    Observable.Interval(period, scheduler)
              .Select(_ => pollFunction())
              .Retry() //Loop on errors
              .Repeat(); //Loop on success



  /// <summary>
  ///   Creates an observable sequence that periodically polls a function and emits its results.
  /// </summary>
  /// <typeparam name="T">The type of values emitted by the observable.</typeparam>
  /// <param name="pollFunction">The function to poll for values.</param>
  /// <param name="period">The time interval between polls.</param>
  /// <returns>An observable sequence that emits values returned by the poll function.</returns>
  public static IObservable<T> Poll<T>(Func<T> pollFunction, TimeSpan period) =>
    Observable.Interval(period)
              .Select(_ => pollFunction())
              .Retry() //Loop on errors
              .Repeat(); //Loop on success



  /// <summary>
  ///   Builds a per-subscription retry-with-backoff handler for <c>RetryWhen</c>: logs each failure and waits an
  ///   exponentially increasing (capped) delay before resubscribing, instead of a bare <c>.Retry()</c> spin.
  /// </summary>
  /// <param name="action">A short description of the action being retried, used for logging.</param>
  /// <param name="initialDelay">The delay before the first retry.</param>
  /// <param name="maxDelay">The upper bound the exponential back-off is capped to.</param>
  /// <param name="logger">Receives a warning per failure; none if null.</param>
  public static Func<IObservable<Exception>, IObservable<long>> RetryWithBackoff(
    string action,
    TimeSpan initialDelay,
    TimeSpan maxDelay,
    ILogger? logger = null) {
    var log = logger ?? NullLogger.Instance;
    var attempt = 0;
    return errors => errors.SelectMany(ex => {
      attempt++;
      var delay = TimeSpan.FromTicks(
        Math.Min(initialDelay.Ticks * (1L << Math.Min(attempt - 1, 20)), maxDelay.Ticks)
      );
      LogRetrying(log, ex, action, attempt, delay);
      return Observable.Timer(delay);
    }
    );
  }



  /// <summary>
  ///   Synchronously invokes <paramref name="action" />, retrying with an exponentially increasing (capped) delay
  ///   on failure instead of letting the first transient throw (e.g. a still-busy device on re-enable) fault the
  ///   caller. Throws the last exception once <paramref name="maxAttempts" /> is exhausted.
  /// </summary>
  /// <param name="action">The blocking action to invoke, e.g. starting a hardware device.</param>
  /// <param name="actionName">A short description of the action being retried, used for logging.</param>
  /// <param name="maxAttempts">The maximum number of attempts, including the first one.</param>
  /// <param name="initialDelay">The delay before the first retry.</param>
  /// <param name="maxDelay">The upper bound the exponential back-off is capped to.</param>
  /// <param name="logger">Receives a warning per failed attempt; none if null.</param>
  public static void RetryWithBackoff(Action action,
                                      string actionName,
                                      int maxAttempts = 3,
                                      TimeSpan? initialDelay = null,
                                      TimeSpan? maxDelay = null,
                                      ILogger? logger = null) {
    var log = logger ?? NullLogger.Instance;
    var delay = initialDelay ?? TimeSpan.FromMilliseconds(250);
    var cap = maxDelay ?? TimeSpan.FromSeconds(5);
    for (var attempt = 1; ; attempt++) {
      try {
        action();
        return;
      } catch (Exception ex) when (attempt < maxAttempts) {
        LogRetryingBlocking(log, ex, actionName, attempt, maxAttempts, delay);
        Thread.Sleep(delay);
        delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, cap.Ticks));
      }
    }
  }

  #endregion

  #region nested types

  /// <param name="source">The source observable sequence.</param>
  /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
  extension<T>(IObservable<T> source) {
    #region methods and others

    /// <summary>
    ///   Subscribes to a <b>control</b> sequence so that neither a failing handler nor a failing source can
    ///   silently switch the control path off.
    /// </summary>
    /// <remarks>
    ///   Rx's <c>AutoDetachObserver</c> disposes a subscription whose <c>OnNext</c> handler throws and then
    ///   rethrows, so the exception also unwinds into — and detaches — whatever subscriptions sit above it on
    ///   the synchronous call stack. For a control stream (a graph definition, an enabled flag, a lifecycle
    ///   event) that is fatal and invisible: the command still runs, its <c>CanExecute</c> is still true, and
    ///   nothing happens ever again. Saving a flow while frames are in flight is the live instance — a single
    ///   <see cref="ObjectDisposedException" /> out of a frame borrow during graph teardown used to leave the
    ///   flow permanently unable to be saved, enabled or disabled.
    ///   <para>
    ///     Use this for control streams only. Data streams must keep propagating their errors so the pipeline's
    ///     <c>Catch</c> / fault reporting can attribute them to a node.
    ///   </para>
    /// </remarks>
    /// <param name="onNext">The handler; exceptions it throws are logged and swallowed.</param>
    /// <param name="context">A short description used in the log messages, e.g. "FlowGraph.ReloadGraph".</param>
    /// <param name="logger">Receives the errors; they are swallowed without a trace if null.</param>
    public IDisposable SubscribeGuarded(Action<T> onNext, string context, ILogger? logger = null) {
      var log = logger ?? NullLogger.Instance;
      return source.Subscribe(
        item => {
          try {
            onNext(item);
          } catch (Exception ex) {
            LogGuardedHandlerFailed(log, ex, context);
          }
        },
        ex => LogGuardedStreamFaulted(log, ex, context)
      );
    }



    /// <summary>
    ///   Wraps the source sequence to track whether it has active subscribers.
    /// </summary>
    /// <param name="hasObservers">
    ///   An output observable that emits true when the first subscriber connects,
    ///   and false when the last subscriber disconnects.
    /// </param>
    /// <returns>The source observable wrapped with tracking logic.</returns>
    public IObservable<T> TrackSubscription(out IObservable<bool> hasObservers) {
      BehaviorSubject<bool> stateSubject = new(false);
      var subscriberCount = 0;

      var trackedSource = Observable.Create<T>(observer => {
        var count = Interlocked.Increment(ref subscriberCount);

        if (count == 1) {
          stateSubject.OnNext(true);
        }

        var subscription = source.Subscribe(observer);

        return Disposable.Create(() => {
          subscription.Dispose();
          var remaining = Interlocked.Decrement(ref subscriberCount);
          if (remaining == 0) {
            stateSubject.OnNext(false);
          }
        }
        );
      }
      );
      hasObservers = stateSubject.AsObservable().DistinctUntilChanged();
      return trackedSource;
    }



    /// <summary>
    ///   Transforms an observable sequence into a sequence of tuples containing each element and its immediate predecessor.
    ///   The resulting sequence skips the first element since it has no predecessor.
    /// </summary>
    /// <returns>
    ///   An observable sequence of tuples, where each tuple contains the previous and current elements
    ///   from the source sequence.
    /// </returns>
    public IObservable<(T Previous, T Current)> PairWithPrevious() =>
      // One subscription to the source, shared by both sides of the Zip: `source.Zip(source.Skip(1), ...)` subscribes twice,
      // which runs a cold source (a query, a device read) twice and lets the two copies drift apart.
      source.Publish(shared => shared.Zip(shared.Skip(1), (prev, curr) => (prev, curr)));



    /// <summary>
    ///   Filters the observable sequence by applying a predicate to each pair of consecutive values.
    ///   Only elements for which the predicate returns true are emitted.
    /// </summary>
    /// <param name="predicate">
    ///   A function that takes the previous and current elements and returns true if the current element should be emitted.
    /// </param>
    /// <returns>
    ///   An observable sequence that emits elements from the source sequence for which the predicate applied to the previous
    ///   and current elements returns true.
    /// </returns>
    public IObservable<T> WhereConsecutive(Func<T, T, bool> predicate) =>
      source.PairWithPrevious()
            .Where(pair => predicate(pair.Previous, pair.Current))
            .Select(pair => pair.Current);



    /// <summary>
    ///   Traces the events of an observable sequence with a specified name.
    ///   Logs subscription, unsubscription, and notifications (OnNext, OnError, OnCompleted)
    ///   to the default logger with a unique identifier for each subscription.
    /// </summary>
    /// <param name="name">The name to include in the log messages.</param>
    /// <param name="logger">Receives the trace at debug level; nothing is logged if null.</param>
    /// <returns>An observable sequence that is identical to the source sequence but with tracing enabled.</returns>
    public IObservable<T> Trace(string name, ILogger? logger = null) {
      var log = logger ?? NullLogger.Instance;
      var id = 0;
      return Observable.Create<T>(observer => {
        var id1 = Interlocked.Increment(ref id);

        void Debug(string m, object? v) => LogTrace(log, name, id1, m, v);

        Debug("Subscribe", "");
        var disposable = source.Subscribe(
          v => {
            Debug("OnNext", v);
            observer.OnNext(v);
          },
          e => {
            Debug("OnError", "");
            observer.OnError(e);
          },
          () => {
            Debug("OnCompleted", "");
            observer.OnCompleted();
          }
        );
        return () => {
          Debug("Dispose", "");
          disposable.Dispose();
        };
      }
      );
    }



    /// <summary>
    ///   For each incoming item, emits how many items arrived in the last second (e.g. frames per second), when that
    ///   changed.
    /// </summary>
    public IObservable<double> RatePerSecond() =>
      Observable.Defer(() => {
        var counter = new SlidingWindowRateCounter(TimeSpan.FromSeconds(1));
        return source
               // Every frame arrival, tick the counter and read out the last full‐sec rate
               .Select(_ => counter.Tick())
               // Optional: only push when the rate actually changes
               .DistinctUntilChanged();
      }
      );



    public IObservable<ISet<T>> TrackStatusFlags<TReset>(IObservable<TReset> reset) =>
      // Per subscription: the set a subscriber builds is its own, and what it is handed is a snapshot that nothing mutates
      // later. (The accumulator of a Scan was handed out as the snapshot, so the next flag changed the one already emitted,
      // and the seed was created once per call, so every subscriber shared it.)
      Observable.Defer(() => {
        var flags = new HashSet<T>();
        // The flag of a reset is never read (it clears the set): default(T) is a placeholder, and the tuple keeps the
        // non-nullable T of the "set" branch it is merged with.
        return reset.Select(_ => (Set: false, Flag: default(T)!))
                    .Merge(source.Select(flag => (Set: true, Flag: flag)))
                    .Select(operation => {
                      if (operation.Set) {
                        flags.Add(operation.Flag!);
                      } else {
                        flags.Clear();
                      }

                      return (ISet<T>)new HashSet<T>(flags);
                    }
                    );
      }
      );



    /// <summary>
    ///   Ensures that any item emitted by the source is disposed immediately after
    ///   it has been processed by the downstream observer. It is like FireAndForget, but for observables it optional
    ///   disposable items.
    /// </summary>
    /// <returns>A cold observable that disposes elements after emission.</returns>
    public IObservable<T> FireAndForget(Func<T, IDisposable?> disposableSelector) =>
      Observable.Create<T>(observer => source.Subscribe(
                    item => {
                      try {
                        observer.OnNext(item);
                      } finally {
                        disposableSelector(item)?.Dispose();
                      }
                    },
                    observer.OnError,
                    observer.OnCompleted
                  )
                )
                .Share();



    public IObservable<T> ThrowIfDisposed(Func<T, ICancelable?> selector, string? message = null) =>
      source.Select(item =>
        selector(item)?.IsDisposed == true ? throw new ObjectDisposedException(nameof(item), message) : item
      );



    public IObservable<T> WhereNotDisposed(Func<T, ICancelable?> selector) =>
      source.Where(item => selector(item)?.IsDisposed != true);



    /// <summary>
    ///   Drops any new values if the downstream is still processing the previous one.
    /// </summary>
    /// <param name="scheduler">The scheduler on which accepted items are delivered.</param>
    /// <param name="onDropped">Optional action invoked for each item that is dropped.</param>
    public IObservable<T> DropWhenBusy(IScheduler scheduler, Action<T>? onDropped = null) =>
      Observable.Create<T>(observer => {
        var gate = new object();
        var busy = false;
        var hasPending = false;
        var disposed = false;
        T? pendingItem = default;

        // We need to track both the upstream subscription and any scheduled disposables
        var cd = new CompositeDisposable();

        // Subscribe to the source
        var subscription = source.Subscribe(
          item => {
            bool scheduleThis;
            lock (gate) {
              scheduleThis = !busy;
              if (scheduleThis) {
                busy = true;
                hasPending = true;
                pendingItem = item;
              }
            }

            if (scheduleThis) {
              // Schedule the delivery on the provided scheduler
              var scheduled = scheduler.Schedule(
                item,
                (sched, x) => {
                  lock (gate) {
                    // Teardown may have already claimed this item as dropped; if so, do not deliver.
                    if (disposed) {
                      return Disposable.Empty;
                    }

                    hasPending = false;
                    pendingItem = default;
                  }

                  observer.OnNext(x);
                  lock (gate) {
                    busy = false;
                  }

                  return Disposable.Empty;
                }
              );

              cd.Add(scheduled);
            } else {
              onDropped?.Invoke(item);
            }
          },
          observer.OnError,
          observer.OnCompleted
        );

        cd.Add(subscription);
        // A scheduled-but-not-yet-delivered item is cancelled when cd is disposed. Report it as
        // dropped on teardown (guarded so the scheduled action never double-reports) - otherwise the
        // one in-flight item leaks when onDropped is the resource-releasing action.
        cd.Add(Disposable.Create(() => {
          T pending;
          bool drop;
          lock (gate) {
            disposed = true;
            drop = hasPending;
            hasPending = false;
            pending = pendingItem!;
            pendingItem = default;
          }

          if (drop) {
            onDropped?.Invoke(pending);
          }
        }
        ));
        return cd;
      }
      );



    /// <summary>
    ///   Like <c>ObserveOn</c>, but never strands items on teardown: any item still queued when the
    ///   subscription is disposed - pipeline teardown, a <c>Switch()</c> collapse, a <c>Catch</c> swap -
    ///   is released via <paramref name="dropDisposable" /> instead of being silently dropped, and an item
    ///   that arrives after disposal is released immediately. In steady state it is a lossless, ordered
    ///   scheduler hop identical to <c>ObserveOn</c>: delivered items are NOT touched (the downstream owns
    ///   them), and terminal notifications are delivered only after every pending item, so a downstream
    ///   <c>Catch</c> still sees the items that preceded the error.
    /// </summary>
    /// <remarks>
    ///   Used by <c>FlowConnection</c> for the frame-carrying scheduler hop: a borrowed <see cref="IFrame" />
    ///   record queued on the data scheduler when the connection is disabled/disposed is released here rather
    ///   than leaked. Each item is dequeued-and-delivered or dequeued-and-disposed exactly once.
    /// </remarks>
    /// <param name="scheduler">The scheduler on which items are delivered.</param>
    /// <param name="dropDisposable">
    ///   Maps an undelivered item to the resource to release when it is dropped (return <c>null</c> for items
    ///   that own nothing).
    /// </param>
    public IObservable<T> ObserveOnDisposingDropped(IScheduler scheduler, Func<T, IDisposable?> dropDisposable) =>
      source.ObserveOnDisposingDropped(scheduler, dropDisposable, int.MaxValue);



    /// <summary>
    ///   <see cref="ObserveOnDisposingDropped{T}(IScheduler, Func{T, IDisposable?})" /> with an upper bound on the
    ///   queue: once <paramref name="maxQueued" /> items are waiting, the oldest is released through
    ///   <paramref name="dropDisposable" /> to make room for the newcomer, and <paramref name="onOverflow" /> is
    ///   called (outside the hop's lock) for every item displaced that way.
    /// </summary>
    /// <remarks>
    ///   In order and lossless while the consumer keeps up on average - a burst of up to
    ///   <paramref name="maxQueued" /> items is absorbed - and bounded when it does not. Without a bound, a
    ///   consumer that is permanently slower than its producer (a chessboard detection or a video encoder behind a
    ///   30 fps camera) held one more borrowed frame for every frame it fell behind, so memory and latency grew
    ///   for as long as the flow ran.
    /// </remarks>
    public IObservable<T> ObserveOnDisposingDropped(IScheduler scheduler,
                                                    Func<T, IDisposable?> dropDisposable,
                                                    int maxQueued,
                                                    Action? onOverflow = null) {
      ArgumentNullException.ThrowIfNull(scheduler);
      ArgumentNullException.ThrowIfNull(dropDisposable);
      ArgumentOutOfRangeException.ThrowIfLessThan(maxQueued, 1);
      return Observable.Create<T>(observer => {
        var gate = new object();
        var queue = new Queue<T>();
        var drainScheduled = false;
        var disposed = false;
        var terminated = false;
        Exception? error = null;
        var drain = new SerialDisposable();

        void DisposeQueuedLocked() {
          while (queue.Count > 0) {
            dropDisposable(queue.Dequeue())?.Dispose();
          }
        }

        void ScheduleDrain() {
          lock (gate) {
            if (drainScheduled || disposed) {
              return;
            }

            drainScheduled = true;
          }

          drain.Disposable = scheduler.Schedule(self => {
            // Drain a batch per scheduled work item rather than re-scheduling after every single one.
            // Each `self()` is a whole thread-pool dispatch, and this hop runs on EVERY flow connection,
            // so per-item scheduling made a chain of N nodes cost N dispatches per record - the dominant
            // term in a deep pipeline's latency, and pure overhead once more than one item is waiting.
            var deliveredInBatch = 0;

            while (true) {
              T item;
              bool deliverError;
              bool deliverCompleted;
              Exception? err;
              lock (gate) {
                if (disposed) {
                  drainScheduled = false;
                  DisposeQueuedLocked();
                  return;
                }

                if (queue.Count > 0) {
                  item = queue.Dequeue();
                  deliverError = false;
                  deliverCompleted = false;
                  err = null;
                } else {
                  drainScheduled = false;
                  if (!terminated) {
                    return;
                  }

                  item = default!;
                  err = error;
                  deliverError = error != null;
                  deliverCompleted = error == null;
                }
              }

              if (deliverError) {
                observer.OnError(err!);
                return;
              }

              if (deliverCompleted) {
                observer.OnCompleted();
                return;
              }

              observer.OnNext(item);

              // Yield the thread back to the pool periodically. Without this bound a producer that keeps
              // the queue non-empty would hold one pool thread indefinitely, starving every other hop -
              // batching is meant to remove dispatches, not to monopolise a thread.
              if (++deliveredInBatch >= _maxDrainBatch) {
                self();
                return;
              }
            }
          }
          );
        }

        var subscription = source.Subscribe(
          item => {
            var overflowed = false;
            lock (gate) {
              if (disposed || terminated) {
                dropDisposable(item)?.Dispose();
                return;
              }

              if (queue.Count >= maxQueued) {
                dropDisposable(queue.Dequeue())?.Dispose();
                overflowed = true;
              }

              queue.Enqueue(item);
            }

            if (overflowed) {
              onOverflow?.Invoke();
            }

            ScheduleDrain();
          },
          ex => {
            lock (gate) {
              if (disposed || terminated) {
                return;
              }

              terminated = true;
              error = ex;
            }

            ScheduleDrain();
          },
          () => {
            lock (gate) {
              if (disposed || terminated) {
                return;
              }

              terminated = true;
            }

            ScheduleDrain();
          }
        );

        return new CompositeDisposable(
          subscription,
          drain,
          Disposable.Create(() => {
            lock (gate) {
              disposed = true;
              DisposeQueuedLocked();
            }
          }
          )
        );
      }
      );
    }



    /// <summary>
    ///   Like <see cref="ObserveOnDisposingDropped{T}" />, but conflating: it buffers at most one pending item
    ///   between the source and the scheduler. When a new item arrives while an earlier one is still waiting to
    ///   be delivered, the earlier (staler) item is dropped and released via <paramref name="dropDisposable" />
    ///   and the newer item takes its place, so the downstream always receives the freshest item and the
    ///   end-to-end latency stays bounded to a single in-flight item instead of growing with an unbounded
    ///   backlog. As with <c>ObserveOnDisposingDropped</c>, a pending item still buffered on teardown is
    ///   released rather than leaked, an item arriving after disposal is released immediately, and each item is
    ///   either delivered or released exactly once. Delivered items are NOT touched (the downstream owns them);
    ///   terminal notifications are delivered only after the pending item, so a downstream <c>Catch</c> still
    ///   sees the item that preceded the error.
    /// </summary>
    /// <remarks>
    ///   Used by <c>FlowConnection</c> for frame-carrying edges into latest-only inputs: under sustained
    ///   overload (a downstream node slower than the source frame rate) the connection delivers the newest
    ///   frame and releases the borrow of every displaced frame, keeping the pipeline latency bounded. Not to
    ///   be confused with <see cref="DropWhenBusy{T}" />, which drops the <em>newcomer</em> and keeps the older
    ///   accepted item - the wrong freshness semantics for a latency-bounded hop.
    /// </remarks>
    /// <param name="scheduler">The scheduler on which the retained item is delivered.</param>
    /// <param name="dropDisposable">
    ///   Maps a dropped item to the resource to release when it is displaced or stranded on teardown (return
    ///   <c>null</c> for items that own nothing).
    /// </param>
    public IObservable<T> ObserveOnConflatingDropped(IScheduler scheduler, Func<T, IDisposable?> dropDisposable) {
      ArgumentNullException.ThrowIfNull(scheduler);
      ArgumentNullException.ThrowIfNull(dropDisposable);
      return Observable.Create<T>(observer => {
        var gate = new object();
        var hasPending = false;
        T? pending = default;
        var drainScheduled = false;
        var disposed = false;
        var terminated = false;
        Exception? error = null;
        var drain = new SerialDisposable();

        void DropPendingLocked() {
          if (!hasPending) {
            return;
          }

          var stale = pending!;
          pending = default;
          hasPending = false;
          dropDisposable(stale)?.Dispose();
        }

        void ScheduleDrain() {
          lock (gate) {
            if (drainScheduled || disposed) {
              return;
            }

            drainScheduled = true;
          }

          drain.Disposable = scheduler.Schedule(self => {
            // Looped for the same reason as the lossless hop, though the win here is different: this hop
            // holds at most one item, so the old shape cost TWO dispatches per record - one to deliver it
            // and one more to discover there was nothing left. The loop settles that second question
            // without leaving the thread.
            var deliveredInBatch = 0;

            while (true) {
              T item;
              bool deliverError;
              bool deliverCompleted;
              Exception? err;
              lock (gate) {
                if (disposed) {
                  drainScheduled = false;
                  DropPendingLocked();
                  return;
                }

                if (hasPending) {
                  item = pending!;
                  pending = default;
                  hasPending = false;
                  deliverError = false;
                  deliverCompleted = false;
                  err = null;
                } else {
                  drainScheduled = false;
                  if (!terminated) {
                    return;
                  }

                  item = default!;
                  err = error;
                  deliverError = error != null;
                  deliverCompleted = error == null;
                }
              }

              if (deliverError) {
                observer.OnError(err!);
                return;
              }

              if (deliverCompleted) {
                observer.OnCompleted();
                return;
              }

              observer.OnNext(item);

              if (++deliveredInBatch >= _maxDrainBatch) {
                self();
                return;
              }
            }
          }
          );
        }

        var subscription = source.Subscribe(
          item => {
            lock (gate) {
              if (disposed || terminated) {
                dropDisposable(item)?.Dispose();
                return;
              }

              // Conflate: displace and release the staler pending item, keeping only the newest.
              DropPendingLocked();
              pending = item;
              hasPending = true;
            }

            ScheduleDrain();
          },
          ex => {
            lock (gate) {
              if (disposed || terminated) {
                return;
              }

              terminated = true;
              error = ex;
            }

            ScheduleDrain();
          },
          () => {
            lock (gate) {
              if (disposed || terminated) {
                return;
              }

              terminated = true;
            }

            ScheduleDrain();
          }
        );

        return new CompositeDisposable(
          subscription,
          drain,
          Disposable.Create(() => {
            lock (gate) {
              disposed = true;
              DropPendingLocked();
            }
          }
          )
        );
      }
      );
    }



    /// <summary>
    ///   Applies an action on the previous item in the sequence before emitting the current item.
    /// </summary>
    /// <param name="onPrevious">The action to apply on the previous item.</param>
    /// <param name="onLast">The action to apply on the last item.</param>
    /// <param name="onError">The action </param>
    /// <returns>An observable sequence with the action applied on the previous item.</returns>
    public IObservable<T> DoOnPrevious(Action<T> onPrevious,
                                       Action<T> onLast,
                                       Action<Exception, T?> onError) {
      ArgumentNullException.ThrowIfNull(onPrevious);
      ArgumentNullException.ThrowIfNull(onLast);
      ArgumentNullException.ThrowIfNull(onError);
      return Observable.Create<T>(observer => {
        T? previous = default;
        var valueReceived = false;
        return source
               .Do(
                 current => {
                   if (valueReceived) {
                     onPrevious(previous!);
                   } else {
                     valueReceived = true;
                   }

                   previous = current;
                 },
                 ex => onError(ex, previous),
                 () => {
                   if (valueReceived) {
                     onLast(previous!);
                   }
                 }
               )
               .Subscribe(observer);
      }
      );
    }



    /// <summary>
    ///   Applies an action on the previous item in the sequence before emitting the current item.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="onPrevious">The action to apply on the previous item.</param>
    /// <param name="onLast">The action to apply on the last item in the sequence.</param>
    /// <param name="onError">The action to apply in case of an error, optionally providing the last processed item.</param>
    /// <returns>An observable sequence with the specified actions applied on the previous item, last item, and errors.</returns>
    public IObservable<T> DoOnPrevious(Action<T> onPrevious) =>
      source.DoOnPrevious(onPrevious, onPrevious);



    /// <summary>
    ///   Applies an action on the previous item in the sequence before emitting the current item.
    /// </summary>
    /// <param name="onPrevious">The action to apply on the previous item.</param>
    /// <param name="onLast">The action to apply on the last item.</param>
    /// <returns>An observable sequence with the action applied on the previous item.</returns>
    public IObservable<T> DoOnPrevious(Action<T> onPrevious, Action<T> onLast) {
      ArgumentNullException.ThrowIfNull(onPrevious);
      ArgumentNullException.ThrowIfNull(onLast);
      return Observable.Create<T>(observer => {
        T? previous = default;
        var valueReceived = false;
        return source
               .Do(
                 current => {
                   if (valueReceived) {
                     onPrevious(previous!);
                   } else {
                     valueReceived = true;
                   }

                   previous = current;
                 },
                 () => {
                   if (valueReceived) {
                     onLast(previous!);
                   }
                 }
               )
               .Subscribe(observer);
      }
      );
    }



    /// <summary>
    ///   Turns a cold observable into a hot, reference-counted one.
    ///   First subscriber connects, last subscriber disconnects.
    ///   Semantics: source.Publish().RefCount()
    /// </summary>
    public IObservable<T> Share() =>
      source.Publish().RefCount();


    #endregion
  }



  /// <param name="source">The source observable sequence.</param>
  /// <typeparam name="T">The type of the items in the observable sequence, which must implement IDisposable.</typeparam>
  extension<T>(IObservable<T> source)
    where T : IDisposable? {
    #region methods and others

    /// <summary>
    ///   Ensures that any item emitted by the source is disposed immediately after
    ///   it has been processed by the downstream observer.
    /// </summary>
    /// <returns>A cold observable that disposes elements after emission.</returns>
    public IObservable<T> FireAndForget() =>
      Observable.Create<T>(observer =>
        source.Subscribe(
          item => {
            try {
              observer.OnNext(item);
            } finally {
              item?.Dispose();
            }
          },
          observer.OnError,
          observer.OnCompleted
        ))
                .Share();



    /// <summary>
    ///   Disposes of the previous disposable item in the sequence before emitting the next item.
    ///   Ensures that resources are properly released before processing new items.
    /// </summary>
    /// <typeparam name="T">The type of the items in the observable sequence, which must implement IDisposable.</typeparam>
    /// <param name="source">The source observable sequence.</param>
    /// <param name="disposableSelector">The selector that returns the disposable</param>
    /// <param name="disposeOnException">Specify whether or not prior disposal should take place in the event of an error.</param>
    /// <returns>
    ///   An observable sequence where the previous disposable item is disposed of before the next item is emitted.
    /// </returns>
    /// <remarks>
    ///   <paramref name="disposeOnException" /> gates <b>only</b> the error path. Every other release —
    ///   superseded item, completion, and unsubscription — is unconditional, which is the whole point of the
    ///   operator. It used to gate all four on that flag, so the default <c>DisposePrevious()</c> disposed
    ///   nothing at all: every caller ("swap to the newly configured output/gate/subscription") silently kept
    ///   its predecessor alive and running alongside the replacement.
    /// </remarks>
    public IObservable<T> DisposePrevious(Func<T, IDisposable?> disposableSelector,
                                          bool disposeOnException = false) =>
      Observable.Create<T>(observer => {
        T? previous = default;

        void DisposeCurrent() {
          if (previous is not null) {
            disposableSelector(previous)?.Dispose();
            previous = default;
          }
        }

        var subscription = source
                           .Do(
                             current => {
                               DisposeCurrent();
                               previous = current;
                             },
                             ex => {
                               try {
                                 if (disposeOnException) {
                                   DisposeCurrent();
                                 }
                               } finally {
#pragma warning disable CA2219
                                 throw ex;
#pragma warning restore CA2219
                               }
                             },
                             DisposeCurrent
                           )
                           .Subscribe(observer);

        // The last item must go when the subscription does, not only when the source completes: these
        // sequences (a node's output gate, a device's active connection) never complete on their own, so
        // without this the final item outlives the pipeline that owns it.
        return new CompositeDisposable(subscription, Disposable.Create(DisposeCurrent));
      }
      );



    /// <summary>
    ///   Ensures that the previous item is disposed of before moving to the next item in the sequence and also disposes of the
    ///   last item when the sequence is completed. It uses the Do operator to perform these side effects without subscribing
    ///   to the observable, making it a cold observable variant.
    /// </summary>
    /// <param name="beforeDispose">An action applied directly before the object becomes disposed</param>
    /// <param name="disposeOnException">Specify whether or not prior disposal should take place in the event of an error.</param>
    /// <returns>
    ///   An observable sequence where each item disposes of the previous item before being emitted,
    ///   and the last item is disposed of when the sequence is completed.
    /// </returns>
    public IObservable<T> DisposePrevious(Action<T> beforeDispose,
                                          bool disposeOnException = false) =>
      source.DisposePrevious(
        srcObj => {
          beforeDispose(srcObj);
          return srcObj;
        },
        disposeOnException
      );



    /// <summary>
    ///   Ensures that the previous item is disposed of before moving to the next item in the sequence and also disposes of the
    ///   last item when the sequence is completed. It uses the Do operator to perform these side effects without subscribing
    ///   to the observable, making it a cold observable variant.
    /// </summary>
    /// <param name="disposeOnException">Specify whether or not prior disposal should take place in the event of an error.</param>
    /// <returns>
    ///   An observable sequence where each item disposes of the previous item before being emitted,
    ///   and the last item is disposed of when the sequence is completed.
    /// </returns>
    public IObservable<T> DisposePrevious(bool disposeOnException = false) =>
      source.DisposePrevious(srcObj => srcObj, disposeOnException);

    #endregion
  }



  /// <param name="source">The observable sequence of boolean values.</param>
  extension(IObservable<bool> source) {
    #region methods and others

    /// <summary>
    ///   Filters an observable sequence to only include true values.
    /// </summary>
    /// <returns>An observable sequence that contains only true values.</returns>
    public IObservable<bool> WhereTrue() => source.Where(v => v);



    /// <summary>
    ///   Filters an observable sequence to only include false values.
    /// </summary>
    /// <returns>An observable sequence that contains only false values.</returns>
    public IObservable<bool> WhereFalse() => source.Where(v => !v);



    public IObservable<Unit> ToCommand(BoolToCommandTrigger trigger) =>
      trigger switch {
        BoolToCommandTrigger.FromFalseToTrue => source.PairWithPrevious()
                                                      .Where(t => t is { Item1: false, Item2: true })
                                                      .Select(_ => Unit.Default),
        BoolToCommandTrigger.FromTrueToFalse => source.PairWithPrevious()
                                                      .Where(t => t is { Item1: true, Item2: false })
                                                      .Select(_ => Unit.Default),
        BoolToCommandTrigger.FromAnyToTrue => source.Where(b => b).Select(_ => Unit.Default),
        BoolToCommandTrigger.FromAnyToFalse => source.Where(b => !b).Select(_ => Unit.Default),
        BoolToCommandTrigger.Changed => source.PairWithPrevious()
                                              .Where(t => t.Previous != t.Current)
                                              .Select(_ => Unit.Default),
        BoolToCommandTrigger.Equal => source.PairWithPrevious()
                                            .Where(t => t.Previous == t.Current)
                                            .Select(_ => Unit.Default),
        _ => Observable.Empty<Unit>()
      };

    #endregion
  }



  extension<T>(IObservable<T?> source)
    where T : struct {
    #region methods and others

    /// <summary>
    ///   Filters an observable sequence to include only elements that have a value and extracts their underlying values.
    /// </summary>
    /// <typeparam name="T">The value type of the nullable source elements.</typeparam>
    /// <param name="source">The source observable sequence of nullable values.</param>
    /// <returns>An observable sequence containing the non-null values of the source sequence.</returns>
    public IObservable<T> WhereHasValue()
      =>
        source.Where(x => x.HasValue).Select(x => x!.Value);

    #endregion
  }



  extension<T>(IObservable<T?> source) {
    #region methods and others

    public IObservable<T> WhereNotNullAndNotDisposed(Func<T, ICancelable?> selector) =>
      source.Where(item => item != null && selector(item)?.IsDisposed != true)!;

    #endregion
  }



  extension<T>(IObservable<T?> source)
    where T : ICancelable {
    #region methods and others

    public IObservable<T> WhereNotNullAndNotDisposed() =>
      source.Where(item => item != null && !item.IsDisposed)!;

    #endregion
  }



  extension<T>(IObservable<T> source)
    where T : ICancelable {
    #region methods and others

    public IObservable<T> ThrowIfDisposed(string? message = null) =>
      source.Select(item => item.IsDisposed ? throw new ObjectDisposedException(nameof(item), message) : item);



    public IObservable<T> WhereNotDisposed() =>
      source.Where(item => !item.IsDisposed);

    #endregion
  }

  #endregion
}
