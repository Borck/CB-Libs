namespace CB.System.Collections;

using global::System;
using global::System.Collections.Generic;

[Obsolete("Use System.Collections.Generic.IReadOnlyCollection<System.Tuple<T1,T2>> instead")]
public interface IReadOnlyTupleCollection<TFirst, TSecond> : IReadOnlyCollection<Tuple<TFirst, TSecond>> { }
