using System;
using System.Collections.Generic;
using System.Linq;



namespace CB.System.Collections {
  public static class EnumerableX {
    /// <summary>Projects each element of a sequence into a new form depending on a TryFunc returning true and a out value.</summary>
    public static IEnumerable<TOut> SelectWhere<T, TOut>(this IEnumerable<T> enumerable, TryFunc<T, TOut> trySelector) {
      foreach (var item in enumerable) {
        if (trySelector(item, out var value)) {
          yield return value;
        }
      }
    }



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
