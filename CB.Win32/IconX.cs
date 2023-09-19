using System;
using System.Drawing;



namespace CB.Win32 {
  [Obsolete("Use Icons class instead")]
  public static class IconX {
    public static Icon? ExtractAssociatedIcon(string path, ushort size) => Icons.ExtractAssociatedIcon(path, size);



    public static Icon? ExtractIconFromResource(string filename, int iconIndex, ushort size) =>
      Icons.ExtractIconFromResource(filename, iconIndex, size);



    public static Icon? ExtractIconFromResource(string filename, ushort size) =>
      Icons.ExtractIconFromResource(filename, size);



    public static Icon? ExtractIconFromExe(string file, bool large, int iconIndex = 0) =>
      Icons.ExtractIconFromExe(file, large, iconIndex);
  }
}
