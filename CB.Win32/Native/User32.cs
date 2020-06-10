using System;
using System.Runtime.InteropServices;



namespace CB.Win32.Native {
  public static class User32 {
    [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
    public static extern int DestroyIcon(IntPtr hIcon);
  }
}
