namespace CB.System.Threading;

using global::System;
using global::System.Runtime.ExceptionServices;
using global::System.Threading.Tasks;

public static class TaskX {
  public static Task AsStarted(this Task task) {
    task.Start();
    return task;
  }



  public static Task<T> AsStarted<T>(this Task<T> task) {
    task.Start();
    return task;
  }



  /// <summary>
  ///   Runs <paramref name="operation" /> on the thread pool and waits for it at most <paramref name="deadline" />, from
  ///   synchronous code. Returns whether it finished in time.
  /// </summary>
  /// <remarks>
  ///   For a synchronous teardown (<c>Dispose</c>) that must stop something asynchronous: because the operation starts on the
  ///   thread pool and not on the caller's context, a caller on a single-threaded context (a UI thread, an event loop) does
  ///   not deadlock against a continuation that wants that same thread. An operation that overruns keeps running; its later
  ///   failure is observed so it does not surface as an unobserved task exception.
  /// </remarks>
  /// <returns><c>true</c> if the operation completed, <c>false</c> if the deadline passed first.</returns>
  /// <exception cref="ArgumentNullException"><paramref name="operation" /> is null.</exception>
  /// <exception cref="Exception">The operation failed within the deadline: the original exception, not an <see cref="AggregateException" />.</exception>
  public static bool WaitWithDeadline(Func<Task> operation, TimeSpan deadline) {
    ArgumentNullException.ThrowIfNull(operation);

    var task = Task.Run(operation);
    bool completed;
    try {
      completed = task.Wait(deadline);
    } catch (AggregateException ex) when (ex.InnerExceptions.Count == 1) {
      ExceptionDispatchInfo.Capture(ex.InnerExceptions[0]).Throw();
      throw;
    }

    if (!completed) {
      _ = task.ContinueWith(
        static t => _ = t.Exception,
        TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously
      );
    }

    return completed;
  }
}
