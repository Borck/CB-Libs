#if !NET6_0_OR_GREATER
namespace CB.Reactive;

using global::System;
using global::System.Threading;
using global::System.Threading.Tasks;



/// <summary><c>Task.WaitAsync(CancellationToken)</c> (.NET 6+) for the net48 test build.</summary>
internal static class TaskTimeoutExtensions {
  public static async Task WaitAsync(this Task task, CancellationToken cancellationToken) {
    var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    using (cancellationToken.Register(static state => ((TaskCompletionSource<bool>)state).TrySetResult(true), cancelled)) {
      if (await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false) != task) {
        throw new OperationCanceledException(cancellationToken);
      }
    }

    await task.ConfigureAwait(false);
  }



  public static async Task<T> WaitAsync<T>(this Task<T> task, CancellationToken cancellationToken) {
    var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    using (cancellationToken.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(true), cancelled)) {
      if (await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false) != task) {
        throw new OperationCanceledException(cancellationToken);
      }
    }

    return await task.ConfigureAwait(false);
  }
}
#endif
