using Xunit;



namespace CB.System.Collections {
  public class CollectionExtTests {
    [Fact]
    public void PrepandTest() {
      var values = new[] {1, 5};
      var valuesPrepanded = values.Prepand(8);

      Assert.Equal(new[] {8, 1, 5}, valuesPrepanded!);
    }
  }
}
