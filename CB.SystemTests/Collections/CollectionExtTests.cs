using NUnit.Framework;



namespace CB.System.Collections {
  [TestFixture]
  public class CollectionExtTests {
    [Test]
    public void PrepandTest() {
      var values = new[] {1, 5};
      var valuesPrepanded = values.Prepand( 8 );

      CollectionAssert.AreEqual( new[] {8, 1, 5}, valuesPrepanded );
    }
  }
}
