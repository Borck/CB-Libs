using System.Collections.Generic;



namespace CB.System.Collections {
  public static class ListX {
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> items) {
      foreach (var item in items)
        list.Add( item );
    }
  }
}
