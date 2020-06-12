using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;



namespace CB.Win32.Native.Structures {
  public class SysImageList : IDisposable {
    #region UnmanagedCode

    private const int FILE_ATTRIBUTE_NORMAL = 0x80;
    private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

    private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
    private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
    private const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
    private const int FORMAT_MESSAGE_FROM_STRING = 0x400;
    private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
    private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
    private const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;

    #endregion

    #region Private Enumerations

    #endregion



    #region Member Variables

    private IntPtr hIml = IntPtr.Zero;
    private IImageList iImageList = null;
    private SysImageListSize size = SysImageListSize.SMALL;
    private bool disposed = false;

    #endregion


    #region Properties

    /// <summary>
    ///   Gets the hImageList handle
    /// </summary>
    public IntPtr Handle => hIml;

    /// <summary>
    ///   Gets/sets the size of System Image List to retrieve.
    /// </summary>
    public SysImageListSize ImageListSize {
      get => size;
      set {
        size = value;
        create();
      }
    }

    /// <summary>
    ///   Returns the size of the Image List Icons.
    /// </summary>
    public Size Size {
      get {
        var cx = 0;
        var cy = 0;
        if (iImageList == null) {
          ComCtl32.ImageList_GetIconSize(
            hIml,
            ref cx,
            ref cy
          );
        } else {
          iImageList.GetIconSize(ref cx, ref cy);
        }

        var sz = new Size(
          cx,
          cy
        );
        return sz;
      }
    }

    #endregion

    #region Methods

    /// <summary>
    ///   Returns a GDI+ copy of the icon from the ImageList
    ///   at the specified index.
    /// </summary>
    /// <param name="index">The index to get the icon for</param>
    /// <returns>The specified icon</returns>
    public Icon Icon(int index) {
      Icon icon = null;

      var hIcon = IntPtr.Zero;
      if (iImageList == null) {
        hIcon = ComCtl32.ImageList_GetIcon(
          hIml,
          index,
          ImageListDrawItemConstants.TRANSPARENT
        );
      } else {
        iImageList.GetIcon(
          index,
          ImageListDrawItemConstants.TRANSPARENT,
          ref hIcon
        );
      }

      if (hIcon != IntPtr.Zero) {
        icon = global::System.Drawing.Icon.FromHandle(hIcon);
      }

      return icon;
    }



    /// <summary>
    ///   Return the index of the icon for the specified file, always using
    ///   the cached version where possible.
    /// </summary>
    /// <param name="fileName">Filename to get icon for</param>
    /// <returns>Index of the icon</returns>
    public int IconIndex(string fileName) {
      return IconIndex(fileName, false);
    }



    /// <summary>
    ///   Returns the index of the icon for the specified file
    /// </summary>
    /// <param name="fileName">Filename to get icon for</param>
    /// <param name="forceLoadFromDisk">
    ///   If True, then hit the disk to get the icon,
    ///   otherwise only hit the disk if no cached icon is available.
    /// </param>
    /// <returns>Index of the icon</returns>
    public int IconIndex(
      string fileName,
      bool forceLoadFromDisk) {
      return IconIndex(
        fileName,
        forceLoadFromDisk,
        ShellIconStateConstants.ShellIconStateNormal
      );
    }



    /// <summary>
    ///   Returns the index of the icon for the specified file
    /// </summary>
    /// <param name="fileName">Filename to get icon for</param>
    /// <param name="forceLoadFromDisk">
    ///   If True, then hit the disk to get the icon,
    ///   otherwise only hit the disk if no cached icon is available.
    /// </param>
    /// <param name="iconState">
    ///   Flags specifying the state of the icon
    ///   returned.
    /// </param>
    /// <returns>Index of the icon</returns>
    public int IconIndex(
      string fileName,
      bool forceLoadFromDisk,
      ShellIconStateConstants iconState
    ) {
      var dwFlags = SHGetFileInfoConstants.SysIconIndex;
      uint dwAttr = 0;
      if (size == SysImageListSize.SMALL) {
        dwFlags |= SHGetFileInfoConstants.SmallIcon;
      }

      // We can choose whether to access the disk or not. If you don't
      // hit the disk, you may get the wrong icon if the icon is
      // not cached. Also only works for files.
      if (!forceLoadFromDisk) {
        dwFlags |= SHGetFileInfoConstants.UseFileAttributes;
        dwAttr = FILE_ATTRIBUTE_NORMAL;
      } else {
        dwAttr = 0;
      }

      // sFileSpec can be any file. You can specify a
      // file that does not exist and still get the
      // icon, for example sFileSpec = "C:\PANTS.DOC"
      var shfi = new ShFileInfo();
      var shfiSize = (uint)Marshal.SizeOf(shfi.GetType());
      var retVal = Shell32.SHGetFileInfo(
        fileName,
        dwAttr,
        ref shfi,
        shfiSize,
        ((uint)(dwFlags) | (uint)iconState)
      );

      if (retVal.Equals(IntPtr.Zero)) {
        Debug.Assert((!retVal.Equals(IntPtr.Zero)), "Failed to get icon index");
        return 0;
      } else {
        return shfi.iIcon;
      }
    }



