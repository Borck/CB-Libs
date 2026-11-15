namespace CB.System;

using global::System;
using global::System.Linq;

public static class TypeX {
  [Obsolete("Use CB.System.Reflection.MemberInfoX.TryGetCustomAttribute(...) instead")]
  public static bool TryGetCustomAttribute<TAttribute>(
    this Type type,
    bool inherit,
    out TAttribute attribute)
    where TAttribute : Attribute {
    attribute = type.GetCustomAttributes(
                      typeof(TAttribute),
                      inherit
                    )
                    .FirstOrDefault() as TAttribute;
    return attribute != null;
  }
}
