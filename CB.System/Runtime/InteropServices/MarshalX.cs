using System;
using System.Runtime.InteropServices;



namespace CB.System.Runtime.InteropServices {
  public class MarshalX {
    public static IntPtr Copy(IntPtr dest, IntPtr src, int count) {
      return UnsafeNativeMethods.memcpy( dest, src, count );
    }



    public static void Copy<T>(T[,] dest, T[] src, int count) {
      UnsafeNativeMethods.memcpy(
        GCHandle.Alloc( dest, GCHandleType.Pinned ).AddrOfPinnedObject(),
        GCHandle.Alloc( src, GCHandleType.Pinned ).AddrOfPinnedObject(),
        count * Marshal.SizeOf( typeof(T) )
      );
    }



    public static void Copy<T>(T[] dest, T[,] src, int count) {
      UnsafeNativeMethods.memcpy(
        GCHandle.Alloc( dest, GCHandleType.Pinned ).AddrOfPinnedObject(),
        GCHandle.Alloc( src, GCHandleType.Pinned ).AddrOfPinnedObject(),
        count * Marshal.SizeOf( typeof(T) )
      );
    }



    private static class UnsafeNativeMethods {
      [DllImport(
        "msvcrt.dll",
        EntryPoint = "memcpy",
        CallingConvention = CallingConvention.Cdecl,
        SetLastError = false
      )]
      internal static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);
    }
  }
}
