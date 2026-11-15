namespace CB.Win32.Native.Structures;

using global::System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Point {
  public readonly int x;
  public readonly int y;
}
