namespace CB.System.Runtime.InteropServices;

using global::System;

public static class Interop {
  private static readonly Lazy<bool> IsIgnoreCaseLazy = new(
    () => Environment.OSVersion.Platform switch {
      // HFS+ (the Mac file-system) is usually configured to be case insensitive
      PlatformID.MacOSX or PlatformID.Win32NT or PlatformID.Win32S or PlatformID.Win32Windows or PlatformID.WinCE or PlatformID.Xbox => true,
      PlatformID.Unix => false,
      _ => throw new NotSupportedException("Operating system not supported: " + Environment.OSVersion),
    });

  /// <summary>
  ///   Determines whether the filesystem is case variant if true, otherwise case sensitive.
  /// </summary>
  public static bool IsIgnoreCase => IsIgnoreCaseLazy.Value;
}
