namespace CB.System.Reflection;

using global::System;
using global::System.Linq;
using global::System.Reflection;

public static class MemberInfoX {
  public static bool TryGetCustomAttribute<TAttribute>(this MemberInfo memberInfo, out TAttribute attribute)
    where TAttribute : Attribute {
    attribute = memberInfo.GetCustomAttribute(typeof(TAttribute)) as TAttribute;
    return attribute != null;
  }



  public static bool TryGetCustomAttribute<TAttribute>(
    this MemberInfo memberInfo,
    bool inherit,
    out TAttribute attribute)
    where TAttribute : Attribute {
    attribute = memberInfo.GetCustomAttributes(
                            typeof(TAttribute),
                            inherit
                          )
                          .FirstOrDefault() as TAttribute;
    return attribute != null;
  }
}
