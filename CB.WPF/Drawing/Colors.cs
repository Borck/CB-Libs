namespace CB.WPF.Drawing;

using System.Drawing;
using System.Text.RegularExpressions;

internal static class Colors {
  private static readonly Regex RegexColor = new("^#([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);
  private static readonly ColorConverter Converter = new();



  public static bool IsHexColor(string colorString) => RegexColor.IsMatch(colorString);



  public static Color Parse(string colorString) => (Color)Converter.ConvertFromString(colorString);
}
