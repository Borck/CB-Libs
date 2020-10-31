using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CB.WPF.Windows;



namespace CB.WPF.Drawing {
  public static class ImageSourceX {
    private static readonly IReadOnlyDictionary<PixelFormat, Func<byte[], Color>> PixelToColorCreators =
      new Dictionary<PixelFormat, Func<byte[], Color>> {
        {PixelFormats.Bgra32, bytes => Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0])},
        {PixelFormats.Bgr32, bytes => Color.FromRgb(bytes[2], bytes[1], bytes[0])},
        {PixelFormats.Bgr24, bytes => Color.FromRgb(bytes[2], bytes[1], bytes[0])},
        {PixelFormats.Pbgra32, bytes => Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0])}
      };



    public static Color GetPixelColor(this BitmapSource bitmap, int x, int y) {
      var bytesPerPixel = bitmap.Format.GetBytesPerPixel();
      var bytes = new byte[bytesPerPixel];
      var rect = new Int32Rect(x, y, 1, 1);

      bitmap.CopyPixels(rect, bytes, bytesPerPixel, 0);
      return PixelToColorCreators[bitmap.Format]
        .Invoke(bytes);
    }



    public static void Save(this BitmapSource bitmap, string filePath) {
      BitmapEncoder encoder = new PngBitmapEncoder();
      encoder.Frames.Add(BitmapFrame.Create(bitmap));
      // ReSharper disable once ConvertToUsingDeclaration
      using (var file = File.OpenWrite(filePath)) {
        encoder.Save(file);
      }
    }
  }
}
