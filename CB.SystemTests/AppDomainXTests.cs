using System;
using System.Collections;
using System.Linq;
using Xunit;



namespace CB.System {
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
      Assert.Equal(classes, new[] {typeof(AppDomainXTests)});
    }
  }
}
