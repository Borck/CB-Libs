using System.Runtime.InteropServices;



namespace CB.Win32.Native.Structures {
  [StructLayout(LayoutKind.Sequential)]
  public readonly struct Point {
    public readonly int x;
    public readonly int y;
  }
}
