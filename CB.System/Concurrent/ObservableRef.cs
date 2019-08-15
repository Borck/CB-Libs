using System;
using System.Diagnostics;
using System.Threading.Tasks;



namespace CB.System.Concurrent {
  [DebuggerDisplay( "{" + nameof(Value) + "}" )]
  public sealed class ObservableRef<T> {
    private readonly object _lock = new object();

    private T _value;

    public T Value {
      get => _value;
      set {
        lock (_lock) {
          var eventArgs = new NotifyValueChangedEventArgs<T>( value, _value );
          _value = value;
          ValueChanged?.Invoke( this, eventArgs );
        }
      }
    }



    public event EventHandler<NotifyValueChangedEventArgs<T>> ValueChanged;



    public Task UpdateValueAsync(T value) {
      var task = new Task( () => { Value = value; } );
      task.Start();
      return task;
    }
  }
}
