namespace CB.System.Diagnostics;

using global::System;
using global::System.Runtime.InteropServices;



/// <summary>The two kernel32 calls the machine-wide readings need on Windows.</summary>
internal static partial class NativeSystem {
  [StructLayout(LayoutKind.Sequential)]
  internal struct MemoryStatusEx {
    public uint Length;
    public uint MemoryLoad;
    public ulong TotalPhys;
    public ulong AvailPhys;
    public ulong TotalPageFile;
    public ulong AvailPageFile;
    public ulong TotalVirtual;
    public ulong AvailVirtual;
    public ulong AvailExtendedVirtual;
  }



#if NET7_0_OR_GREATER
  [LibraryImport("kernel32.dll", EntryPoint = "GlobalMemoryStatusEx")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);



  [LibraryImport("kernel32.dll", EntryPoint = "GetSystemTimes")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool GetSystemTimes(out ulong idle, out ulong kernel, out ulong user);
#else
  [DllImport("kernel32.dll", EntryPoint = "GlobalMemoryStatusEx")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);



  [DllImport("kernel32.dll", EntryPoint = "GetSystemTimes")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static extern bool GetSystemTimes(out ulong idle, out ulong kernel, out ulong user);
#endif
}
