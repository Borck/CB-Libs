namespace CB.System;

using global::System;

public static class LazyX {
  public static bool TryGetValueIfCreate<T>(this Lazy<T> lazy, out T value) {
    var isValueCreated = lazy.IsValueCreated;
    value = isValueCreated ? lazy.Value : default;
    return isValueCreated;
  }
}
