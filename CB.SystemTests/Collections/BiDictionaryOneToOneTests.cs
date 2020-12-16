using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;



namespace CB.System.Collections {
  public class BiDictionaryOneToOneTests {
    [Fact]
    public void AddTest_DuplicateFirst_Fail() {
      object Check()
        => new BiDictionaryOneToOne<int, int> {{0, 815}, {0, 888},};

      Assert.Throws<ArgumentException>(Check);
    }



    [Fact]
    public void AddTest_DuplicateSecond_Fail() {
      object Check()
        => new BiDictionaryOneToOne<int, int> {{0, 815}, {42, 815},};

      Assert.Throws<ArgumentException>(Check);
    }



    [Fact]
    public void AddTest_Single_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815}};

      Assert.Equal(dict.ToArray(), new[] {new KeyValuePair<int, int>(0, 815)});
    }



    [Fact]
    public void GetFirstTest_Exists_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};
      Assert.Equal(0, dict.GetFirst(815));
    }



    [Fact]
    public void GetFirstTest_Missing_Fail() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};
      object Check() => dict.GetFirst(888);
      Assert.Throws<ArgumentException>(Check);
    }



    [Fact]
    public void GetTest_Exists_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};
      Assert.Equal(815, dict.Get(0));
    }



    [Fact]
    public void GetTest_Missing_Fail() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};

      object Check() => dict.Get(42);
      Assert.Throws<ArgumentException>(Check);
    }



    [Fact]
    public void TestTest() {
      var dictionary = new Dictionary<int, int> {{4, 3}};
      var result = dictionary.TryGetValue(6574, out _);
      Assert.False(result);
    }



    [Fact]
    public void TryAddTest_Duplicate_Fail() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};
      Assert.False(dict.TryAdd(0, 888));
    }



    [Fact]
    public void TryAddTest_Unique_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};
      Assert.True(dict.TryAdd(42, 888));
    }



    [Fact]
    public void TryGetFirstTest_Exists_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};
      Assert.True(dict.TryGetFirst(815, out var first));
      Assert.Equal(0, first);
    }



    [Fact]
    public void TryGetFirstTest_Missing_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};
      Assert.False(dict.TryGetFirst(888, out var unused));
    }



    [Fact]
    public void TryGetValueTest_Exists_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};
      Assert.True(dict.TryGetValue(0, out var second));
      Assert.Equal(815, second);
    }



    [Fact]
    public void TryGetValueTest_Missing_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815},};
      Assert.False(dict.TryGetValue(42, out _));
    }
  }
}
