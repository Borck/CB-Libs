using System;
using System.Drawing;
using System.Windows.Media.Imaging;



namespace CB.WPF.Media {
  [Obsolete("Use CB.WPF.Drawing.BitmapX instead")]
  public static class BitmapX {
    /// <summary>
    ///   Creates a <see cref="System.Windows.Media.Imaging.BitmapSource" /> from a <see cref="System.Drawing.Bitmap" />
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public static BitmapSource CreateBitmapSource(this Bitmap bitmap)
      => Drawing.BitmapX.CreateBitmapSource(bitmap);
  }
}
