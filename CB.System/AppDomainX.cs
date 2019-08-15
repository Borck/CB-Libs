using System;
using System.Collections.Generic;
using System.Linq;
using CB.System.Reflection;



namespace CB.System {
  public static class AppDomainX {
    public static IEnumerable<Type> GetLoadableClasses(this AppDomain domain, Type assignableType) {
      return domain
             .GetAssemblies()
             .SelectMany( assembly => assembly.GetLoadableClasses( assignableType ) );
    }
  }
}
