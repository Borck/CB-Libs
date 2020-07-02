using System;



namespace CB.System.Runtime.InteropServices {
  public static class Interop {
    private static readonly Lazy<bool> IsIgnoreCaseLazy = new Lazy<bool>(
      () => {
        switch (Environment.OSVersion.Platform) {
          case PlatformID.MacOSX: // HFS+ (the Mac file-system) is usually configured to be case insensitive
          case PlatformID.Win32NT:
          case PlatformID.Win32S:
          case PlatformID.Win32Windows:
          case PlatformID.WinCE:
          case PlatformID.Xbox:
            return true;
          case PlatformID.Unix:
            return false;
          default:
            throw new NotSupportedException("Operating system not supported: " + Environment.OSVersion);
        }
      }
    );

    /// <summary>
    ///   Determines whether the filesystem is case variant if true, otherwise case sensitive.
    /// </summary>
    public static bool IsIgnoreCase => IsIgnoreCaseLazy.Value;
  }
}
