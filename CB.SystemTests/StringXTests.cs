using NUnit.Framework;



namespace CB.System {
  [TestFixture]
  public class StringXTests {
    [Test]
    public void Separate_CommaPairSeparatedString_ExpectSeparated() {
      var @string = "123,,431,,33";
      var (left, right) = @string.Separate(",,");
      Assert.AreEqual("123", left);
      Assert.AreEqual("431,,33", right);
    }



    [Test]
    public void Separate_CommaPairOnly_ExpectSeparated() {
      var @string = ",,";
      var (left, right) = @string.Separate(",,");
      Assert.AreEqual(string.Empty, left);
      Assert.AreEqual(string.Empty, right);
    }



    [Test]
    public void SeparateLast_CommaPairSeparatedString_ExpectSeparated() {
      var @string = "123,,431,,33";
      var (left, right) = @string.SeparateLast(",,");
      Assert.AreEqual("123,,431", left);
      Assert.AreEqual("33", right);
    }



    [Test]
    public void SeparateLast_CommaPairOnly_ExpectSeparated() {
      var @string = ",,";
      var (left, right) = @string.SeparateLast(",,");
      Assert.AreEqual(string.Empty, left);
      Assert.AreEqual(string.Empty, right);
    }
  }
}
