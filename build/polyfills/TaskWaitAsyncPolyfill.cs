namespace System.Threading.Tasks;

/// <summary><c>Task.WaitAsync(CancellationToken)</c> (.NET 6+) for the targets that predate it.</summary>
internal static class TaskWaitAsyncPolyfill {
  public static async Task WaitAsync(this Task task, CancellationToken cancellationToken) {
    if (task.IsCompleted || !cancellationToken.CanBeCanceled) {
      await task.ConfigureAwait(false);
      return;
    }

    var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    using (cancellationToken.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(true), cancelled)) {
      if (await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false) != task) {
        throw new OperationCanceledException(cancellationToken);
      }
    }

    await task.ConfigureAwait(false);
  }



  public static async Task<T> WaitAsync<T>(this Task<T> task, CancellationToken cancellationToken) {
    await ((Task)task).WaitAsync(cancellationToken).ConfigureAwait(false);
    return await task.ConfigureAwait(false);
  }
}
