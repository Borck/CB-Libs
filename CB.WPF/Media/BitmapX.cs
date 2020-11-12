using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;



namespace CB.WPF.Media {
  [Obsolete("Use CB.WPF.Drawing.BitmapX instead")]
  public static class BitmapX {
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);



    /// <summary>
    ///   Creates a <see cref="System.Windows.Media.Imaging.BitmapSource" /> from a <see cref="System.Drawing.Bitmap" />
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public static BitmapSource CreateBitmapSource([NotNull] this Bitmap bitmap)
      => Drawing.BitmapX.CreateBitmapSource(bitmap);
  }
}
