using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;



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



    public static void CopyTransposed<T>(this T[][] thisArray, T[][] dstArray) {
      for (var y = 0; y < thisArray.Length; y++) {
        var col = thisArray[y];
        for (var x = 0; x < col.Length; x++)
          dstArray[x][y] = col[x];
      }
    }



    public static T[] DeepClone<T>(this T[] data, int index, int length) {
      var arrCopy = new T[length];
      Array.Copy( data, index, arrCopy, 0, length );
      using (var ms = new MemoryStream()) {
        var bf = new BinaryFormatter();
        bf.Serialize( ms, arrCopy );
        ms.Position = 0;
        return (T[]) bf.Deserialize( ms );
      }
    }



    public static T[] Prepand<T>(this T[] array, T headElement) {
      var result = new T[array.Length + 1];
      result[0] = headElement;
      Array.Copy( array, 0, result, 1, array.Length );
      return result;
    }



    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="outArray"></param>
    /// <param name="startRow"></param>
    /// <param name="numberOfRows"></param>
    /// <returns></returns>
    public static void Redim<T>(this T[][] array, T[] outArray, int startRow, int numberOfRows) {
      if (startRow < 0 && numberOfRows < 0 && startRow + numberOfRows > array.Length)
        throw new ArgumentOutOfRangeException();
      DoRedim1D( array, outArray, startRow, startRow + numberOfRows );
    }



    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T[] Redim1D<T>(this T[][] array) {
      // TODO: test it!
      var length = array.Sum( part => part.Length );
      var result = new T[length];

      var pos = 0;
      foreach (var part in array) {
        Array.Copy( part, 0, result, pos, part.Length );
        pos += part.Length;
      }

      return result;
    }



    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="startRow"></param>
    /// <param name="numberOfRows"></param>
    /// <returns></returns>
    public static T[] Redim1D<T>(this T[][] array, int startRow, int numberOfRows) {
      if (startRow < 0 && numberOfRows < 0 && startRow + numberOfRows > array.Length)
        throw new ArgumentOutOfRangeException();
      numberOfRows += startRow;

      var length = 0;
      for (var i = startRow; i < numberOfRows; i++) {
        T[] part;
        if (( part = array[i] ) != null)
          length += part.Length;
      }

      var result = new T[length];
      DoRedim1D( array, result, startRow, numberOfRows );
      return result;
    }



    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="outArray"></param>
    /// <param name="startRow"></param>
    /// <param name="toRow"></param>
    /// <returns></returns>
    private static void DoRedim1D<T>(this T[][] array, T[] outArray, int startRow, int toRow) {
      var pos = 0;
      for (; startRow < toRow; startRow++) {
        T[] part;
        if (( part = array[startRow] ) == null)
          continue;
        Array.Copy( part, 0, outArray, pos, part.Length );
        pos += part.Length;
      }
    }



    public static T[,] Redim2D<T>(this T[] thisArray, int width) {
      var height = thisArray.Length / width;
      if (width * height != thisArray.Length)
        throw new ArgumentOutOfRangeException( nameof(width), @"Length of array is not n times of width." );

      var result = new T[width, height];
      for (int y = 0,
               i = 0;
           y < height;
           y++)
      for (var x = 0; x < width; x++, i++)
        result[x, y] = thisArray[i];
      return result;
    }



    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public static T[][] Redim2DJagged<T>(this T[] array, int width) {
      var h = array.Length / width;
      if (h * width != array.Length)
        throw new ArgumentOutOfRangeException( nameof(width), @"Length of array is not n times of width." );
      var result = new T[h][];
      for (int y = 0,
               i = 0;
           y < h;
           y++, i += width)
        Array.Copy( array, i, result[y] = new T[width], 0, width );
      return result;
    }



    public static T[][] Redim2DJaggedTransposed<T>(this T[] thisArray, int width) {
      var h = thisArray.Length / width;
      if (h * width != thisArray.Length)
        throw new ArgumentOutOfRangeException( nameof(width), @"Length of array is not n times of width." );
      var result = new T[width][];
      for (var y = 0; y < width; y++) {
        var col = result[y];
        for (int j = 0,
                 i = y;
             j < col.Length;
             j++, i += width)
          col[j] = thisArray[i];
      }

      return result;
    }



    public static bool EqualsElementwise<T>(this T[] thisArray, T[] otherArray) {
      var n = thisArray.Length;
      if (n != otherArray.Length)
        return false;
      for (var i = 0; i < n; i++)
        if (!Equals( thisArray[i], otherArray[i] ))
          return false;
      return true;
    }



    public static bool EqualsElementwise(this int[] thisArray, int[] otherArray) {
      var n = thisArray.Length;
      if (n != otherArray.Length)
        return false;
      for (var i = 0; i < n; i++)
        if (thisArray[i] != otherArray[i])
          return false;
      return true;
    }



    public static bool EqualsElementwise(this double[] thisArray, double[] otherArray) {
      var n = thisArray.Length;
      if (n != otherArray.Length)
        return false;
      for (var i = 0; i < n; i++)
        if (thisArray[i] != otherArray[i])
          return false;
      return true;
    }



    public static bool EqualsElementwise(this float[] thisArray, float[] otherArray) {
      var n = thisArray.Length;
      if (n != otherArray.Length)
        return false;
      for (var i = 0; i < n; i++)
        if (thisArray[i] != otherArray[i])
          return false;
      return true;
    }
  }
}
