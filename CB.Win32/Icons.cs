using CB.System;
using CB.Win32.Native;
using CB.Win32.Native.Structures;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;



namespace CB.Win32 {
  public static class Icons {
    private static int GetIconIndex(string filename, bool largeIcon) {
      var sfi = new ShFileInfo();
      var sizeFlag = largeIcon
                       ? SHGetFileInfoConstants.LargeIcon
                       : SHGetFileInfoConstants.SmallIcon;
      Shell32.SHGetFileInfo(
        filename,
        0,
        ref sfi,
        (uint)Marshal.SizeOf(sfi),
        (uint)(SHGetFileInfoConstants.SysIconIndex |
               sizeFlag |
               SHGetFileInfoConstants.UseFileAttributes)
      );
      return sfi.iIcon;
    }



    private static IntPtr GetIconHandle(int iImage, SysImageListSize size) {
      IImageList spiml = null;
      var guil = Shell32.IidImageList2; //or IID_IImageList
      Shell32.SHGetImageList(size, ref guil, ref spiml);
      var hIcon = IntPtr.Zero;
      spiml.GetIcon(
        iImage,
        ImageListDrawItemConstants.TRANSPARENT | ImageListDrawItemConstants.IMAGE,
        ref hIcon
      ); //

      return hIcon;
    }



    public static IntPtr ExtractIconHandle(string filename, IconSize size) {
      var iconIndex = GetIconIndex(filename, size != IconSize.Small);
      return GetIconHandle(iconIndex, (SysImageListSize)size);
    }



    public static Icon ExtractIcon(string filename, IconSize size) {
      var hIcon = ExtractIconHandle(filename, size);
      return Icon.FromHandle(hIcon);

      //TODO ? User32.DestroyIcon(hIcon); // don't forget to cleanup
    }



    // <summary>
    /// Return a file icon of the specified file.
    /// </summary>
    [Obsolete]
    public static Icon ExtractIcon(string fileName, bool largeIcon)
      => ExtractIcon(fileName, largeIcon ? IconSize.Large : IconSize.Small);



    public static IntPtr ExtractIconHandle(string filename, int iconIndex, int width, int height) {
      User32.PrivateExtractIcons(filename, iconIndex, width, height, out var iconHandle, out _, 1, IntPtr.Zero);
      return iconHandle;
    }



    public static IntPtr ExtractIconHandle(string filename, int iconIndex, int size)
      => ExtractIconHandle(filename, iconIndex, size, size);



    public static IntPtr ExtractIconHandle(string filename, int iconIndex, IconSize size) {
      var numSize = size.GetDefaultSize();
      return ExtractIconHandle(filename, iconIndex, numSize.Width, numSize.Height);
    }



    public static Icon? ExtractIcon(string filename, int iconIndex, int width, int height) {
      var iconHandle = ExtractIconHandle(filename, iconIndex, width, height);
      return iconHandle != IntPtr.Zero ? Icon.FromHandle(iconHandle) : null;
      //TODO ? User32.DestroyIcon(hIcon); // don't forget to cleanup
    }



    public static Icon? ExtractIcon(string filename, int iconIndex, int size)
      => ExtractIcon(filename, iconIndex, size, size);



    public static Icon? ExtractIcon(string filename, int iconIndex, IconSize size) {
      var numSize = size.GetDefaultSize();
      return ExtractIcon(filename, iconIndex, numSize.Width, numSize.Height);
    }



    [Obsolete("Use ExtractIcon()")]
    public static Icon? ExtractAssociatedIcon(string fileName, ushort size) {
      if (File.Exists(fileName)) {
        return ExtractIconFromResource(fileName, size);
      }

      var dirInfo = new DirectoryInfo(fileName.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
      if (dirInfo.Parent == null) {
        return ExtractIcon(fileName, IconSizes.CeilingToIconSize(size));
      }

      throw new NotImplementedException("Reading associated icon in not yet supported for " + fileName);
    }



    [Obsolete("Use ExtractIcon instead")]
    public static Icon? ExtractIconFromResource(string fileName, int iconIndex, bool largeIcon)
      => ExtractIcon(fileName, iconIndex, largeIcon ? IconSize.Large : IconSize.Small);



    [Obsolete("Use ExtractIcon instead")]
    public static Icon? ExtractIconFromResource(string fileName, int iconIndex, ushort size)
      => ExtractIcon(fileName, iconIndex, size);



    [Obsolete("Use ExtractIcon() instead, and separate index with comma separator yourself")]
    public static Icon? ExtractIconFromResource(
      string filename,
      ushort size) {
      var (left, right) = filename.SeparateLast(',');
      var iconIndex = right != null ? int.Parse(right) : 0;
      return ExtractIcon(left, iconIndex, size);
    }



    [Obsolete("Use ExtractIcon instead")]
    public static Icon? ExtractIconFromExe(string file, bool large, int iconIndex = 0)
      => ExtractIcon(file, iconIndex, large ? IconSize.Large : IconSize.Small);
  }
}
