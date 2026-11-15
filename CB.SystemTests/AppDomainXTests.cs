namespace CB.System;

using global::System;
using global::System.Collections;
using global::System.Linq;
using Xunit;

public class AppDomainXTests {
  [Fact]
  public void GetClassesTest_WithIList() {
    var classes = AppDomain.CurrentDomain.GetLoadableClasses(typeof(IList)).ToArray();
    Assert.True(classes.Any());
    foreach (var @class in classes) {
      Assert.True(typeof(IList).IsAssignableFrom(@class));
    }
  }



  [Fact]
  public void GetClassesTest_WithThisClass() {
    var classes = AppDomain.CurrentDomain.GetLoadableClasses(typeof(AppDomainXTests)).ToArray();
    Assert.Equal(classes, new[] { typeof(AppDomainXTests) });
  }
}
