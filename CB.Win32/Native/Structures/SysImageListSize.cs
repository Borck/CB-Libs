namespace CB.Win32.Native.Structures {
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
}
