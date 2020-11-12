using System;
using System.Drawing;
using JetBrains.Annotations;



namespace CB.WPF.Media {
  [Obsolete("Use CB.WPF.Drawing.Colors instead")]
  public static class Colors {
    public static bool IsHexColor(string colorString) => Drawing.Colors.IsHexColor(colorString);



    public static Color Parse([NotNull] string colorString) => Drawing.Colors.Parse(colorString);
  }
}
