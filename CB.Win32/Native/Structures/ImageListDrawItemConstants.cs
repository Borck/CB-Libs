using System;



namespace CB.Win32.Native.Structures {
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
}
