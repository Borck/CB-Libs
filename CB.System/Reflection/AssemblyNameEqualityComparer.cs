namespace CB.System.Reflection;

using global::System;
using global::System.Collections.Generic;
using global::System.Reflection;



/// <summary>Compares assemblies by their full name (name, version, culture, public key token), ordinally.</summary>
public sealed class AssemblyNameEqualityComparer : IEqualityComparer<AssemblyName> {
  public bool Equals(AssemblyName? x, AssemblyName? y) =>
    ReferenceEquals(x, y) || x is not null && y is not null && string.Equals(x.FullName, y.FullName, StringComparison.Ordinal);



  public int GetHashCode(AssemblyName obj) {
    ArgumentNullException.ThrowIfNull(obj);

    return StringComparer.Ordinal.GetHashCode(obj.FullName);
  }
}
