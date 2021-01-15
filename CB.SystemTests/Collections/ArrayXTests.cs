using System;
using Xunit;



namespace CB.System.Collections {
  public class ArrayXTests {
    public class CollectionXTests {
      [Fact]
      public void Crop_0ValuesWithThreshold_ExpectOnlyValueAboveThreshold() {
        var bytes = new byte[] {4, 3, 5};
        ArrayX.Crop(ref bytes, b => b > 3);

        Assert.Equal(new byte[] {4, 5}, bytes);
      }



      [Fact]
      public void Crop_3ValuesWithThreshold_ExpectOnlyValueAboveThreshold() {
        var bytes = new byte[] {4, 3, 5};
        ArrayX.Crop(ref bytes, b => b > 3);

        Assert.Equal(new byte[] {4, 5}, bytes);
      }



      [Fact]
      public void Crop_8ValuesWithThreshold_ExpectOnlyValueAboveThreshold() {
        var bytes = new byte[] { };
        ArrayX.Crop(ref bytes, b => b > 3);
        Assert.Equal(Array.Empty<byte>(), bytes);
      }
    }
  }
}
