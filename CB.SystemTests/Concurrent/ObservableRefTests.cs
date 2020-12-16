using Xunit;



namespace CB.System.Concurrent {
  public class ObservableRefTests {
    [Fact]
    public void TestUpdateFieldValue_ChangeTwiceWithSameValue_ExpectNonChangedValue() {
      var obj = new object();
      var @ref = new ObservableRef<object> {Value = obj};

      @ref.ValueChanged += (_, args) => { Assert.False(args.ValueChanged); };
      @ref.Value = obj;
    }



    [Fact]
    public void TestUpdateFieldValue_ChangeValue_ExpectValueChanged() {
      var expectedObj = new object();

      var @ref = new ObservableRef<object> {Value = expectedObj};

      Assert.Equal(expectedObj, @ref.Value);
    }



    [Fact]
    public void TestUpdateFieldValue_ChangeValue_ExpectValueChangedEvent() {
      var @ref = new ObservableRef<object>();

      var eventRaised = false;
      @ref.ValueChanged += (_, __) => { eventRaised = true; };
      @ref.Value = new object();

      Assert.True(eventRaised);
    }
  }
}
