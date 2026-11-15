namespace CB.System;

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using CB.System.Reflection;

public static class AppDomainX {
  public static IEnumerable<Type> GetLoadableClasses(this AppDomain domain, Type assignableType) => domain
           .GetAssemblies()
           .SelectMany(assembly => assembly.GetLoadableClasses(assignableType));
}
