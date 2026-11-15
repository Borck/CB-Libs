namespace CB.System.Reflection;

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Reflection;

public static class AssemblyX {
  public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly) {
    // TODO: Argument validation
    try {
      return assembly.GetTypes();
    } catch (ReflectionTypeLoadException e) {
      return e.Types.Where(t => t != null);
    }
  }



  public static IEnumerable<Type> GetLoadableClasses(this Assembly assembly, Type assignableType) => GetLoadableTypes(assembly)
      .Where(type => type.IsClass && assignableType.IsAssignableFrom(type));
}
