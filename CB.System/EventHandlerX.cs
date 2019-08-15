using System;
using System.Linq;
using System.Threading.Tasks;



namespace CB.System {
  public static class EventHandlerX {
    private static readonly Action[] _noActions = Array.Empty<Action>();



    public static void InvokeParallel<T>(this EventHandler<T> eventHandler, T arg) {
      var actions = eventHandler
                    ?.GetInvocationList()
                    .Select( del => new Action( () => del.DynamicInvoke( arg ) ) )
                    .ToArray() ??
                    _noActions;
      Parallel.Invoke( actions );
    }
  }
}
