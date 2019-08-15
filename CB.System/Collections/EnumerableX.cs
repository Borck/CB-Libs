using System;
using System.Collections.Generic;



namespace CB.System.Collections {
  public static class EnumerableX {
    public static bool TryGetFirst<TResult>(this IEnumerable<TResult> enumerable, out TResult value) {
      foreach (var elem in enumerable) {
        value = elem;
        return true;
      }

      value = default(TResult);
      return false;
    }



    public static IEnumerable<TResult> SelectWhile<TResult>(this IEnumerable<TResult> enumerable,
                                                            Predicate<TResult> predicate) {
      foreach (var item in enumerable)
        if (predicate( item ))
          yield return item;
        else
          yield break;
    }
  }
}
