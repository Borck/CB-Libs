namespace CB.Win32.Native;

using global::System;
using global::System.Runtime.InteropServices;

public static class Msvcrt {
  [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
  public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);
}
