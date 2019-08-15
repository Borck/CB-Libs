using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



namespace CB.System.Reflection {
  public static class AssemblyX {
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly) {
      // TODO: Argument validation
      try {
        return assembly.GetTypes();
      } catch (ReflectionTypeLoadException e) {
        return e.Types.Where( t => t != null );
      }
    }



    public static IEnumerable<Type> GetLoadableClasses(this Assembly assembly, Type assignableType) {
      return GetLoadableTypes( assembly )
        .Where( type => type.IsClass && assignableType.IsAssignableFrom( type ) );
    }
  }
}
