using System;
using Xunit;



namespace CB.System.Collections {
  public class ArrayXTests {
    public class CollectionXTests {
      [Fact]
      public void Copy_ArrayWithThreeRandomInt_ExpectArraysBeforeAndAfterAreEqual() {
        var source = RandomInts(3);
        var destination = new int[source.Length];
        source.Copy(destination);

        Assert.Equal(source, destination);
      }



      [Fact]
      public void Copy_LengthLongerThanSourceIntArray_ExpectArgumentOutOfRangeException() {
        var source = RandomInts(3);
        var destination = new int[source.Length + 1];
        Assert.Throws<ArgumentOutOfRangeException>(() => source.Copy(0, destination, 0, source.Length + 1));
      }
      [Fact]
      public void Copy_LengthLongerThanDestinationIntArray_ExpectArgumentOutOfRangeException() {
        var source = RandomInts(3);
        var destination = new int[source.Length - 1];
        Assert.Throws<ArgumentOutOfRangeException>(() => source.Copy(0, destination, 0, source.Length));
      }



      [Fact]
      public void Crop_0ValuesWithThreshold_ExpectOnlyValueAboveThreshold() {
        var bytes = new byte[] { 4, 3, 5 };
        ArrayX.Crop(ref bytes, b => b > 3);

        Assert.Equal(new byte[] { 4, 5 }, bytes);
      }



      [Fact]
      public void Crop_3ValuesWithThreshold_ExpectOnlyValueAboveThreshold() {
        var bytes = new byte[] { 4, 3, 5 };
        ArrayX.Crop(ref bytes, b => b > 3);

        Assert.Equal(new byte[] { 4, 5 }, bytes);
      }



      [Fact]
      public void Crop_8ValuesWithThreshold_ExpectOnlyValueAboveThreshold() {
        var bytes = new byte[] { };
        ArrayX.Crop(ref bytes, b => b > 3);
        Assert.Equal(Array.Empty<byte>(), bytes);
      }



      public static int[] RandomInts(int length) {
        var random = new Random();
        var values = new int[length];
        for (var i = values.Length - 1; i >= 0; i--) {
          values[i] = random.Next();
        }
        return values;
      }
    }
  }
}
