using System;
using System.Runtime.InteropServices;



namespace CB.Win32.Native.Structures {
  [StructLayout(LayoutKind.Sequential)]
  public struct ImageInfo {
    public IntPtr hbmImage;
    public IntPtr hbmMask;
    public int Unused1;
    public int Unused2;
    public Rect rcImage;
  }
}
