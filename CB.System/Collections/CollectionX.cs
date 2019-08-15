using System.Collections;
using JetBrains.Annotations;



namespace CB.System.Collections {
  public static class CollectionX {
    public static bool ContainsIndex([NotNull] this ICollection collection, int index) {
      return index >= 0 && index < collection.Count;
    }
  }
}
