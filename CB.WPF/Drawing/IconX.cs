namespace CB.WPF.Drawing;

using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public static class IconX {
  public static ImageSource ToImageSource(this Icon icon) => Imaging.CreateBitmapSourceFromHIcon(
      icon.Handle,
      Int32Rect.Empty,
      BitmapSizeOptions.FromEmptyOptions()
    );
}
