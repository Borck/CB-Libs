namespace CB.System.Concurrent {
  public class ShiftRegister<T> {
    public T Current { get; protected set; }



    public ShiftRegister(T initialState = default(T)) {
      Current = initialState;
    }



    public virtual T Shift(T nextState) {
      var curr = Current;
      Current = nextState;
      return curr;
    }
  }
}
