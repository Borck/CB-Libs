namespace CB.WPF.Windows;

using System.Windows.Media;

public static class PixelFormatX {
  public static int GetBytesPerPixel(this PixelFormat pixelFormat) => (pixelFormat.BitsPerPixel + 7) >> 3;
}
