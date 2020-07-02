using System;
using System.Runtime.InteropServices;



namespace CB.Win32.Native {
  public static class Msvcrt {
    [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);
  }
}
