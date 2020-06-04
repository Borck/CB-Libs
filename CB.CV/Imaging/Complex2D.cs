using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Accord;
using Accord.Math;
using JetBrains.Annotations;



namespace CB.CV.Imaging {
  public class Complex2D {
    #region fields/constants

    // threshold to approximate the divide by zero problem
    //TODO: make editable by overloading respective methods with new parameter threshold

    /// <summary>
    ///   Minimum absolute of a complex number.
    /// </summary>

    public Complex[] Data { get; }

    public int Width { get; }

    public int Height { get; }

    public int Length => Width * Height;

    public double[] Magnitudes => Data.Magnitude();
    public double[] Phases => Data.Phase();
    public double[] Re => Data.Re();
    public double[] Im => Data.Im();
    public double MaxMagnitude => Data.Select(t => t.Magnitude).Concat(new[] {0.0}).Max();
    public double MinMagnitude => Data.Select(t => t.Magnitude).Concat(new[] {0.0}).Min();
    public double MaxRe => Data.Select(t => t.Real).Concat(new[] {0.0}).Max();
    public double MinRe => Data.Select(t => t.Real).Concat(new[] {0.0}).Min();
    public double MaxIm => Data.Select(t => t.Imaginary).Concat(new[] {0.0}).Max();
    public double MinIm => Data.Select(t => t.Imaginary).Concat(new[] {0.0}).Min();
    public double AvgMagnitude => Data.AvgAbs();
    public double SumMagnitude => Data.SumAbs();

    public const int MAX_MAGNITUDE = byte.MaxValue;

    #endregion

    #region constructors

    public Complex2D(int width, int height) {
      Data = new Complex[(Width = width) * (Height = height)];
    }



    public Complex2D(int width, int height, Func<int, int, Complex> generate)
      : this(width, height) {
      for (int j = 0,
               k = 0;
           j < height;
           j++) {
        for (var i = 0; i < width; i++, k++) {
          Data[k] = generate(i, j);
        }
      }
    }



    public Complex2D(int width, int height, Func<Complex> generate)
      : this(width, height) {
      for (var i = 0; i < Length; i++) {
        Data[i] = generate();
      }
    }



    public Complex2D(Complex2D channel)
      : this(channel.Width, channel.Height) {
      Array.Copy(channel.Data, Data, Length);
    }

    #endregion

    #region operators

