namespace CB.Win32.Native.Structures;

using global::System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Rect {
  public int left,
             top,
             right,
             bottom;
}
