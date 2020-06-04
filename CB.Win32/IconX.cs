using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CB.System;
using CB.Win32.Registry;
using Microsoft.Win32;



namespace CB.Win32 {
  public static class IconX {
    public static Icon ExtractAssociatedIcon(string path, ushort size) {
      if (File.Exists(path)) {
        return ExtractIconFromResource(path, size);
      }

      var dirInfo = new DirectoryInfo(path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
      if (dirInfo.Parent == null) {
        return ExtractAssociatedDriveIcon(path, size);
      }

      throw new NotImplementedException("Reading associated icon in not yet supported for " + path);
    }



    private static Icon ExtractAssociatedDriveIcon(string path, ushort size) {
      var iconFileKey = RegistryFactory.Drive.OpenDefaultIconKey(path, RegistryHive.CurrentUser) ??
                        RegistryFactory.Drive.OpenDefaultIconKey(path, RegistryHive.LocalMachine) ??
                        RegistryFactory.Drive.OpenDefaultIconKey();
      var iconFilename = iconFileKey.GetValue<string>(default);

      Console.WriteLine(iconFilename);

      try {
        return iconFilename != null
                 ? ExtractIconFromResource(iconFilename, size)
                 : null;
      } catch (FileNotFoundException e) {
        Console.Error.WriteLine(e);
        return null;
      }
    }



    [DllImport("Shell32", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(
      string lpszFile,
      int nIconIndex,
      IntPtr[] phIconLarge,
      IntPtr[] phIconSmall,
      uint nIconSize);



    [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
    private static extern int DestroyIcon(IntPtr hIcon);



    public static Icon ExtractIconFromResource(
      string filename,
      int iconIndex,
      ushort size) {
      IntPtr[] hDummy = {IntPtr.Zero};
      IntPtr[] hIconEx = {IntPtr.Zero};

      try {
        var readIconCount = ExtractIconEx(filename, iconIndex, hIconEx, hDummy, 1);

        return readIconCount > 0 &&
               hIconEx[0] != IntPtr.Zero
                 ? (Icon)Icon.FromHandle(hIconEx[0]).Clone()
                 : null;
      } catch (Exception ex) {
        /* EXTRACT ICON ERROR */

        // BUBBLE UP
        throw new ApplicationException($"Could not extract icon: {filename},{iconIndex}", ex);
      } finally {
        // RELEASE RESOURCES
        foreach (var ptr in hIconEx.Concat(hDummy)) {
          if (ptr != IntPtr.Zero) {
            DestroyIcon(ptr);
          }
        }
      }
    }



    public static Icon ExtractIconFromResource(
      string filename,
      ushort size) {
      var (left, right) = filename.SeparateLast(',');
      var iconIndex = right != null
                        ? int.Parse(right)
                        : 0;
      return ExtractIconFromResource(left, iconIndex, size);
    }



    [DllImport("Shell32", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(
      string lpszFile,
      int nIconIndex,
      IntPtr[] phIconLarge,
      IntPtr[] phIconSmall,
      int nIcons);



    public static Icon ExtractIconFromExe(string file, bool large, int iconIndex = 0) {
      var hIconSmall = new IntPtr[1];
      var hIconLarge = new IntPtr[1];

      try {
        var readIconCount = ExtractIconEx(file, iconIndex, hIconLarge, hIconSmall, 1);
        var hIcon = large ? hIconLarge : hIconSmall;

        return readIconCount > 0 && hIcon[0] != IntPtr.Zero
                 // GET FIRST EXTRACTED ICON
                 ? (Icon)Icon.FromHandle(hIcon[0]).Clone()
                 : null;
      } catch (Exception ex) {
        /* EXTRACT ICON ERROR */

        // BUBBLE UP
        throw new ApplicationException("Could not extract icon", ex);
      } finally {
        // RELEASE RESOURCES
        DestroyIcons(hIconSmall);
        DestroyIcons(hIconLarge);
      }
    }



    private static void DestroyIcons(IntPtr[] iconPtr) {
      foreach (var ptr in iconPtr) {
        if (ptr != IntPtr.Zero) {
          DestroyIcon(ptr);
        }
      }
    }
  }
}
