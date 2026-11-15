namespace CB.System.Collections;

using global::System.Collections;

public static class CollectionX {
  public static bool ContainsIndex(this ICollection collection, int index) => index >= 0 && index < collection.Count;
}
