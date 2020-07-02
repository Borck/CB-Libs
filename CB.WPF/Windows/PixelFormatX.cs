using System.Windows.Media;



namespace CB.WPF.Windows {
  public static class PixelFormatX {
    public static int GetBytesPerPixel(this PixelFormat pixelFormat) {
      return (pixelFormat.BitsPerPixel + 7) >> 3;
    }
  }
}
