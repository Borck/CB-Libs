using System.Collections;



namespace CB.System.Collections {
  public static class CollectionX {
    public static bool ContainsIndex(this ICollection collection, int index) {
      return index >= 0 && index < collection.Count;
    }
  }
}
