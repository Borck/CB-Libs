namespace CB.Reactive;

public enum BoolToCommandTrigger {
  FromFalseToTrue,
  FromTrueToFalse,
  FromAnyToTrue,
  FromAnyToFalse,
  Changed,
  Equal
}