    public static Complex2D operator +(Complex2D a, double b) {
      var result = new Complex2D(a.Width, a.Height);
      var d = a.Data;
      var e = result.Data;
      for (var i = 0; i < d.Length; i++) {
        e[i] = d[i] + b;
      }

      return result;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckSameSize(Complex2D a, Complex2D b, params Complex2D[] others) {
      if (a.Width != b.Width ||
          a.Height != b.Height) {
        throw new DimensionMismatchException();
      }

      if (others.Length == 0) {
        return;
      }

      var i = 0;
      do {
        if (!a.SameSizeAs(others[i++])) {
          throw new DimensionMismatchException();
        }
      } while (i < others.Length);
    }



    public bool SameSizeAs(Complex2D channel) {
      return Width == channel.Width && Height == channel.Height;
    }



    /// <summary>
    ///   Elementwise addition
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Complex2D operator +(Complex2D a, Complex2D b) {
      CheckSameSize(a, b);
      var result = new Complex2D(a.Width, a.Height);
      var d = a.Data;
      var e = b.Data;
      var f = result.Data;
      for (var i = 0; i < d.Length; i++) {
        f[i] = d[i] + e[i];
      }

      return result;
    }



    public static Complex2D operator -(Complex2D a, double b) {
      var result = new Complex2D(a.Width, a.Height);
      var d = a.Data;
      var e = result.Data;
      for (var i = 0; i < d.Length; i++) {
        e[i] = d[i] - b;
      }

      return result;
    }



    /// <summary>
    ///   Elementwise subtract
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Complex2D operator -(Complex2D a, Complex2D b) {
      CheckSameSize(a, b);
      var result = new Complex2D(a.Width, a.Height);
      var d = a.Data;
      var e = b.Data;
      var f = result.Data;
      for (var i = 0; i < d.Length; i++) {
        f[i] = d[i] - e[i];
      }

      return result;
    }



    /// <summary>
    ///   Elementwise multiply
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Complex2D operator *(Complex2D a, Complex2D b) {
      var result = new Complex2D(a.Width, a.Height);
      DoMultiply(a, b, result);
      return result;
    }



    /// <summary>
    ///   Elementwise multiplitkation
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Complex2D operator *(Complex2D a, double b) {
      var result = new Complex2D(a.Width, a.Height);
      DoMultiply(a, b, result);
      return result;
    }



    /// <summary>
    ///   Elementwise multiplication
    /// </summary>
    /// <param name="factor"></param>
    public void MultiplyWith(Complex2D factor) {
      if (!SameSizeAs(factor)) {
        throw new DimensionMismatchException(nameof(factor));
      }

      DoMultiply(this, factor, this);
    }



    /// <summary>
    ///   Elementwise multiplication
    /// </summary>
    /// <param name="factor"></param>
    public void MultiplyWith(double factor) {
      DoMultiply(this, factor, this);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DoMultiply(Complex2D factor1, Complex2D factor2, Complex2D result) {
      var d1 = factor1.Data;
      var d2 = factor2.Data;
      var dr = result.Data;
      for (var i = 0; i < d1.Length; i++) {
        dr[i] = Complex.Multiply(d1[i], d2[i]);
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DoMultiply(Complex2D factor1, double factor2, Complex2D result) {
      var d1 = factor1.Data;
      var dr = result.Data;
      for (var i = 0; i < d1.Length; i++) {
        dr[i] = d1[i] * factor2;
      }
    }



    /// <summary>
    ///   Elementwise division
    /// </summary>
    /// <param name="numerator"></param>
    /// <param name="denumerator"></param>
    /// <returns></returns>
    public static Complex2D operator /(Complex2D numerator, double denumerator) {
      var result = new Complex2D(numerator.Width, numerator.Height);
      DoMultiply(numerator, 1 / denumerator, result);
      return result;
    }



    /// <summary>
    ///   Elementwise division
    /// </summary>
    /// <param name="denumerator"></param>
    /// <param name="epsilon">Lowest acceptable quotient </param>
    public Complex2D DivideBy(Complex2D denumerator, double epsilon) {
      CheckSameSize(this, denumerator);
      var result = new Complex2D(Width, Height);
      DoDivide(this, denumerator, result, epsilon);
      return result;
    }



    /// <summary>
    ///   Elementwise division
    /// </summary>
    /// <param name="divisor"></param>
    public void DivideBy(double divisor) {
      DoMultiply(this, 1 / divisor, this);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoDivide(Complex2D numerator, Complex2D denumerator, Complex2D result, double epsilon) {
      var absEpsilonInv = 1 / epsilon;
      var d1 = numerator.Data;
      var d2 = denumerator.Data;
      var r = result.Data;
      for (int i = 0,
               n = denumerator.Length;
           i < n;
           i++) {
        var c2 = d2[i];
        r[i] = c2.Magnitude >= epsilon
                 ? r[i] = Complex.Divide(d1[i], c2)
                 : Complex.Multiply(d1[i], Complex.FromPolarCoordinates(absEpsilonInv, -c2.Phase));
      }
    }



    public static void Divide(Complex2D numerator, Complex2D denumerator, Complex2D result, double epsilon) {
      CheckSameSize(numerator, denumerator, result);
      DoDivide(numerator, denumerator, result, epsilon);
    }



    public static Complex2D Log([NotNull] Complex2D c2D) {
      var result = new Complex2D(c2D.Width, c2D.Height);
      var d = c2D.Data;
      var e = result.Data;
      for (var i = 0; i < d.Length; i++) {
        var c = d[i];
        e[i] = Complex.FromPolarCoordinates(Math.Log(c.Magnitude), c.Phase);
      }

      return result;
    }



    public static Complex2D Log([NotNull] Complex2D c2D, double newBase) {
      var result = new Complex2D(c2D.Width, c2D.Height);
      var d = c2D.Data;
      var e = result.Data;
      for (var i = 0; i < d.Length; i++) {
        var c = d[i];
        e[i] = Complex.FromPolarCoordinates(Math.Log(c.Magnitude, newBase), c.Phase);
      }

      return result;
    }

    #endregion

    #region for operations

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForEachRow(Action<Complex[]> action) {
      ForEachRow(action, this);
    }



    public void ForEachRow(Action<Complex[]> action, [NotNull] Complex2D output) {
      var outData = output.Data;
      var row = new Complex[Width];
      for (var i0Line = 0; i0Line < Length; i0Line += Width) {
        Array.Copy(Data, i0Line, row, 0, Width);
        action(row);
        Array.Copy(row, 0, outData, i0Line, Width);
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForEachColumn(Action<Complex[]> action) {
      ForEachColumn(action, this);
    }



    public void ForEachColumn(Action<Complex[]> action, [NotNull] Complex2D output) {
      var outData = output.Data;
      var column = new Complex[Height];
      for (var i = 0; i < Width; i++) {
        for (int j = 0,
                 k = i;
             j < Height;
             j++, k += Width) {
          column[j] = Data[k];
        }

        action(column);
        for (int j = 0,
                 k = i;
             j < Height;
             j++, k += Width) {
          outData[k] = column[j];
        }
      }
    }



    public void ForEachPixel(Action<Complex> action) {
      foreach (var e in Data) {
        action(e);
      }
    }



    public void ForEachPixel(Action<int, int, Complex> action) {
      for (int j = 0,
               k = 0;
           j < Height;
           j++) {
        for (var i = 0; i < Width; i++, k++) {
          action(i, j, Data[k]);
        }
      }
    }



    public static void ForEachPixel(Complex2D channel1,
                                    Complex2D channel2,
                                    Complex2D result,
                                    Func<Complex, Complex, Complex> func) {
      if (!channel1.SameSizeAs(channel2)) {
        throw new Exception("Error at 'MultiplyWith': Complex2DChannels not in the same solution");
      }

      if (channel1 == null ||
          channel2 == null ||
          result == null) {
        throw new NullReferenceException("one of the channels is null");
      }

      if (!result.SameSizeAs(channel2)) {
        throw new Exception("Error at 'MultiplyWith': Complex2DChannels not in the same solution");
      }

      var n = channel1.Width * channel2.Height;

      var c1Data = channel1.Data;
      var c2Data = channel2.Data;
      var cResultData = result.Data;

      for (var i = 0; i < n; i++) {
        cResultData[i] = func(c1Data[i], c2Data[i]);
      }
    }

    #endregion



    /// <summary>
    ///   Merges this and an other channel.
    /// </summary>
    /// <param name="channel">the other channel</param>
    /// <param name="fold">defines how to merge</param>
    public void FoldWith(Complex2D channel, Action<Complex, Complex, int, int> fold) {
      if (!SameSizeAs(channel)) {
        throw new Exception("Error at 'Merge': Complex2DChannels not in the same solution");
      }

      var inputData = channel.Data;
      for (var i = 0; i < Length; i++) {
        fold(Data[i], inputData[i], i % Width, i / Width);
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CheckComparable([NotNull] Complex2D image1, [NotNull] Complex2D image2) {
      if (image1.Width != image2.Width ||
          image1.Height != image2.Height) {
        throw new DimensionMismatchException();
      }
    }



    public Complex[,] ToMatrix() {
      return Data.ToMatrix();
    }



    public Complex[][] ToJagged() {
      return Data.ToJagged();
    }



    public static Complex2D CreateFromReals(double[][] reals) {
      var h = reals.Length;
      var w = h > 0 ? reals[0].Length : 0;
      var result = new Complex2D(w, h);
      var d = result.Data;
      for (int y = 0,
               i = 0;
           y < h;
           y++) {
        var rl = reals[y];
        for (var x = 0; x < w; x++, i++) {
          d[i] = rl[x];
        }
      }

      return result;
    }



    public Complex[][] ToJagged(bool transposed) {
      return Data.ToJagged(transposed);
    }



    public double[] Raw() {
      var result = new double[Width * Height * 2 + 1];
      result[0] = Width;
      var gch = GCHandle.Alloc(Data, GCHandleType.Pinned);
      try {
        var sPtr = gch.AddrOfPinnedObject();
        Marshal.Copy(sPtr, result, 1, result.Length - 1);
      } finally {
        if (gch.IsAllocated) {
          gch.Free();
        }
      }

      return result;
    }



    internal static Complex2D FromRaw(double[] data) {
      var n = (data.Length - 1) / 2;
      var w = (int)data[0];
      var result = new Complex2D(w, n / w);

      var gch = GCHandle.Alloc(result.Data, GCHandleType.Pinned);
      try {
        var sPtr = gch.AddrOfPinnedObject();
        Marshal.Copy(data, 1, sPtr, data.Length - 1);
      } finally {
        if (gch.IsAllocated) {
          gch.Free();
        }
      }

      return result;
    }



    public Complex2D Clone() {
      return new Complex2D(this);
    }



    public static Complex2D Create([NotNull] Complex[][] data) {
      var result = new Complex2D(data[0].Length, data.Length);
      Array.Copy(data, result.Data, data.Length);
      return result;
    }



    public void Norm(Complex2D cRef) {
      var a = cRef.Data.Sum(c1 => c1.Magnitude) / Data.Sum(c1 => c1.Magnitude);
      if (Math.Abs(a - 1.0) < double.Epsilon) {
        return;
      }

      for (var i = 0; i < Data.Length; i++) {
        Data[i] *= a;
      }
    }
  }
}
