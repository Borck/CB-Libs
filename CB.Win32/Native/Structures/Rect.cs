using System.Runtime.InteropServices;



namespace CB.Win32.Native.Structures {
  [StructLayout(LayoutKind.Sequential)]
  public struct Rect {
    public int left,
               top,
               right,
               bottom;
  }
}
