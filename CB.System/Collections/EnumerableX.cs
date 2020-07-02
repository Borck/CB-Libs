using System;
using System.Collections.Generic;
using System.Linq;



namespace CB.System.Collections {
  public static class EnumerableX {
    public static bool TryGetFirst<TResult>(this IEnumerable<TResult> enumerable, out TResult value) {
      using (var enumerator = enumerable.GetEnumerator()) {
        if (enumerator.MoveNext()) {
          value = enumerator.Current;
          return true;
        }
      }

      value = default;
      return false;
    }



    public static IEnumerable<TResult> SelectWhile<TResult>(this IEnumerable<TResult> enumerable,
                                                            Predicate<TResult> predicate) {
      foreach (var item in enumerable) {
        if (predicate(item)) {
          yield return item;
        } else {
          yield break;
        }
      }
    }



    public static (IEnumerable<T> added, IEnumerable<T> removed) Delta<T>(
      this IEnumerable<T> enumerableAfter,
      IEnumerable<T> enumerableBefore) {
      return (enumerableAfter
                ?.Except(enumerableBefore ?? Enumerable.Empty<T>()) ??
              Enumerable.Empty<T>(),
               enumerableBefore
                 ?.Except(enumerableAfter ?? Enumerable.Empty<T>()) ??
               Enumerable.Empty<T>());
    }



    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action) {
      foreach (var item in enumerable) {
        action(item);
      }
    }



    public static IEnumerable<(T left, T right)> AsTuples<T>(this IEnumerable<T> enumerable) {
      using (var eTor = enumerable.GetEnumerator()) {
        while (eTor.MoveNext()) {
          var left = eTor.Current;
          yield return eTor.MoveNext()
                         ? (left, eTor.Current)
                         : throw new ArgumentException("Enumerable length is not times of two");
        }
      }
    }



    public static IEnumerable<(T left, T middle, T right)> AsTriples<T>(this IEnumerable<T> enumerable) {
      using (var eTor = enumerable.GetEnumerator()) {
        while (eTor.MoveNext()) {
          var left = eTor.Current;
          if (!eTor.MoveNext()) {
            throw new ArgumentException("Enumerable length is not times of three");
          }

          var middle = eTor.Current;
          yield return eTor.MoveNext()
                         ? (left, middle, eTor.Current)
                         : throw new ArgumentException("Enumerable length is not times of three");
        }
      }
    }
  }
}
