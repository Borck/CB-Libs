using System;
using System.Runtime.InteropServices;



namespace CB.Win32.Native {
  public static class Kernel32 {
    [DllImport("kernel32")]
    public static extern int FormatMessage(
      int dwFlags,
      IntPtr lpSource,
      int dwMessageId,
      int dwLanguageId,
      string lpBuffer,
      uint nSize,
      int argumentsLong);



    [DllImport("kernel32")]
    public static extern int GetLastError();
  }
}
