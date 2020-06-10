using System;
using System.Runtime.InteropServices;



namespace CB.Win32.Native {
  public static class Shell32 {
    private const string IID_IImageList = "46EB5926-582E-4017-9FDF-E8998DAA0950";
    private const string IID_IImageList2 = "192B9D83-50FC-457B-90A0-2B82A8B5DAE1";

    public static readonly Guid IidImageList = new Guid(IID_IImageList);
    public static readonly Guid IidImageList2 = new Guid(IID_IImageList2);


    #region Public Enumerations

    /// <summary>
    ///   Available system image list sizes (also known as SHIL)
    /// </summary>
    public enum SysImageListSize {
      /// <summary>
      ///   System Large Icon Size (typically 32x32)
      /// </summary>
      LARGE = 0x0,

      /// <summary>
      ///   System Small Icon Size (typically 16x16)
      /// </summary>
      SMALL = 0x1,

      /// <summary>
      ///   System Extra Large Icon Size (typically 48x48).
      ///   Only available under XP; under other OS the
      ///   Large Icon ImageList is returned.
      /// </summary>
      EXTRALARGE = 0x2,
      SYSSMALL = 0x3,
      JUMBO = 0x4,
      LAST = 0x4
    }



    /// <summary>
    ///   Flags controlling how the Image List item is (also known as IDL)
    ///   drawn
    /// </summary>
    [Flags]
    public enum ImageListDrawItemConstants {
      /// <summary>
      ///   Draw item normally.
      /// </summary>
      NORMAL = 0x0,

      /// <summary>
      ///   Draw item transparently.
      /// </summary>
      TRANSPARENT = 0x00000001,

      /// <summary>
      ///   Draw item blended with 25% of the specified foreground colour
      ///   or the Highlight colour if no foreground colour specified.
      /// </summary>
      BLEND25 = 0x2,

      /// <summary>
      ///   Draw item blended with 50% of the specified foreground colour
      ///   or the Highlight colour if no foreground colour specified.
      /// </summary>
      SELECTED = 0x4,

      /// <summary>
      ///   Draw the icon's mask
      /// </summary>
      MASK = 0x10,

      /// <summary>
      ///   Draw the icon image without using the mask
      /// </summary>
      IMAGE = 0x00000020,

      /// <summary>
      ///   Draw the icon using the ROP specified.
      /// </summary>
      ROP = 0x40,

      /// <summary>
      ///   Preserves the alpha channel in dest. XP only.
      /// </summary>
      PRESERVEALPHA = 0x1000,

      /// <summary>
      ///   Scale the image to cx, cy instead of clipping it.  XP only.
      /// </summary>
      SCALE = 0x2000,

      /// <summary>
      ///   Scale the image to the current DPI of the display. XP only.
      /// </summary>
      DPISCALE = 0x4000
    }



    /// <summary>
    ///   Enumeration containing XP ImageList Draw State options
    /// </summary>
    [Flags]
    public enum ImageListDrawStateConstants : int {
      /// <summary>
      ///   The image state is not modified.
      /// </summary>
      ILS_NORMAL = (0x00000000),

      /// <summary>
      ///   Adds a glow effect to the icon, which causes the icon to appear to glow
      ///   with a given color around the edges. (Note: does not appear to be
      ///   implemented)
      /// </summary>
      ILS_GLOW = (0x00000001), //The color for the glow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 

      /// <summary>
      ///   Adds a drop shadow effect to the icon. (Note: does not appear to be
      ///   implemented)
      /// </summary>
      ILS_SHADOW =
        (0x00000002), //The color for the drop shadow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 

      /// <summary>
      ///   Saturates the icon by increasing each color component
      ///   of the RGB triplet for each pixel in the icon. (Note: only ever appears
      ///   to result in a completely unsaturated icon)
      /// </summary>
      ILS_SATURATE =
        (0x00000004), // The amount to increase is indicated by the frame member in the IMAGELISTDRAWPARAMS method. 

