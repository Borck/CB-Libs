namespace CB.WPF.Media;

using System;
using System.Drawing;
using System.Windows.Media;

[Obsolete("Use CB.WPF.Drawing.IconX instead")]
public static class IconX {
  public static ImageSource ToImageSource(this Icon icon) => CB.WPF.Drawing.IconX.ToImageSource(icon);
}
