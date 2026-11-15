namespace CB.Reactive;

using global::System;
using global::System.Reactive.Concurrency;
using global::System.Reactive.Disposables;



/// <summary>
///   An owner's own <see cref="EventLoopScheduler" /> as a scheduler for <c>ObserveOn</c>: work that reaches it after the loop
///   has been disposed is dropped, where the loop itself throws <see cref="ObjectDisposedException" />.
/// </summary>
/// <remarks>
///   <para>
///     An engine or a graph disposes its subscriptions and then its loop (last, never inside the same
///     <c>CompositeDisposable</c> - see <c>FlowEngine.Dispose</c>). That order is not enough: a notification that was already
///     past the last disposed operator on another thread (the tree's dispatcher, a pool thread) when the subscriptions ended
///     can reach <c>ObserveOn</c> after the loop has gone, and <c>ObserveOn</c> schedules on the thread that delivered it.
///     <see cref="ObjectDisposedException" /> then escapes on a thread nothing catches, and the process ends. It cost whole
///     test runs, intermittently, only under load.
///   </para>
///   <para>
///     Nothing is lost by dropping it: the owner is gone, and so is whoever would have read what the work produced. Use this
///     for <c>ObserveOn</c> only. Work somebody awaits (<c>Observable.Start</c>, a command) must still fail loudly.
///   </para>
/// </remarks>
public sealed class QuietLoopScheduler(EventLoopScheduler loop) : IScheduler {
  #region fields and properties

  /// <inheritdoc />
  public DateTimeOffset Now => loop.Now;

  #endregion

  #region methods and others

  /// <inheritdoc />
  public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action) =>
    Quietly(() => loop.Schedule(state, (_, s) => action(this, s)));



  /// <inheritdoc />
  public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) =>
    Quietly(() => loop.Schedule(state, dueTime, (_, s) => action(this, s)));



  /// <inheritdoc />
  public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) =>
    Quietly(() => loop.Schedule(state, dueTime, (_, s) => action(this, s)));



  private static IDisposable Quietly(Func<IDisposable> schedule) {
    try {
      return schedule();
    } catch (ObjectDisposedException) {
      // The loop is gone with its owner; see the remarks.
      return Disposable.Empty;
    }
  }

  #endregion
}
