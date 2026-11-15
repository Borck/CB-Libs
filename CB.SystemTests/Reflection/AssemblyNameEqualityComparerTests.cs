namespace CB.System.Reflection;

using global::System.Collections.Generic;
using global::System.Reflection;
using Xunit;



public class AssemblyNameEqualityComparerTests {
  private readonly AssemblyNameEqualityComparer _comparer = new();



  [Fact]
  public void Equals_WithSameFullName_ExpectTrueAndSameHash() {
    var first = new AssemblyName("Some.Assembly, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null");
    var second = new AssemblyName("Some.Assembly, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null");

    Assert.True(_comparer.Equals(first, second));
    Assert.Equal(_comparer.GetHashCode(first), _comparer.GetHashCode(second));
  }



  [Fact]
  public void Equals_WithDifferentVersion_ExpectFalse() =>
    Assert.False(_comparer.Equals(new AssemblyName("Some.Assembly, Version=1.0.0.0"), new AssemblyName("Some.Assembly, Version=2.0.0.0")));



  [Fact]
  public void Equals_WithDifferentCase_ExpectFalse() =>
    Assert.False(_comparer.Equals(new AssemblyName("Some.Assembly"), new AssemblyName("some.assembly")));



  [Fact]
  public void Equals_WithNulls_ExpectTrueOnlyForTwoNulls() {
    var name = new AssemblyName("Some.Assembly");

    Assert.True(_comparer.Equals(null, null));
    Assert.False(_comparer.Equals(name, null));
    Assert.False(_comparer.Equals(null, name));
  }



  [Fact]
  public void HashSet_WithEqualNamesFromSeparateInstances_ExpectOneEntry() {
    var set = new HashSet<AssemblyName>(_comparer) {
      new("Some.Assembly, Version=1.0.0.0"),
      new("Some.Assembly, Version=1.0.0.0"),
      new("Other.Assembly, Version=1.0.0.0")
    };

    Assert.Equal(2, set.Count);
  }
}
