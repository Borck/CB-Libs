namespace CB.System {
  public static class ObjectX {
    /// <summary>
    ///   Swaps the values of the two referenced objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="object1"></param>
    /// <param name="object2"></param>
    public static void Swap<T>(ref T object1, ref T object2) {
      var temp = object1;
      object1 = object2;
      object2 = temp;
    }
  }
}
