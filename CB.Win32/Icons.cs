using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CB.System;
using CB.Win32.Native;
using CB.Win32.Registry;
using Microsoft.Win32;



namespace CB.Win32 {
  public static class Icons {
    internal const uint SHGFI_ICON = 0x100;
    internal const uint SHGFI_SMALLICON = 0x1; // 'Small icon
    internal const uint SHGFI_SYSICONINDEX = 16384;

    internal const uint SHGFI_USEFILEATTRIBUTES = 16;



    private static int GetIconIndex(string filename) {
      var sfi = new Shell32.SHFILEINFO();
      Shell32.SHGetFileInfo(
        filename,
        0,
        ref sfi,
        (uint)Marshal.SizeOf(sfi),
        (uint)(Shell32.SHGetFileInfoConstants.SysIconIndex |
               Shell32.SHGetFileInfoConstants.LargeIcon |
               Shell32.SHGetFileInfoConstants.UseFileAttributes)
      );
      return sfi.iIcon;
    }



    private static IntPtr GetIconHandle(int iImage, Shell32.SysImageListSize size) {
      Shell32.IImageList spiml = null;
      var guil = Shell32.IidImageList2; //or IID_IImageList
      Shell32.SHGetImageList(size, ref guil, ref spiml);
      var hIcon = IntPtr.Zero;
      spiml.GetIcon(
        iImage,
        Shell32.ImageListDrawItemConstants.TRANSPARENT | Shell32.ImageListDrawItemConstants.IMAGE,
        ref hIcon
      ); //

      return hIcon;
    }



    public static IntPtr ExtractIconHandle(string filename, IconSize size) {
      var iconIndex = GetIconIndex(filename);
      return GetIconHandle(iconIndex, (Shell32.SysImageListSize)size);
    }



    public static Icon ExtractIcon(string filename, IconSize size) {
      var hIcon = ExtractIconHandle(filename, size);
      return Icon.FromHandle(hIcon);

      //TODO ? Shell32.DestroyIcon(hIcon); // don't forget to cleanup
    }



    // <summary>
    /// Return a file icon of the specified file.
    /// </summary>
    [Obsolete]
    public static Icon ExtractIcon(string fileName, bool largeIcon)
      => ExtractIcon(fileName, largeIcon ? IconSize.Large : IconSize.Small);



    [Obsolete("Use ExtractIcon()")]
    public static Icon ExtractAssociatedIcon(string fileName, ushort size) {
      if (File.Exists(fileName)) {
        return ExtractIconFromResource(fileName, size);
      }

      var dirInfo = new DirectoryInfo(fileName.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
      if (dirInfo.Parent == null) {
        return ExtractAssociatedDriveIcon(fileName, size);
      }

      throw new NotImplementedException("Reading associated icon in not yet supported for " + fileName);
    }



    public static Icon ExtractIconFromResource(string fileName, int iconIndex, bool largeIcon) {
      ExtractIconExW(fileName, iconIndex, out var large, out var small, 1);
      var hIcon = largeIcon ? large : small;
      DestroyIcon(largeIcon ? small : large);
      return Icon.FromHandle(hIcon);
    }



    private static Icon ExtractIconFromResourceWithoutIndex(string fileName, bool largeIcon) {
      var flags = SHGFI_SYSICONINDEX;
      if (fileName.IndexOf(":", StringComparison.InvariantCulture) == -1) {
        flags |= SHGFI_USEFILEATTRIBUTES;
      }

      flags |= largeIcon
                 ? SHGFI_ICON
                 : SHGFI_ICON | SHGFI_SMALLICON;

      var shInfo = new Shfileinfo();
      SHGetFileInfo(fileName, 0, ref shInfo, (uint)Marshal.SizeOf(shInfo), flags);
      var hIcon = shInfo.hIcon;

      return Icon.FromHandle(hIcon);
    }



    //TODO find a way to get huge icon from resource index
    public static Icon ExtractIconFromResource(
      string fileName,
      int iconIndex,
      ushort size) {
      IntPtr[] hDummy = {IntPtr.Zero};
      IntPtr[] hIconEx = {IntPtr.Zero};

      try {
        var readIconCount = ExtractIconEx(fileName, iconIndex, hIconEx, hDummy, 1);

        return readIconCount > 0 &&
               hIconEx[0] != IntPtr.Zero
                 ? (Icon)Icon.FromHandle(hIconEx[0]).Clone()
                 : null;
      } catch (Exception ex) {
        /* EXTRACT ICON ERROR */

        // BUBBLE UP
        throw new ApplicationException($"Could not extract icon: {fileName},{iconIndex}", ex);
      } finally {
        // RELEASE RESOURCES
        foreach (var ptr in hIconEx.Concat(hDummy)) {
          if (ptr != IntPtr.Zero) {
            DestroyIcon(ptr);
          }
        }
      }
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



    [DllImport(
      "Shell32",
      CharSet = CharSet.Unicode,
      ExactSpelling = true,
      CallingConvention = CallingConvention.StdCall
    )]
    private static extern int ExtractIconExW(string sFile,
                                             int iIndex,
                                             out IntPtr piLargeVersion,
                                             out IntPtr piSmallVersion,
                                             int amountIcons);



    [StructLayout(LayoutKind.Sequential)]
    internal struct Shfileinfo {
      public IntPtr hIcon;
      public IntPtr iIcon;
      public uint dwAttributes;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string szDisplayName;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
      public string szTypeName;
    }



    // <summary>
    /// Get Icons that are associated with files.
    /// To use it, use (System.Drawing.Icon myIcon = System.Drawing.Icon.FromHandle(shInfo.hIcon));
    /// hImgSmall = SHGetFileInfo(fName, 0, ref shInfo,(uint)Marshal.SizeOf(shInfo),Win32.SHGFI_ICON |Win32.SHGFI_SMALLICON);
    /// </summary>
    [DllImport("shell32.dll")]
    private static extern IntPtr SHGetFileInfo(string pszPath,
                                               uint dwFileAttributes,
                                               ref Shfileinfo psfi,
                                               uint cbSizeFileInfo,
                                               uint uFlags);



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



    [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
    private static extern int DestroyIcon(IntPtr hIcon);



    private static void DestroyIcons(params IntPtr[] iconPtr) =>
      iconPtr.Where(ptr => ptr != IntPtr.Zero).Select(DestroyIcon);
  }
}
