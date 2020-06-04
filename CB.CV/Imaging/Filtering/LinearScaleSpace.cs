using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Accord.Imaging;
using Accord.Imaging.Filters;



namespace CB.CV.Imaging.Filtering {
  public class LinearScaleSpace : BaseFilter {
    protected readonly GaussianBlur Filter1;
    protected readonly GaussianBlur Filter2;
    protected readonly GaussianBlur Filter3;

    private static readonly Dictionary<PixelFormat, PixelFormat> Ft = new Dictionary<PixelFormat, PixelFormat> {
      {PixelFormat.Format8bppIndexed, PixelFormat.Format8bppIndexed}
    };

    public override Dictionary<PixelFormat, PixelFormat> FormatTranslations =>
      new Dictionary<PixelFormat, PixelFormat>(Ft);



    public LinearScaleSpace(double sigma = Preferences.LSS_SIGMA) {
      Filter1 = new GaussianBlur(sigma / 4);
      Filter2 = new GaussianBlur(sigma / 2);
      Filter3 = new GaussianBlur(sigma);
    }



    protected override unsafe void ProcessFilter(UnmanagedImage srcImage, UnmanagedImage dstImage) {
      if (srcImage.Width < 3 ||
          srcImage.Height < 3) {
        throw new InvalidImagePropertiesException($"{srcImage} is to small. It has to have at least 3x3 pixels.");
      }

      var diff = CalcDifferenceOfGaussian2(srcImage);

      //zero crossing between successive difference of gaussian images

      var d = (byte*)dstImage.ImageData;
      var wb = dstImage.Width;

      // first line
      FillLineWithZeros(ref d, wb);

      // middle lines
      var j = wb;

      for (var ptrTo = d + wb * (dstImage.Height - 2); d < ptrTo;) {
        var e = d;
        var k = j;

        *(d++) = 0;
        j++;

        for (var ptrLine = d + wb - 2; d < ptrLine; d++, j++) {
          var val = SameSign(
            diff[j],
            diff[j - 1],
            diff[j + 1],
            diff[j - wb],
            diff[j + wb]
          );
          *d = val ? byte.MinValue : byte.MaxValue;
        }

        *(d++) = 0;
        j++;
        Debug.Assert((d - (byte*)dstImage.ImageData) % wb == 0);
        Debug.Assert(e + wb == d);
        Debug.Assert(k + wb == j);
      }

      //last line
      FillLineWithZeros(ref d, wb);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void FillLineWithZeros(ref byte* imagePtr, int imageWidth) {
      for (var ptrLine = imagePtr + imageWidth; imagePtr < ptrLine; imagePtr++) {
        *imagePtr = 0;
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SameSign(int a, int b, int c, int d, int e) {
      var posi = a >= 0;
      return posi == (b >= 0) && posi == (c >= 0) && posi == (d >= 0) && posi == (e >= 0);
    }



    private int[] CalcDifferenceOfGaussian2(UnmanagedImage srcImage) {
      //difference of gaussian
      var image1 = Filter1.Apply(srcImage);
      var image2 = Filter2.Apply(srcImage);

      var diff = image1.Subtract(image2);
      image1.Dispose();

      var image3 = Filter3.Apply(srcImage);
      var diff2 = image2.Subtract(image3);
      image2.Dispose();
      image3.Dispose();

      for (var i = 0; i < diff.Length; i++) {
        diff[i] -= diff2[i];
      }

      return diff;
    }
  }
}
