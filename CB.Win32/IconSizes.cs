namespace CB.Win32;

using global::System;
using global::System.Drawing;

public static class IconSizes {
  public static Size GetDefaultSize(this IconSize size) {
    return size switch {
      IconSize.Jumbo => new Size(256, 256),
      IconSize.Extralarge => new Size(48, 48),
      IconSize.Large => new Size(32, 32),
      IconSize.Small => new Size(16, 16),
      IconSize.Syssmall => throw new NotImplementedException(
                size + " has not a static size, and must be read from system metrics"
              ),
      _ => throw new ArgumentException(size.ToString(), nameof(size)),
    };
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