    /// <summary>
    ///   Draws an image
    /// </summary>
    /// <param name="hdc">Device context to draw to</param>
    /// <param name="index">Index of image to draw</param>
    /// <param name="x">X Position to draw at</param>
    /// <param name="y">Y Position to draw at</param>
    public void DrawImage(
      IntPtr hdc,
      int index,
      int x,
      int y
    ) {
      DrawImage(hdc, index, x, y, ImageListDrawItemConstants.TRANSPARENT);
    }



    /// <summary>
    ///   Draws an image using the specified flags
    /// </summary>
    /// <param name="hdc">Device context to draw to</param>
    /// <param name="index">Index of image to draw</param>
    /// <param name="x">X Position to draw at</param>
    /// <param name="y">Y Position to draw at</param>
    /// <param name="flags">Drawing flags</param>
    public void DrawImage(
      IntPtr hdc,
      int index,
      int x,
      int y,
      ImageListDrawItemConstants flags
    ) {
      if (iImageList == null) {
        var ret = ComCtl32.ImageList_Draw(
          hIml,
          index,
          hdc,
          x,
          y,
          (int)flags
        );
      } else {
        var pimldp = new ImageListDrawParams();
        pimldp.hdcDst = hdc;
        pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
        pimldp.i = index;
        pimldp.x = x;
        pimldp.y = y;
        pimldp.rgbFg = -1;
        pimldp.fStyle = (int)flags;
        iImageList.Draw(ref pimldp);
      }
    }



    /// <summary>
    ///   Draws an image using the specified flags and specifies
    ///   the size to clip to (or to stretch to if ILD_SCALE
    ///   is provided).
    /// </summary>
    /// <param name="hdc">Device context to draw to</param>
    /// <param name="index">Index of image to draw</param>
    /// <param name="x">X Position to draw at</param>
    /// <param name="y">Y Position to draw at</param>
    /// <param name="flags">Drawing flags</param>
    /// <param name="cx">Width to draw</param>
    /// <param name="cy">Height to draw</param>
    public void DrawImage(
      IntPtr hdc,
      int index,
      int x,
      int y,
      ImageListDrawItemConstants flags,
      int cx,
      int cy
    ) {
      var pimldp = new ImageListDrawParams {hdcDst = hdc};
      pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
      pimldp.i = index;
      pimldp.x = x;
      pimldp.y = y;
      pimldp.cx = cx;
      pimldp.cy = cy;
      pimldp.fStyle = (int)flags;
      if (iImageList == null) {
        pimldp.himl = hIml;
        var ret = ComCtl32.ImageList_DrawIndirect(ref pimldp);
      } else {
        iImageList.Draw(ref pimldp);
      }
    }



