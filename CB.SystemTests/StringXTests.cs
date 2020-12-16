using Xunit;



namespace CB.System {
  public class StringXTests {
    [Fact]
    public void Separate_CommaPairSeparatedString_ExpectSeparated() {
      var @string = "123,,431,,33";
      var (left, right) = @string.Separate(",,");
      Assert.Equal("123", left);
      Assert.Equal("431,,33", right);
    }



    [Fact]
    public void Separate_CommaPairOnly_ExpectSeparated() {
      var @string = ",,";
      var (left, right) = @string.Separate(",,");
      Assert.Equal(string.Empty, left);
      Assert.Equal(string.Empty, right);
    }



    [Fact]
    public void SeparateLast_CommaPairSeparatedString_ExpectSeparated() {
      var @string = "123,,431,,33";
      var (left, right) = @string.SeparateLast(",,");
      Assert.Equal("123,,431", left);
      Assert.Equal("33", right);
    }



    [Fact]
    public void SeparateLast_CommaPairOnly_ExpectSeparated() {
      var @string = ",,";
      var (left, right) = @string.SeparateLast(",,");
      Assert.Equal(string.Empty, left);
      Assert.Equal(string.Empty, right);
    }
  }
}
