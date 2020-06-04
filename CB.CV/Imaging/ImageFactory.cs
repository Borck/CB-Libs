using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Accord;
using Accord.Imaging;
using JetBrains.Annotations;



namespace CB.CV.Imaging {
  public class ImageSampleFactory {
    public static UnmanagedImage GenerateBlackWhite(int width,
                                                    int height,
                                                    Func<int, int, bool> predicate,
                                                    PixelFormat format = PixelFormat.Format24bppRgb) {
      var n = width * height / 2;
      var whitePixels = new List<IntPoint>(n);
      var blackPixels = new List<IntPoint>(width * height - n);
      // horizontal stripes
      for (var y = 0; y < height; y++) {
        for (var x = 0; x < width; x++) {
          (predicate(x, y) ? whitePixels : blackPixels).Add(new IntPoint(x, y));
        }
      }

      var result = UnmanagedImage.Create(width, height, format);
      result.SetPixels(whitePixels, Color.White);
      result.SetPixels(blackPixels, Color.Black);
      return result;
    }



    public static UnmanagedImage GenerateHoriStripes(int width, int height) {
      return GenerateBlackWhite(width, height, (x, y) => y % 2 == 1);
    }



    public static UnmanagedImage GenerateHoriStripes(int width, int height, int lineWidth) {
      return GenerateBlackWhite(width, height, (x, y) => y / lineWidth % 2 == 1);
    }



    public static UnmanagedImage GenerateVertStripes(int width, int height) {
      return GenerateBlackWhite(width, height, (x, y) => x % 2 == 1);
    }



    public static UnmanagedImage GenerateVertStripes(int width, int height, int lineWidth) {
      return GenerateBlackWhite(width, height, (x, y) => x / lineWidth % 2 == 1);
    }



    public static UnmanagedImage GenerateChessBoard(int width, int height) {
      return GenerateBlackWhite(width, height, (x, y) => (x + y) % 2 == 1);
    }



    public static UnmanagedImage GenerateDiagStripes(int width, int height, int lineWidth = 2) {
      return GenerateBlackWhite(width, height, (x, y) => (x + y) / lineWidth % 2 == 1);
    }



    public static UnmanagedImage GenerateDiagStripes(int width, int height, PixelFormat format, int lineWidth = 2) {
      return GenerateBlackWhite(width, height, (x, y) => (x + y) / lineWidth % 2 == 1, format);
    }



    public static UnmanagedImage GenerateEmpty(int width, int height) {
      return UnmanagedImage.Create(width, height, PixelFormat.Format24bppRgb);
    }



    //private static Bitmap BitmapFromSource(BitmapSource bitmapsource) {
    //  Bitmap bitmap;
    //  using (var outStream = new MemoryStream()) {
    //    BitmapEncoder enc = new BmpBitmapEncoder();
    //    enc.Frames.Add(BitmapFrame.Create(bitmapsource));
    //    enc.Save(outStream);
    //    bitmap = new Bitmap(outStream);
    //  }
    //  return bitmap;
    //}



    public static UnmanagedImage Open([NotNull] string filename) {
      if (filename.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
          filename.EndsWith(".tif", StringComparison.OrdinalIgnoreCase)) {
        // Open a Stream and decode a TIFF image
        throw new NotImplementedException();
        //var imageStreamSource = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        //var decoder = new TiffBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
        //return UnmanagedImage.FromManagedImage(BitmapFromSource(decoder.Frames[0]));
      }

      return UnmanagedImage.FromManagedImage(new Bitmap(filename));
    }
  }
}
