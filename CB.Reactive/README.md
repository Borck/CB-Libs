# CB.Reactive

Operators and helpers for [System.Reactive](https://github.com/dotnet/reactive), extracted from the T·VIN framework where
they run in production. Targets net11.0, net10.0, net48 and netstandard2.0.

```csharp
using CB.Reactive;
```

## What is in it

**Hand-off between threads that does not leak what it drops**
- `ObserveOnDisposingDropped(scheduler, dropDisposable, maxQueued, onOverflow)` - `ObserveOn` with a bounded queue; whatever is
  displaced or still queued at teardown is released through `dropDisposable`.
- `ObserveOnConflatingDropped(scheduler, dropDisposable)` - the same, keeping only the newest item.
- `DropWhenBusy(scheduler, onDropped)` - deliver an item only when the previous one is done; report the rest as dropped.
- `QuietLoopScheduler` - an `EventLoopScheduler` that drops work after it is disposed instead of throwing on a foreign thread.

**Subscriptions**
- `SubscribeGuarded(onNext, context, logger)` - a handler that throws is logged and the subscription stays (Rx would detach it).
- `Share()`, `TrackSubscription(out hasObservers)`, `FireAndForget()` / `FireAndForget(selector)`, `DisposePrevious(...)`.

**Sequences**
- `PairWithPrevious()`, `WhereConsecutive(predicate)`, `DoOnPrevious(...)`, `TrackStatusFlags(reset)`, `RatePerSecond()`.
- `WhereTrue()`, `WhereFalse()`, `ToCommand(BoolToCommandTrigger)`, `WhereHasValue()`, `ThrowIfDisposed(...)`,
  `WhereNotDisposed(...)`, `WhereNotNullAndNotDisposed(...)`.
- `Demultiplex(keySelector)` and `Demultiplex()` (by type): one stream per key or type, plus a fallback.

**Producers and retries**
- `Producer<T>` - runs an enumerable on its own thread only while it has observers; faults are reported and the source rebuilt.
- `ObservableX.Poll(...)`, `ObservableX.RetryWithBackoff(...)` for `RetryWhen` and for blocking actions.

**Async clean-up** (namespace `System.Reactive.Disposables`, so no extra `using`)
- `AddAsync(...)`, `AsDisposable()`, `DisposeWithAsync(...)` - start an `IAsyncDisposable` from a synchronous `Dispose`, and log its failure.

## Logging

Operators that report something take an optional `Microsoft.Extensions.Logging.ILogger`; without one they stay silent. The package
depends on `Microsoft.Extensions.Logging.Abstractions` only.

## Contributors/Contributing

Maintained by [Christian Borck](https://github.com/Borck/)
