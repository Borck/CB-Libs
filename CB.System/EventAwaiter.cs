using System;
using System.Threading.Tasks;



namespace CB.System {
  /// <summary>
  ///   It make event handling available for a async-await implementation.
  ///   Source: https://stackoverflow.com/questions/12858501/is-it-possible-to-await-an-event-instead-of-another-async-method
  /// </summary>
  /// <typeparam name="TEventArgs"></typeparam>
  public class EventAwaiter<TEventArgs> {
    private readonly TaskCompletionSource<TEventArgs> _eventArrived = new TaskCompletionSource<TEventArgs>();

    private readonly Action<EventHandler<TEventArgs>> _unsubscribe;
    private readonly Func<object, TEventArgs, bool> _condition;



    /// <summary>
    /// </summary>
    /// <param name="subscribe">action to subscribe to a <see cref="EventHandler" /></param>
    /// <param name="unsubscribe">action to unsubscribe to a <see cref="EventHandler" /></param>
    public EventAwaiter(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe) {
      subscribe(Subscription);
      _unsubscribe = unsubscribe;
      _condition = (_, __) => true;
    }



    /// <summary>
    /// </summary>
    /// <param name="subscribe">action to subscribe to a <see cref="EventHandler" /></param>
    /// <param name="unsubscribe">action to unsubscribe to a <see cref="EventHandler" /></param>
    /// <param name="condition"></param>
    public EventAwaiter(Action<EventHandler<TEventArgs>> subscribe,
                        Action<EventHandler<TEventArgs>> unsubscribe,
                        Func<object, TEventArgs, bool> condition) {
      subscribe(Subscription);
      _unsubscribe = unsubscribe;
      _condition = condition;
    }



    public Task<TEventArgs> Task => _eventArrived.Task;

    private EventHandler<TEventArgs> Subscription => (s, e) => {
      if (_condition.Invoke(s, e)) {
        _unsubscribe(Subscription);
        _eventArrived.TrySetResult(e);
      }
    };
  }
}
