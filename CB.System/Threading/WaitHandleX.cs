using System;
using System.Threading;
using System.Threading.Tasks;



namespace CB.System.Threading {
  public static class WaitHandleX {
    #region methods and others



    public static async Task<bool> WaitOneAsync(this WaitHandle waitHandle,
                                                int timeout,
                                                CancellationToken cancellationToken) {
      if (waitHandle == null) {
        throw new ArgumentNullException(nameof(waitHandle));
      }

      if (waitHandle is Mutex) {
        throw new ArgumentException(
          "WaitOneAsync cannot be used with Mutex, because the wait and release needs to be on the same thread.",
          nameof(waitHandle)
        );
      }

      if (timeout < -1) {
        throw new ArgumentOutOfRangeException(nameof(timeout), "The timeout must be greater or equal -1.");
      }

      var tcs = new TaskCompletionSource<bool>();
      cancellationToken.ThrowIfCancellationRequested();

      var registeredHandle =
        ThreadPool.RegisterWaitForSingleObject(
          waitHandle,
          (state, timedOut) => (state as TaskCompletionSource<bool>)?.TrySetResult(!timedOut),
          tcs,
          timeout,
          true
        );

#if NETCOREAPP3_1_OR_GREATER
      await
#endif
      using (cancellationToken.Register(
                     () => {
                       if (registeredHandle.Unregister(null)) {
                         tcs.TrySetCanceled(cancellationToken);
                       }
                     }
                   )) {
        try {
          return await tcs.Task.ConfigureAwait(false);
        }
        finally {
          registeredHandle.Unregister(null);
        }
      }
    }



    public static Task<bool> WaitOneAsync(this WaitHandle handle,
                                          TimeSpan timeout,
                                          CancellationToken cancellationToken) {
      return handle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);
    }



    public static Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken) {
      return handle.WaitOneAsync(Timeout.Infinite, cancellationToken);
    }



    public static Task AsTask(this WaitHandle handle) {
      return AsTask(handle, Timeout.InfiniteTimeSpan);
    }



    public static Task AsTask(this WaitHandle handle, TimeSpan timeout) {
      var tcs = new TaskCompletionSource<object>();
      var registration = ThreadPool.RegisterWaitForSingleObject(
        handle,
        (state, timedOut) => {
          var localTcs = state as TaskCompletionSource<object?>;
          if (timedOut)
            localTcs?.TrySetCanceled();
          else
            localTcs?.TrySetResult(default);
        },
        tcs,
        timeout,
        executeOnlyOnce: true
      );
      tcs.Task.ContinueWith(
        (_, state) => (state as RegisteredWaitHandle)?.Unregister(default),
        registration,
        TaskScheduler.Default
      );
      return tcs.Task;
    }

    #endregion
  }
}
