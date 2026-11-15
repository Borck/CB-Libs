namespace CB.System.Collections;

using Xunit;

public class CollectionXTests {
  [Fact]
  public void PrepandTest() {
    var values = new[] { 1, 5 };
    var valuesPrepanded = values.Prepand(8);

    Assert.Equal(new[] { 8, 1, 5 }, valuesPrepanded!);
  }
}
