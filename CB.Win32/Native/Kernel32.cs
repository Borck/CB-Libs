using System;
using System.Runtime.InteropServices;
using CB.Win32.Native.Structures;



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



    [DllImport(
      "kernel32.dll",
      SetLastError = true,
      CharSet = CharSet.Auto
    )]
    public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);



    //[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadLibraryW")]
    //internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, LoadLibraryExFlags dwFlags);



    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr hModule);



    [DllImport("kernel32.dll")]
    public static extern ErrorModes SetErrorMode(ErrorModes errorMode);
  }
}
