namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Reactive.Disposables;
using global::System.Reactive.Linq;
using global::System.Reactive.Subjects;
using global::System.Threading;
using global::System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;



/// <summary>
///   One failed pull of a <see cref="Producer{T}" />'s source.
/// </summary>
/// <param name="Error">What the source threw.</param>
/// <param name="ConsecutiveFailures">
///   How many pulls have failed in a row, this one included. Reset by the next value the source produces, so a
///   count that keeps climbing means the source is not coming back by itself - an unplugged camera.
/// </param>
public readonly record struct ProducerFault(Exception Error, int ConsecutiveFailures);



/// <summary>
///   Produces values by pulling from a source and pushing it to the <see cref="IObserver{T}" />'s. The production can be
///   paused
///   (disabled) and continued (enabled).
/// </summary>
/// <typeparam name="T"></typeparam>
public class Producer<T> : IObservable<T>, IDisposable {
  #region fields and properties

  /// <summary>
  ///   The producer instance whose <see cref="ProduceValues" /> loop currently owns the calling thread, if any.
  ///   Observers run inline on that thread, so an observer calling back into <see cref="SetEnabled" /> /
  ///   <see cref="SetDisabled" /> would otherwise join the very task it is running inside.
  /// </summary>
  [ThreadStatic]
  private static Producer<T>? _producingOnThread;

  private readonly CancellationToken _lifetimeToken;
  private readonly Subject<ProducerFault> _faults = new();
  private readonly Subject<bool> _hasObservers = new();
#if NET9_0_OR_GREATER
  private readonly Lock _lock = new();
#else
  private readonly object _lock = new();
#endif
  private readonly ILogger _logger;

  /// <summary>
  ///   Invoked after a value has been pushed to every observer, on the producing thread. See the
  ///   <c>onDelivered</c> constructor parameter.
  /// </summary>
  private readonly Action<T>? _onDelivered;

  private readonly Func<CancellationToken, IEnumerable<T>> _sourceFactory;
  private readonly Subject<T> _subject = new();

  private CancellationTokenSource? _cancellationTokenSource;
  private bool _disposed;
  private bool _enabled;
  private int _observerCount;
  private Task _producerTask = Task.CompletedTask;

  /// <summary>
  ///   Provides values, if it has observers or not.
  /// </summary>
  public IObservable<bool> HasObservers => _hasObservers;

  /// <summary>
  ///   Every failed pull of the source, on the producing thread. The producer itself logs, backs off and keeps
  ///   going regardless; this is for an owner that has to decide when "keeps failing" means the hardware
  ///   behind the source is gone (see <c>DeviceBuilder.DisconnectsAfterCaptureFailures</c>).
  /// </summary>
  public IObservable<ProducerFault> Faults => _faults.AsObservable();

  /// <summary>
  ///   True while the calling thread is this producer's own capture loop — i.e. we were re-entered from an observer
  ///   callback. Joining the producer task from there would block until it times out.
  /// </summary>
  private bool IsOnProducerThread => ReferenceEquals(_producingOnThread, this);

  #endregion

  #region constructors

  public Producer(IEnumerable<T> source, bool initialEnabled = false, Action<T>? onDelivered = null, ILogger? logger = null)
    : this(_ => source, initialEnabled, onDelivered, logger, CancellationToken.None) { }



  public Producer(Func<CancellationToken, IEnumerable<T>> sourceFactory, bool initialEnabled = false, ILogger? logger = null)
    : this(sourceFactory, initialEnabled, null, logger, CancellationToken.None) { }



  public Producer(Func<CancellationToken, T?> source,
                  CancellationToken cancellation,
                  bool initialEnabled = false,
                  Action<T>? onDelivered = null,
                  ILogger? logger = null)
    : this(ct => Enumerate(source, ct), initialEnabled, onDelivered, logger, cancellation) { }



