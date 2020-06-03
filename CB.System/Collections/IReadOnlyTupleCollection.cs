using System;
using System.Collections.Generic;



namespace CB.System.Collections {
  [Obsolete( "Use System.Collections.Generic.IReadOnlyCollection<System.Tuple<T1,T2>> instead" )]
  public interface IReadOnlyTupleCollection<TFirst, TSecond> : IReadOnlyCollection<Tuple<TFirst, TSecond>> { }
}
