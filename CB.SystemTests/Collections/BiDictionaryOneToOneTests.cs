using System;
using System.Collections.Generic;
using NUnit.Framework;



namespace CB.System.Collections {
  [TestFixture]
  public class BiDictionaryOneToOneTests {
    [Test]
    public void AddTest_DuplicateFirst_Fail() {
      object Check()
        => new BiDictionaryOneToOne<int, int> {
          {0, 815},
          {0, 888},
        };

      Assert.That( Check, Throws.ArgumentException );
    }



    [Test]
    public void AddTest_DuplicateSecond_Fail() {
      object Check()
        => new BiDictionaryOneToOne<int, int> {
          {0, 815},
          {42, 815},
        };

      Assert.That( Check, Throws.ArgumentException );
    }



    [Test]
    public void AddTest_Single_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {{0, 815}};
      CollectionAssert.AreEquivalent( dict, new Dictionary<int, int> {{0, 815}} );
    }



    [Test]
    public void GetFirstTest_Exists_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };
      Assert.AreEqual( dict.GetFirst( 815 ), 0 );
    }



    [Test]
    public void GetFirstTest_Missing_Fail() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };
      object Check() => dict.GetFirst( 888 );
      Assert.That( Check, Throws.ArgumentException );
    }



    [Test]
    public void GetTest_Exists_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };
      Assert.AreEqual( dict.Get( 0 ), 815 );
    }



    [Test]
    public void GetTest_Missing_Fail() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };

      object Check() => dict.Get( 42 );
      Assert.That( Check, Throws.ArgumentException );
    }



    [Test]
    public void TestTest() {
      var dictionary = new Dictionary<int, int>() {{4, 3}};
      var result = dictionary.TryGetValue( 6574, out var value );
      Assert.IsFalse( result );
      Console.WriteLine( value );
    }



    [Test]
    public void TryAddTest_Duplicate_Fail() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };
      Assert.IsFalse( dict.TryAdd( 0, 888 ) );
    }



    [Test]
    public void TryAddTest_Unique_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };
      Assert.IsTrue( dict.TryAdd( 42, 888 ) );
    }



    [Test]
    public void TryGetFirstTest_Exists_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };
      Assert.IsTrue( dict.TryGetFirst( 815, out var first ) );
      Assert.AreEqual( first, 0 );
    }



    [Test]
    public void TryGetFirstTest_Missing_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };
      Assert.IsFalse( dict.TryGetFirst( 888, out var unused ) );
    }



    [Test]
    public void TryGetValueTest_Exists_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };
      Assert.IsTrue( dict.TryGetValue( 0, out var second ) );
      Assert.AreEqual( second, 815 );
    }



    [Test]
    public void TryGetValueTest_Missing_Pass() {
      var dict = new BiDictionaryOneToOne<int, int> {
        {0, 815},
      };
      Assert.IsFalse( dict.TryGetValue( 42, out var unused ) );
    }
  }
}
