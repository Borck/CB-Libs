using System;



namespace CB.System.Collections {
  public class IndexedProperty<TIndex, TValue> {
    private readonly Func<TIndex, TValue> _getter;
    private readonly Action<TIndex, TValue> _setter;



    public IndexedProperty(
      Func<TIndex, TValue> getter,
      Action<TIndex, TValue> setter) {
      _getter = getter;
      _setter = setter;
    }



    public TValue this[TIndex index] {
      get => _getter(index);
      set => _setter(index, value);
    }
  }
}
