using System;
using System.Linq;



namespace CB.System {
  public static class TypeX {
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
}