  /// <param name="onDelivered">
  ///   Called once per value, on the producing thread, after <see cref="Subject{T}.OnNext" /> has returned -
  ///   i.e. after every observer has handled it synchronously. This is the only point at which a value the
  ///   producer OWNS (a native SDK capture, say) may be released: the producer pushes to a shared subject, so
  ///   no individual observer knows whether it was the last. Runs in a <c>finally</c>, because
  ///   <c>Subject.OnNext</c> aborts its observer loop on the first throw and the value still has to be freed.
  /// </param>
  private Producer(Func<CancellationToken, IEnumerable<T>> sourceFactory,
                   bool initialEnabled,
                   Action<T>? onDelivered,
                   ILogger? logger,
                   CancellationToken lifetimeToken) {
    _logger = logger ?? NullLogger.Instance;
    _sourceFactory = sourceFactory;
    _lifetimeToken = lifetimeToken;
    _onDelivered = onDelivered;
    _hasObservers.OnNext(false);
    if (initialEnabled) {
      SetEnabled();
    }
  }

  #endregion

  #region methods and others

  private static IEnumerable<T> Enumerate(Func<CancellationToken, T?> valueCallback,
                                          CancellationToken ct) {
    while (!ct.IsCancellationRequested) {
      var value = valueCallback.Invoke(ct);
      if (value != null) {
        yield return value;
      }
    }
  }



