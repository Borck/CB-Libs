using System;
using System.Drawing;
using Accord.Imaging;



namespace CB.CV.Imaging.Filtering {
  public class Colorization {
    public static unsafe void Colorize(UnmanagedImage image, Rectangle rect, int usedChannel) {
      var pb = image.GetPixelFormatSizeInBytes();
      if (usedChannel < 0 ||
          usedChannel >= pb) {
        throw new ArgumentException(nameof(usedChannel));
      }

      var d = (byte*)image.ImageData + rect.Y * image.Stride + rect.X * pb + usedChannel;
      var wd = Math.Min(rect.Width, image.Width - rect.X) * pb;

      var dPad = image.Stride - wd;
      for (var dTo = d + Math.Min(rect.Height, image.Height - rect.Y) * image.Stride; d < dTo; d += dPad) {
        for (var dToLn = d + wd; d < dToLn; d += pb) {
          *d = byte.MaxValue;
        }
      }
    }
  }
}
