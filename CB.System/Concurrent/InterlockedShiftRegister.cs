namespace CB.System.Concurrent;

public class InterlockedShiftRegister<T> : ShiftRegister<T> {
  private readonly object _lock = new();


  public InterlockedShiftRegister() { }



  public InterlockedShiftRegister(T initialState)
    : base(initialState) { }



  public override T Shift(T nextState) {
    lock (_lock) {
      return base.Shift(nextState);
    }
  }
}
