using System;
using JetBrains.Annotations;



namespace CB.System.Collections {
  public class ReadonlyIndexedProperty<TIndex, TValue> {
    private readonly Func<TIndex, TValue> _getter;



    public ReadonlyIndexedProperty([NotNull] Func<TIndex, TValue> getter) {
      _getter = getter;
    }



    public TValue this[TIndex index] => _getter( index );
  }
}
