using System;
using System.Collections.Generic;



namespace CB.System.Collections {
  public static class ListX {
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> items) {
      foreach (var item in items)
        list.Add( item );
    }



    public static void RemoveRange<T>(this IList<T> collection, int index, int count) {
      for (var i = index + count - 1; i >= index; i--)
        collection.RemoveAt( i );
    }



    /// <summary>
    ///   Removes or add elements from/to the right side of the collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="newSize"></param>
    /// <param name="newEntry"></param>
    /// <returns>Size difference</returns>
    public static int Resize<T>(this IList<T> collection, int newSize, Func<T> newEntry) {
      var oldSize = collection.Count;
      var difference = newSize - oldSize;

      if (difference > 0)
        for (var i = oldSize; i < newSize; i++)
          collection.Add( newEntry() );
      else if (difference < 0)
        collection.RemoveRange( oldSize, difference );
      return difference;
    }



    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T[] Redim1D<T>(this List<T[]> array) {
      // TODO: test it!
      var length = 0;
      foreach (var part in array)
        if (part != null)
          length += part.Length;

      var result = new T[length];

      var pos = 0;
      foreach (var part in array) {
        if (part == null)
          continue;
        Array.Copy( part, 0, result, pos, part.Length );
        pos += part.Length;
      }

      return result;
    }
  }
}
