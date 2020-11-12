using System.Drawing;
using System.Text.RegularExpressions;
using JetBrains.Annotations;



namespace CB.WPF.Drawing {
  static class Colors {
    private static readonly Regex RegexColor = new Regex("^#([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);
    private static readonly ColorConverter Converter = new ColorConverter();



    public static bool IsHexColor(string colorString) {
      return RegexColor.IsMatch(colorString);
    }



    public static Color Parse([NotNull] string colorString) {
      return (Color)Converter.ConvertFromString(colorString);
    }
  }
}