    /// <summary>
    ///   Draws an image using the specified flags and state on XP systems.
    /// </summary>
    /// <param name="hdc">Device context to draw to</param>
    /// <param name="index">Index of image to draw</param>
    /// <param name="x">X Position to draw at</param>
    /// <param name="y">Y Position to draw at</param>
    /// <param name="flags">Drawing flags</param>
    /// <param name="cx">Width to draw</param>
    /// <param name="cy">Height to draw</param>
    /// <param name="foreColor">
    ///   Fore colour to blend with when using the
    ///   ILD_SELECTED or ILD_BLEND25 flags
    /// </param>
    /// <param name="stateFlags">State flags</param>
    /// <param name="glowOrShadowColor">
    ///   If stateFlags include ILS_GLOW, then
    ///   the color to use for the glow effect.  Otherwise if stateFlags includes
    ///   ILS_SHADOW, then the color to use for the shadow.
    /// </param>
    /// <param name="saturateColorOrAlpha">
    ///   If stateFlags includes ILS_ALPHA,
    ///   then the alpha component is applied to the icon. Otherwise if
    ///   ILS_SATURATE is included, then the (R,G,B) components are used
    ///   to saturate the image.
    /// </param>
    public void DrawImage(
      IntPtr hdc,
      int index,
      int x,
      int y,
      ImageListDrawItemConstants flags,
      int cx,
      int cy,
      Color foreColor,
      ImageListDrawStateConstants stateFlags,
      Color saturateColorOrAlpha,
      Color glowOrShadowColor
    ) {
      var pimldp = new ImageListDrawParams {hdcDst = hdc};
      pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
      pimldp.i = index;
      pimldp.x = x;
      pimldp.y = y;
      pimldp.cx = cx;
      pimldp.cy = cy;
      pimldp.rgbFg = Color.FromArgb(
                            0,
                            foreColor.R,
                            foreColor.G,
                            foreColor.B
                          )
                          .ToArgb();
      Console.WriteLine("{0}", pimldp.rgbFg);
      pimldp.fStyle = (int)flags;
      pimldp.fState = (int)stateFlags;
      if ((stateFlags & ImageListDrawStateConstants.ILS_ALPHA) ==
          ImageListDrawStateConstants.ILS_ALPHA) {
        // Set the alpha:
        pimldp.Frame = (int)saturateColorOrAlpha.A;
      } else if ((stateFlags & ImageListDrawStateConstants.ILS_SATURATE) ==
                 ImageListDrawStateConstants.ILS_SATURATE) {
        // discard alpha channel:
        saturateColorOrAlpha = Color.FromArgb(
          0,
          saturateColorOrAlpha.R,
          saturateColorOrAlpha.G,
          saturateColorOrAlpha.B
        );
        // set the saturate color
        pimldp.Frame = saturateColorOrAlpha.ToArgb();
      }

      glowOrShadowColor = Color.FromArgb(
        0,
        glowOrShadowColor.R,
        glowOrShadowColor.G,
        glowOrShadowColor.B
      );
      pimldp.crEffect = glowOrShadowColor.ToArgb();
      if (iImageList == null) {
        pimldp.himl = hIml;
        var ret = ComCtl32.ImageList_DrawIndirect(ref pimldp);
      } else {
        iImageList.Draw(ref pimldp);
      }
    }



    /// <summary>
    ///   Determines if the system is running Windows XP
    ///   or above
    /// </summary>
    /// <returns>True if system is running XP or above, False otherwise</returns>
    private bool isXpOrAbove() {
      var ret = false;
      if (Environment.OSVersion.Version.Major > 5) {
        ret = true;
      } else if ((Environment.OSVersion.Version.Major == 5) &&
                 (Environment.OSVersion.Version.Minor >= 1)) {
        ret = true;
      }

      return ret;
      //return false;
    }



    /// <summary>
    ///   Creates the SystemImageList
    /// </summary>
    private void create() {
      // forget last image list if any:
      hIml = IntPtr.Zero;

      if (isXpOrAbove()) {
        // Get the System IImageList object from the Shell:
        var iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
        var ret = Shell32.SHGetImageList(size, ref iidImageList, ref iImageList);
        // the image list handle is the IUnknown pointer, but 
        // using Marshal.GetIUnknownForObject doesn't return
        // the right value.  It really doesn't hurt to make
        // a second call to get the handle:
        Shell32.SHGetImageListHandle(size, ref iidImageList, ref hIml);
      } else {
        // Prepare flags:
        var dwFlags = SHGetFileInfoConstants.UseFileAttributes | SHGetFileInfoConstants.SysIconIndex;
        if (size == SysImageListSize.SMALL) {
          dwFlags |= SHGetFileInfoConstants.SmallIcon;
        }

        // Get image list
        var shfi = new ShFileInfo();
        var shfiSize = (uint)Marshal.SizeOf(shfi.GetType());

        // Call SHGetFileInfo to get the image list handle
        // using an arbitrary file:
        hIml = Shell32.SHGetFileInfo(
          ".txt",
          FILE_ATTRIBUTE_NORMAL,
          ref shfi,
          shfiSize,
          (uint)dwFlags
        );
        Debug.Assert((hIml != IntPtr.Zero), "Failed to create Image List");
      }
    }

    #endregion

    #region Constructor, Dispose, Destructor

    /// <summary>
    ///   Creates a Small Icons SystemImageList
    /// </summary>
    public SysImageList() {
      create();
    }



    /// <summary>
    ///   Creates a SystemImageList with the specified size
    /// </summary>
    /// <param name="size">Size of System ImageList</param>
    public SysImageList(SysImageListSize size) {
      this.size = size;
      create();
    }



    /// <summary>
    ///   Clears up any resources associated with the SystemImageList
    /// </summary>
    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }



    /// <summary>
    ///   Clears up any resources associated with the SystemImageList
    ///   when disposing is true.
    /// </summary>
    /// <param name="disposing">Whether the object is being disposed</param>
    public virtual void Dispose(bool disposing) {
      if (!disposed) {
        if (disposing) {
          if (iImageList != null) {
            Marshal.ReleaseComObject(iImageList);
          }

          iImageList = null;
        }
      }

      disposed = true;
    }



    /// <summary>
    ///   Finalize for SysImageList
    /// </summary>
    ~SysImageList() {
      Dispose(false);
    }

    #endregion
  }
}
