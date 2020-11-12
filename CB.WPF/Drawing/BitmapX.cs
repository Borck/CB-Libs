using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;



namespace CB.WPF.Drawing {
  public static class BitmapX {
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);



    /// <summary>
    ///   Creates a <see cref="System.Windows.Media.Imaging.BitmapSource" /> from a <see cref="System.Drawing.Bitmap" />
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public static BitmapSource CreateBitmapSource([NotNull] this Bitmap bitmap) {
      var hBitmap = bitmap.GetHbitmap();
      try {
        return Imaging.CreateBitmapSourceFromHBitmap(
          hBitmap,
          IntPtr.Zero,
          Int32Rect.Empty,
          BitmapSizeOptions.FromEmptyOptions()
        );
      } finally {
        DeleteObject(hBitmap);
      }
    }
  }
}
