﻿using Xunit;



namespace CB.System.Collections {
  public class ObservableDictionaryTests {
    [Fact]
    public void ObservableDictionaryTest() {
      var objExpected = new object();

      var dictionary = new ObservableDictionary<MyKey, object> {{new MyKey(815), objExpected}};

      var obj = dictionary[new MyKey(815)];
      Assert.Equal(obj, objExpected);
    }



    private class MyKey {
      private readonly int _id;



      public MyKey(int id) {
        _id = id;
      }



      public override bool Equals(object obj)
        => Equals(obj as MyKey);



      private bool Equals(MyKey other)
        => other != null && _id == other._id;



      public override int GetHashCode()
        => _id;
    }
  }
}
