using System;
using System.Runtime.InteropServices;



namespace CB.Win32.Native.Structures {
  /// <summary>
  ///   SHFILEINFO
  ///   https://docs.microsoft.com/de-de/windows/win32/api/shellapi/ns-shellapi-shfileinfoa
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct ShFileInfo {
    public IntPtr hIcon;
    public int iIcon;
    public uint dwAttributes;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szDisplayName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
    public string szTypeName;
  }
}
