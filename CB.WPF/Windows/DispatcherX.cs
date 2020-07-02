using System;
using System.Threading.Tasks;
using System.Windows.Threading;



namespace CB.WPF.Windows {
  public static class DispatcherX {
    public static void InvokeWithCheckedAccess(this Dispatcher dispatcher, Action callback) {
      if (dispatcher.CheckAccess()) {
        callback();
      } else {
        dispatcher.Invoke(callback);
      }
    }



    public static TResult InvokeWithCheckedAccess<TResult>(this Dispatcher dispatcher, Func<TResult> callback) {
      return dispatcher.CheckAccess()
               ? callback()
               : dispatcher.Invoke(callback);
    }



    /// <summary>
    ///   Runs the callback asynchronous except the dispatcher is running on the same thread as this call was done.
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public static Task InvokeAsyncWithCheckedAccess(this Dispatcher dispatcher, Action callback) {
      if (!dispatcher.CheckAccess()) {
        return dispatcher.InvokeAsync(callback).Task;
      }

      callback();
      var task = new Task(() => { });
      task.RunSynchronously();
      return task;
    }



    public static Task<TResult> InvokeAsyncWithCheckedAccess<TResult>(this Dispatcher dispatcher,
                                                                      Func<TResult> callback) {
      if (!dispatcher.CheckAccess()) {
        return dispatcher.InvokeAsync(callback).Task;
      }

      var result = callback();
      var task = new Task<TResult>(() => result);
      task.Start();
      return task;
    }
  }
}
