using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;



namespace CB.System {
  [TestFixture]
  public class AppDomainXTests {
    [Test]
    public void GetClassesTest_WithIList() {
      var classes = AppDomain.CurrentDomain.GetLoadableClasses( typeof(IList) ).ToArray();
      Assert.IsTrue( classes.Any() );
      foreach (var @class in classes) {
        Assert.IsTrue( typeof(IList).IsAssignableFrom( @class ) );
      }
    }



    [Test]
    public void GetClassesTest_WithThisClass() {
      var classes = AppDomain.CurrentDomain.GetLoadableClasses( typeof(AppDomainXTests) ).ToArray();
      CollectionAssert.AreEquivalent( classes, new[] {typeof(AppDomainXTests)} );
    }
  }
}
