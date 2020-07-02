using System.Drawing;
using System.Windows.Media;



namespace CB.WPF.Media {
  public static class IconX {
    public static ImageSource ToImageSource(this Icon icon) => Drawing.IconX.ToImageSource(icon);
  }
}
