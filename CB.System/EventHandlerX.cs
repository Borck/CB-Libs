using System;
using System.Linq;
using System.Threading.Tasks;



namespace CB.System {
  public static class EventHandlerX {
    private static readonly Action[] NoActions = Array.Empty<Action>();



    /// <summary>
    ///   Invokes each invocation, possibly in parallel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventHandler"></param>
    /// <param name="arg"></param>
    public static void InvokeParallel<T>(this EventHandler<T> eventHandler, T arg) {
      var actions = eventHandler
                    ?.GetInvocationList()
                    .Select(del => new Action(() => del.DynamicInvoke(arg)))
                    .ToArray() ??
                    NoActions;
      Parallel.Invoke(actions);
    }



    /// <summary>
    ///   Subscribes an EventHandler and invokes that delegate asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventHandler"></param>
    /// <param name="sender"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static Task InvokeAsync<T>(this EventHandler<T> eventHandler, object sender, T arg)
      => Task.Factory.FromAsync(
        (asyncCallback, @object) => eventHandler.BeginInvoke(sender, arg, asyncCallback, @object),
        eventHandler.EndInvoke,
        null
      );
    
  }
}
