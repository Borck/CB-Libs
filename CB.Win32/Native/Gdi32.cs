namespace CB.Win32.Native;

using global::System;
using global::System.Runtime.InteropServices;

public static class Gdi32 {
  [DllImport("gdi32.dll")]
  public static extern bool DeleteObject(IntPtr hObject);
}
