using System;
using System.Runtime.InteropServices;



namespace CB.System.Collections {
  public static class ArrayX {
    public static T[] Repeat<T>(T element, int size) {
      var array = new T[size];
      unchecked {
        for (var i = 0; i < size; i++)
          array[i] = element;
      }

      return array;
    }



    [DllImport( "kernel32.dll", SetLastError = false )]
    private static extern void CopyMemory(IntPtr destination, IntPtr source, UIntPtr length);



    public static void Copy(IntPtr source, IntPtr destination, int length) {
      CopyMemory( destination, source, (UIntPtr) length );
    }



    public static void Copy<T>(IntPtr source, T[] destination, int startIndex, int length)
      where T : struct {
      var gch = GCHandle.Alloc( destination, GCHandleType.Pinned );
      try {
        var dstPtr = Marshal.UnsafeAddrOfPinnedArrayElement( destination, startIndex );
        var bytesToCopy = Marshal.SizeOf( typeof(T) ) * length;

        CopyMemory( dstPtr, source, (UIntPtr) bytesToCopy );
      } finally {
        gch.Free();
      }
    }



    public static void Copy<T>(T[] source, IntPtr destination, int startIndex, int length)
      where T : struct {
      var gch = GCHandle.Alloc( destination, GCHandleType.Pinned );
      try {
        var srcPtr = Marshal.UnsafeAddrOfPinnedArrayElement( source, startIndex );
        var bytesToCopy = Marshal.SizeOf( typeof(T) ) * length;

        CopyMemory( destination, srcPtr, (UIntPtr) bytesToCopy );
      } finally {
        gch.Free();
      }
    }



    /// <summary>
    ///   A specialized version of LINQs method ToArray() for array's, but with better performance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T[] Copy<T>(this T[] array) {
      var result = new T[array.Length];
      Array.Copy( array, result, array.Length );
      return result;
    }



    public static T[] Prepand<T>(this T[] array, T headElement) {
      var result = new T[array.Length + 1];
      result[0] = headElement;
      Array.Copy( array, 0, result, 1, array.Length );
      return result;
    }
  }
}
