using System;
using System.Drawing;



namespace CB.Win32 {
  public static class IconSizes {
    public static Size GetDefaultSize(this IconSize size) {
      switch (size) {
        case IconSize.Jumbo:
          return new Size(256, 256);
        case IconSize.Extralarge:
          return new Size(48, 48);
        case IconSize.Large:
          return new Size(32, 32);
        case IconSize.Small:
          return new Size(16, 16);
        case IconSize.Syssmall:
          throw new NotImplementedException(
            size + " has not a static size, and must be read from system metrics"
          );
        default:
          throw new ArgumentException(size.ToString(), nameof(size));
      }
    }



    public static readonly IconSize LargestSystemIconSize = Environment.OSVersion.Version.Major >= 6
                                                              ? IconSize.Jumbo
                                                              : IconSize.Extralarge;



    public static IconSize CeilingToIconSize(int size) {
      if (size > 48) {
        return IconSize.Jumbo;
      }

      if (size > 32) {
        return IconSize.Extralarge;
      }

      return size > 16 ? IconSize.Large : IconSize.Small;
    }
  }
}