      /// <summary>
      ///   Alpha blends the icon. Alpha blending controls the transparency
      ///   level of an icon, according to the value of its alpha channel.
      ///   (Note: does not appear to be implemented).
      /// </summary>
      ILS_ALPHA = (0x00000008) //The value of the alpha channel is indicated by the frame member in the IMAGELISTDRAWPARAMS method. The alpha channel can be from 0 to 255, with 0 being completely transparent, and 255 being completely opaque. 
    }



    /// <summary>
    ///   Flags specifying the state of the icon to draw from the Shell
    /// </summary>
    [Flags]
    public enum ShellIconStateConstants {
      /// <summary>
      ///   Get icon in normal state
      /// </summary>
      ShellIconStateNormal = 0,

      /// <summary>
      ///   Put a link overlay on icon
      /// </summary>
      ShellIconStateLinkOverlay = 0x8000,

      /// <summary>
      ///   show icon in selected state
      /// </summary>
      ShellIconStateSelected = 0x10000,

      /// <summary>
      ///   get open icon
      /// </summary>
      ShellIconStateOpen = 0x2,

      /// <summary>
      ///   apply the appropriate overlays
      /// </summary>
      ShellIconAddOverlays = 0x000000020,
    }

    #endregion



    /// <summary>
    ///   https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shgetimagelist
    /// </summary>
    /// <param name="iImageList"></param>
    /// <param name="riid">Reference to the image list interface identifier, normally IID_IImageList.</param>
    /// <param name="ppv"></param>
    /// <returns></returns>
    [DllImport("shell32.dll", EntryPoint = "#727")]
    public static extern int SHGetImageList(SysImageListSize iImageList, ref Guid riid, ref IImageList ppv);



    [DllImport("shell32.dll")]
    public static extern uint SHGetIDListFromObject([MarshalAs(UnmanagedType.IUnknown)] object iUnknown,
                                                    out IntPtr ppidl);



    [DllImport("Shell32.dll")]
    public static extern IntPtr SHGetFileInfo(
      string pszPath,
      uint dwFileAttributes,
      ref SHFILEINFO psfi,
      uint cbFileInfo,
      uint uFlags
    );



    /// <summary>
    ///   Constants for SHGetFileInfo method (also known as SHGFI)
    /// </summary>
    [Flags]
    public enum SHGetFileInfoConstants : uint {
      /// <summary>get icon</summary>
      Icon = 0x000000100,

      /// <summary>get display name</summary>
      DisplayName = 0x000000200,

      /// <summary>get type name</summary>
      TypeName = 0x000000400,

      /// <summary>get attributes</summary>
      Attributes = 0x000000800,

      /// <summary>get icon location</summary>
      IconLocation = 0x000001000,

      /// <summary>return exe type</summary>
      ExeType = 0x000002000,

      /// <summary>get system icon index</summary>
      SysIconIndex = 0x000004000,

      /// <summary>put a link overlay on icon</summary>
      LinkOverlay = 0x000008000,

      /// <summary>show icon in selected state</summary>
      Selected = 0x000010000,

      /// <summary>get only specified attributes</summary>
      Attr_Specified = 0x000020000,

      /// <summary>get large icon</summary>
      LargeIcon = 0x000000000,

      /// <summary>get small icon</summary>
      SmallIcon = 0x000000001,

      /// <summary>get open icon</summary>
      OpenIcon = 0x000000002,

      /// <summary>get shell size icon</summary>
      ShellIconSize = 0x000000004,

      /// <summary>pszPath is a pidl</summary>
      PIDL = 0x000000008,

      /// <summary>use passed dwFileAttribute</summary>
      UseFileAttributes = 0x000000010,

      /// <summary>apply the appropriate overlays</summary>
      AddOverlays = 0x000000020,

