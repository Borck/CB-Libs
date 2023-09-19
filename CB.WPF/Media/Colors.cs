using System;
using System.Drawing;



namespace CB.WPF.Media {
  [Obsolete("Use CB.WPF.Drawing.Colors instead")]
  public static class Colors {
    public static bool IsHexColor(string colorString) => Drawing.Colors.IsHexColor(colorString);



    public static Color Parse(string colorString) => Drawing.Colors.Parse(colorString);
  }
}
