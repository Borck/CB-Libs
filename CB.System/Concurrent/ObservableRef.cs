namespace CB.System.Concurrent;

using global::System;
using global::System.Diagnostics;
using global::System.Threading.Tasks;

[DebuggerDisplay("{" + nameof(Value) + "}")]
public sealed class ObservableRef<T> {
  private readonly object _lock = new();

  private T _value;

  public T Value {
    // Under the setter's lock: a value type wider than a word could otherwise be read half written.
    get {
      lock (_lock) {
        return _value;
      }
    }
    set {
      lock (_lock) {
        var eventArgs = new NotifyValueChangedEventArgs<T>(value, _value);
        _value = value;
        ValueChanged?.Invoke(this, eventArgs);
      }
    }
  }



  public event EventHandler<NotifyValueChangedEventArgs<T>> ValueChanged;



  public Task UpdateValueAsync(T value) {
    var task = new Task(() => Value = value);
    task.Start();
    return task;
  }
}