  /// <summary>
  ///   Continues the production. A fresh source sequence is built from the factory, so production resumes even after a
  ///   prior fault or a clean exhaustion caused by cancellation.
  /// </summary>
  public void SetEnabled() {
    lock (_lock) {
      if (_enabled) {
        return;
      }

      // Join the previous producer task before touching a new enumerator, so two tasks never call
      // MoveNext() concurrently on overlapping rapid disable/enable cycles. Skip the join when we *are* the
      // previous task (re-entered from an observer callback): it can only complete once this call returns,
      // and its token is already cancelled, so it exits at the next loop check without another MoveNext().
      if (!IsOnProducerThread) {
        try {
          _producerTask.GetAwaiter().GetResult();
        } catch {
          // Surfaced already (or swallowed) by the previous run; irrelevant to starting a new one.
        }
      }

      _cancellationTokenSource?.Dispose();
      _enabled = true;
      // Linking to the lifetime token means a device teardown also promptly stops this session,
      // without requiring every capture delegate to separately observe two tokens.
      _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeToken);
      var ct = _cancellationTokenSource.Token;
      _producerTask = Task.Factory.StartNew(() => ProduceValues(ct), ct, TaskCreationOptions.LongRunning,
        TaskScheduler.Default);
    }
  }



  /// <summary>
  ///   Pause the production.
  /// </summary>
  public void SetDisabled() {
    Task taskToWait;

    lock (_lock) {
      if (!_enabled) {
        return;
      }

      _enabled = false;
      _cancellationTokenSource?.Cancel();
      taskToWait = _producerTask;
    }

    // An observer that disables the producer from its own OnNext runs on the producer thread, so the task it
    // would wait for is the one below it on the stack — waiting is a guaranteed 5s stall. Cancellation has
    // already been requested; the loop stops as soon as this callback returns.
    if (IsOnProducerThread) {
      return;
    }

    // Wait outside the lock to avoid blocking concurrent callers on SetEnabled()'s join.
    try {
      taskToWait.Wait(TimeSpan.FromSeconds(5));
    } catch (AggregateException) {
      // Task was already completed or cancelled
    }
  }



  private void ProduceValues(CancellationToken ct) {
    IEnumerator<T>? enumerator = null;
    var attempt = 0;
    var previousProducer = _producingOnThread;
    _producingOnThread = this;
    try {
      while (!ct.IsCancellationRequested) {
        if (enumerator == null) {
          try {
            enumerator = _sourceFactory(ct).GetEnumerator();
          } catch (Exception e) when (!ct.IsCancellationRequested) {
            LogCaptureFaulted(e, attempt);
            PublishFault(e, attempt);
            BackOff(ref attempt, ct);
            continue;
          } catch (Exception) {
            return;
          }
        }

        bool moved;
        try {
          moved = enumerator.MoveNext();
        } catch (Exception e) when (!ct.IsCancellationRequested) {
          // Genuine capture error, not a side-effect of SetDisabled(): log, drop, rebuild the source and keep
          // going. An OnError here would be terminal for the shared subject and unrecoverable by re-enabling.
          LogCaptureFaulted(e, attempt);
          PublishFault(e, attempt);
          enumerator.Dispose();
          enumerator = null;
          BackOff(ref attempt, ct);
          continue;
        } catch (Exception) {
          // Exception during inner cancellation (e.g. camera stops while captureFrame is blocking) — exit cleanly
          return;
        }

        if (ct.IsCancellationRequested) {
          // A value pulled in the same breath as the cancellation is never delivered, so nothing downstream
          // can release it. When the producer owns its values (an SDK capture), dropping it silently here
          // leaks one per disable - and a device is disabled and re-enabled on every graph reload.
          if (moved && enumerator.Current != null) {
            InvokeOnDelivered(enumerator.Current);
          }

          return;
        }

        if (!moved) {
          // Natural end with no cancellation in play: the source itself is exhausted (e.g. a finite array).
          _subject.OnCompleted();
          return;
        }

        attempt = 0;
        var value = enumerator.Current;
        if (value != null) {
          try {
            _subject.OnNext(value);
          } finally {
            InvokeOnDelivered(value);
          }
        }
      }
      // Token cancelled (SetDisabled) — exit without touching the Subject; production simply pauses.
    } finally {
      enumerator?.Dispose();
      _producingOnThread = previousProducer;
    }
  }



  private void InvokeOnDelivered(T value) {
    if (_onDelivered == null) {
      return;
    }

    try {
      _onDelivered(value);
    } catch (Exception e) {
      // Releasing a value must never take the capture loop down; the next value is unaffected.
      ProducerLog.ReleaseFailed(_logger, e);
    }
  }



  /// <summary>
  ///   Tells <see cref="Faults" /> about a failed pull. Whatever an observer does with it must not take the
  ///   capture loop down, and a producer disposed under a still-running loop simply has nobody left to tell.
  /// </summary>
  private void PublishFault(Exception error, int attempt) {
    try {
      _faults.OnNext(new ProducerFault(error, attempt + 1));
    } catch (ObjectDisposedException) {
      // Disposed concurrently; the loop exits at its next cancellation check.
    } catch (Exception e) {
      ProducerLog.FaultObserverFailed(_logger, e);
    }
  }



  private static void BackOff(ref int attempt, CancellationToken ct) {
    attempt++;
    var delayMs = Math.Min(100 * (1 << Math.Min(attempt - 1, 5)), 2000);
    try {
      Task.Delay(delayMs, ct).Wait(ct);
    } catch (OperationCanceledException) {
      // Cancelled while backing off — the outer loop will exit on the next check.
    }
  }



  private void LogCaptureFaulted(Exception e, int attempt) => ProducerLog.CaptureFailed(_logger, e, attempt + 1);



  ~Producer() => Dispose();



  public void Dispose() {
    if (_disposed) {
      return;
    }

    _disposed = true;
    SetDisabled();

    _subject.OnCompleted();
    _subject.Dispose();
    _hasObservers.OnCompleted();
    _hasObservers.Dispose();
    _faults.OnCompleted();
    _faults.Dispose();
    _cancellationTokenSource?.Dispose();
    GC.SuppressFinalize(this);
  }



  public IDisposable Subscribe(IObserver<T> observer) {
    if (Interlocked.Increment(ref _observerCount) == 1) {
      _hasObservers.OnNext(true);
    }

    var subscription = _subject.Subscribe(observer);

    return Disposable.Create(() => {
      subscription.Dispose();
      if (Interlocked.Decrement(ref _observerCount) == 0) {
        _hasObservers.OnNext(false);
      }
    }
    );
  }

  #endregion
}



internal static partial class ProducerLog {
  [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Producer failed to release a delivered value; continuing.")]
  public static partial void ReleaseFailed(ILogger logger, Exception exception);



  [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Producer fault observer failed; continuing.")]
  public static partial void FaultObserverFailed(ILogger logger, Exception exception);



  [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Warning,
    Message = "Producer capture failed (attempt {Attempt}); rebuilding source and continuing."
  )]
  public static partial void CaptureFailed(ILogger logger, Exception exception, int attempt);
}
