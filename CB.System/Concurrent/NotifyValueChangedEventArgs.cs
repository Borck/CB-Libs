using System;



namespace CB.System.Concurrent {
  public class NotifyValueChangedEventArgs<T> : EventArgs {
    public readonly T NewValue;
    public readonly T OldValue;

    public bool ValueChanged => !ReferenceEquals( OldValue, NewValue );



    public NotifyValueChangedEventArgs(T newValue, T oldValue) {
      NewValue = newValue;
      OldValue = oldValue;
    }
  }
}