      /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
      OverlayIndex = 0x000000040
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO {
      public const int NAMESIZE = 80;
      public IntPtr hIcon;
      public int iIcon;
      public uint dwAttributes;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string szDisplayName;

      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
      public string szTypeName;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
      public int left,
                 top,
                 right,
                 bottom;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
      private readonly int x;
      private readonly int y;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGELISTDRAWPARAMS {
      public int cbSize;
      public IntPtr himl;
      public int i;
      public IntPtr hdcDst;
      public int x;
      public int y;
      public int cx;
      public int cy;
      public int xBitmap; // x offest from the upperleft of bitmap
      public int yBitmap; // y offset from the upperleft of bitmap
      public int rgbBk;
      public int rgbFg;
      public int fStyle;
      public int dwRop;
      public int fState;
      public int Frame;
      public int crEffect;
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGEINFO {
      public IntPtr hbmImage;
      public IntPtr hbmMask;
      public int Unused1;
      public int Unused2;
      public RECT rcImage;
    }



    [ComImport]
    [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IImageList {
      [PreserveSig]
      int Add(
        IntPtr hbmImage,
        IntPtr hbmMask,
        ref int pi);



      [PreserveSig]
      int ReplaceIcon(
        int i,
        IntPtr hicon,
        ref int pi);



      [PreserveSig]
      int SetOverlayImage(
        int iImage,
        int iOverlay);



      [PreserveSig]
      int Replace(
        int i,
        IntPtr hbmImage,
        IntPtr hbmMask);



      [PreserveSig]
      int AddMasked(
        IntPtr hbmImage,
        int crMask,
        ref int pi);



      [PreserveSig]
      int Draw(
        ref IMAGELISTDRAWPARAMS pimldp);



      [PreserveSig]
      int Remove(
        int i);



      [PreserveSig]
      int GetIcon(
        int i,
        ImageListDrawItemConstants flags,
        ref IntPtr picon);



      [PreserveSig]
      int GetImageInfo(
        int i,
        ref IMAGEINFO pImageInfo);



      [PreserveSig]
      int Copy(
        int iDst,
        IImageList punkSrc,
        int iSrc,
        int uFlags);



      [PreserveSig]
      int Merge(
        int i1,
        IImageList punk2,
        int i2,
        int dx,
        int dy,
        ref Guid riid,
        ref IntPtr ppv);



      [PreserveSig]
      int Clone(
        ref Guid riid,
        ref IntPtr ppv);



      [PreserveSig]
      int GetImageRect(
        int i,
        ref RECT prc);



      [PreserveSig]
      int GetIconSize(
        ref int cx,
        ref int cy);



      [PreserveSig]
      int SetIconSize(
        int cx,
        int cy);



      [PreserveSig]
      int GetImageCount(
        ref int pi);



      [PreserveSig]
      int SetImageCount(
        int uNewCount);



      [PreserveSig]
      int SetBkColor(
        int clrBk,
        ref int pclr);



      [PreserveSig]
      int GetBkColor(
        ref int pclr);



      [PreserveSig]
      int BeginDrag(
        int iTrack,
        int dxHotspot,
        int dyHotspot);



      [PreserveSig]
      int EndDrag();



      [PreserveSig]
      int DragEnter(
        IntPtr hwndLock,
        int x,
        int y);



      [PreserveSig]
      int DragLeave(
        IntPtr hwndLock);



      [PreserveSig]
      int DragMove(
        int x,
        int y);



      [PreserveSig]
      int SetDragCursorImage(
        ref IImageList punk,
        int iDrag,
        int dxHotspot,
        int dyHotspot);



      [PreserveSig]
      int DragShowNolock(
        int fShow);



      [PreserveSig]
      int GetDragImage(
        ref POINT ppt,
        ref POINT pptHotspot,
        ref Guid riid,
        ref IntPtr ppv);



      [PreserveSig]
      int GetItemFlags(
        int i,
        ref int dwFlags);



      [PreserveSig]
      int GetOverlayImage(
        int iOverlay,
        ref int piIndex);
    }
  }
}
