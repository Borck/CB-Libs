using System;
using System.Drawing;
using System.Windows.Media;



namespace CB.WPF.Media {
  [Obsolete("Use CB.WPF.Drawing.IconX instead")]
  public static class IconX {
    public static ImageSource ToImageSource(this Icon icon) => Drawing.IconX.ToImageSource(icon);
  }
}
