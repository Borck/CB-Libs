using NUnit.Framework;



namespace CB.System.Concurrent {
  [TestFixture]
  public class ObservableRefTests {
    [Test]
    public void TestUpdateFieldValue_ChangeTwiceWithSameValue_ExpectNonChangedValue() {
      var obj = new object();
      var @ref = new ObservableRef<object> {
        Value = obj
      };

      @ref.ValueChanged += (_, args) => { Assert.IsFalse( args.ValueChanged ); };
      @ref.Value = obj;
    }



    [Test]
    public void TestUpdateFieldValue_ChangeValue_ExpectValueChanged() {
      var expectedObj = new object();

      var @ref = new ObservableRef<object> {
        Value = expectedObj
      };

      Assert.AreEqual( expectedObj, @ref.Value );
    }



    [Test]
    public void TestUpdateFieldValue_ChangeValue_ExpectValueChangedEvent() {
      var @ref = new ObservableRef<object>();

      var eventRaised = false;
      @ref.ValueChanged += (_, __) => { eventRaised = true; };
      @ref.Value = new object();

      Assert.IsTrue( eventRaised );
    }
  }
}
