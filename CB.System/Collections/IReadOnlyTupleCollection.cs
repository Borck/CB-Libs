using System;
using System.Collections.Generic;



namespace CB.System.Collections {
  public interface IReadOnlyTupleCollection<TFirst, TSecond> : IReadOnlyCollection<Tuple<TFirst, TSecond>> { }
}
