using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;



namespace CB.WPF.Windows {
  public static class Windows {
    /// <summary>
    /// Creates a window asynchronous
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="show"></param>
    /// <returns></returns>
    public static Task<T> CreateWindowAsync<T>(bool show = true)
      where T : Window {
      var taskCompletionSource = new TaskCompletionSource<T>();
      var thread = new Thread(
        () => {
          var window = Activator.CreateInstance<T>();
          taskCompletionSource.SetResult(window);
          if (show) {
            window.Show();
          }

          Dispatcher.Run();
        }
      ) {IsBackground = true};
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      return taskCompletionSource.Task;
    }
  }
}
