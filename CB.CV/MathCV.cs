using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;



namespace CB.CV {
  /// <summary>
  ///   An advance to <see cref="System.Math" /> and is inspired by http://graphics.stanford.edu/~seander/bithacks.html.
  /// </summary>
  public static class MathCV {
    /// <summary>
    ///   The base of the natural logarithms as a constant e.
    /// </summary>
    public const double E = 2.71828182845904523536;

    /// <summary>
    ///   log2(e)
    /// </summary>
    public const double LOG2_E = 1.44269504088896340736;

    /// <summary>
    ///   log10(e)
    /// </summary>
    public const double LOG10_E = 0.434294481903251827651;

    /// <summary>
    ///   ln(2)
    /// </summary>
    public const double LN2 = 0.693147180559945309417;

    /// <summary>
    ///   ln(10)
    /// </summary>
    public const double LN10 = 2.30258509299404568402;

    /// <summary>
    ///   pi
    /// </summary>
    public const double PI = 3.14159265358979323846;

    /// <summary>
    ///   pi/2
    /// </summary>
    public const double PI_2 = 1.57079632679489661923;

    /// <summary>
    ///   pi/4
    /// </summary>
    public const double PI_4 = 0.785398163397448309616;

    /// <summary>
    ///   1/pi
    /// </summary>
    public const double ONE_PI = Math.PI;

    /// <summary>
    ///   2/pi
    /// </summary>
    public const double TWO_PI = ONE_PI * 2;

    /// <summary>
    ///   2/sqrt(pi)
    /// </summary>
    public const double TWO_SQRTPI = 1.12837916709551257390;

    /// <summary>
    ///   sqrt(2)
    /// </summary>
    public const double SQRT2 = 1.41421356237309504880;

    /// <summary>
    ///   1/sqrt(2)
    /// </summary>
    public const double ONE_SQRT2 = 0.707106781186547524401;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(int a, int b, int c) {
      return a > b ? (a > c ? a : c) : (b > c ? b : c);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Max(double a, double b, double c) {
      return a > b ? (a > c ? a : c) : (b > c ? b : c);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(int a, int b, int c, int d) {
      var ab = a > b ? a : b;
      var cd = c > d ? c : d;
      return ab > cd ? ab : cd;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Max(double a, double b, double c, double d) {
      var ab = a > b ? a : b;
      var cd = c > d ? c : d;
      return ab > cd ? ab : cd;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Min(int a, int b, int c) {
      return a < b ? (a < c ? a : c) : (b < c ? b : c);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Min(double a, double b, double c) {
      return a < b ? (a < c ? a : c) : (b < c ? b : c);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Min(int a, int b, int c, int d) {
      var ab = a < b ? a : b;
      var cd = c < d ? c : d;
      return ab < cd ? ab : cd;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Min(double a, double b, double c, double d) {
      var ab = a < b ? a : b;
      var cd = c < d ? c : d;
      return ab < cd ? ab : cd;
    }



    /// <summary>
    ///   Ceils to the next power of 2
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Log2Ceil(double a) {
      return ((int)(BitConverter.DoubleToInt64Bits(a) >> 52) & 0x7ff) - 1022;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int OddFloor(int a) {
      return a - (a & 1 ^ 1);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int EvenFloor(int a) {
      return a & -2;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPowerOf2(int a) {
      return (a & (a - 1)) == 0;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Pow2(double a) {
      return a * a;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Pow2(int a) {
      return a * a;
    }



    /// <summary>
    ///   Round up to the next power of 2
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong UpperPow2(ulong a) {
      a--;
      a |= a >> 1;
      a |= a >> 2;
      a |= a >> 4;
      a |= a >> 8;
      a |= a >> 16;
      return a + 1;
    }



    /// <summary>
    ///   Round up to the next power of 2
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint UpperPow2(double a) {
      return UpperPow2((uint)Math.Ceiling(a));
    }



    /// <summary>
    ///   Round up to the next power of 2
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint UpperPow2(uint a) {
      a--;
      a |= a >> 1;
      a |= a >> 2;
      a |= a >> 4;
      a |= a >> 8;
      a |= a >> 16;
      return a + 1;
    }



    /// <summary>
    ///   Round up to the next power of 2
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UpperPow2(int a) {
      return (int)UpperPow2((uint)a);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(this int a) {
      return a.ToString("X8");
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundToInt(double a) {
      return (int)(a + .5);
    }



    #region Convertions

    public const byte BYTE_MAX = byte.MaxValue;
    public const byte BYTE_MIN = byte.MinValue;



    /// <summary>
    ///   Unsigned modulo that works like module, but its results are always positive
    /// </summary>
    /// <param name="x"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Umod(int x, int m) {
      return (x % m + m) % m;
    }



    public static double AvgAbs(this Complex[] thisArray) {
      return SumAbs(thisArray) / thisArray.Length;
    }



    public static double SumAbs(this Complex[] thisArray) {
      return thisArray.Sum(c => c.Magnitude);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ToBase256(this double value) {
      return value < 0 ? BYTE_MIN : (value > 1 ? BYTE_MAX : (byte)(value * BYTE_MAX));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ToBase256(this float value) {
      return value < 0 ? BYTE_MIN : (value > 1 ? BYTE_MAX : (byte)(value * BYTE_MAX));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ClampToByte(this double value) {
      return value < BYTE_MIN ? BYTE_MIN : (value > BYTE_MAX ? BYTE_MAX : (byte)value);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ClampToByte(this float value) {
      return value < BYTE_MIN ? BYTE_MIN : (value > BYTE_MAX ? BYTE_MAX : (byte)value);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(this int value, int minValue, int maxValue) {
      return value < minValue ? minValue : (value > maxValue ? maxValue : value);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(this float value, float minValue, float maxValue) {
      return value < minValue ? minValue : (value > maxValue ? maxValue : value);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Clamp(this double value, double minValue, double maxValue) {
      return value < minValue ? minValue : (value > maxValue ? maxValue : value);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex Transpose(this Complex c) {
      return new Complex(c.Imaginary, c.Real);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Hypothenuse(double a, double b) {
      return Math.Sqrt(a * a + b * b);
    }

    #endregion

    #region Error Measure

    public static int Correlation(this byte[] a1, byte[] a2) {
      var n = a1.Length;
      if (n != a2.Length) {
        throw new Exception("Array's are not in the same length");
      }

      var result = 0;
      for (var i = 0; i < n; i++) {
        result += Math.Abs(a2[i] - a1[i]);
      }

      return result;
    }



    /// <summary>
    ///   Calculates the variance and the mean of the values
    /// </summary>
    /// <param name="values"></param>
    /// <param name="mean"></param>
    /// <returns></returns>
    public static double Variance(this double[] values, out double mean) {
      var oneByN = 1.0 / values.Length;
      var mu = mean = values.Sum() * oneByN;
      return values.Sum(value => Pow2(value - mu)) * oneByN;
    }



    /// <summary>
    ///   Calculates the variance and the mean of the values
    /// </summary>
    /// <param name="values"></param>
    /// <param name="mean"></param>
    /// <returns></returns>
    public static double Variance(this int[] values, out double mean) {
      var oneByN = 1.0 / values.Length;
      var mu = mean = values.Sum() * oneByN;
      return values.Sum(value => Pow2(value - mu)) * oneByN;
    }



    /// <summary>
    ///   Calculates the variance of the values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static double Variance(this IEnumerable<double> values) {
      return Variance(values as double[] ?? values.ToArray());
    }



    /// <summary>
    ///   Calculates the variance of the values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static double Variance(this double[] values) {
      var oneByN = 1.0 / values.Length;
      var mu = values.Sum() * oneByN;
      return values.Sum(value => Pow2(value - mu)) * oneByN;
    }



    /// <summary>
    ///   Calculates the variance of the values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static double Variance(this int[] values) {
      var oneByN = 1.0 / values.Length;
      var mu = values.Sum() * oneByN;
      return values.Sum(value => Pow2(value - mu)) * oneByN;
    }



    /// <summary>
    ///   Calculates the sum squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static double GetSad(this double[] x1, double[] x2) {
      var n = x1.Length;
      if (n != x2.Length) {
        throw new ArgumentException("x1 and x2 are not in the same length.");
      }

      var result = .0;
      for (var i = 0; i < n; i++) {
        result += Math.Abs(x2[i] - x1[i]);
      }

      return result;
    }



    /// <summary>
    ///   Calculates the sum squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static double GetSad(this float[] x1, float[] x2) {
      var n = x1.Length;
      if (n != x2.Length) {
        throw new ArgumentException("x1 and x2 are not in the same length.");
      }

      var result = .0;
      for (var i = 0; i < n; i++) {
        result += Math.Abs(x2[i] - x1[i]);
      }

      return result;
    }



    /// <summary>
    ///   Calculates the sum squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static int GetSad(int[] x1, int[] x2) {
      var n = x1.Length;
      if (n != x2.Length) {
        throw new ArgumentException("x1 and x2 are not in the same length.");
      }

      var result = 0;
      for (var i = 0; i < n; i++) {
        result += Math.Abs(x2[i] - x1[i]);
      }

      return result;
    }



    /// <summary>
    ///   Calculates the sum squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static double GetSse(double[] x1, double[] x2) {
      var n = x1.Length;
      if (n != x2.Length) {
        throw new ArgumentException("x1 and x2 are not in the same length.");
      }

      var result = .0;
      for (var i = 0; i < n; i++) {
        result += Pow2(x2[i] - x1[i]);
      }

      return result;
    }



    /// <summary>
    ///   Calculates the sum squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSse(int[] x1, int[] x2) {
      var n = x1.Length;
      //if (n != x2.Length)
      //  throw new ArgumentException("x1 and x2 are not in the same length.");
      var result = 0;
      for (var i = 0; i < n; i++) {
        result += Pow2(x2[i] - x1[i]);
      }

      return result;
    }



    /// <summary>
    ///   Calculates the mean squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static double GetMse(int[] x1, int[] x2) {
      var n = x1.Length;
      if (n != x2.Length) {
        throw new ArgumentException("x1 and x2 are not in the same length.");
      }

      var result = 0;
      for (var i = 0; i < n; i++) {
        result += Pow2(x2[i] - x1[i]);
      }

      return result / (double)n;
    }



    /// <summary>
    ///   Calculates the mean squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static double GetMse(double[] x1, double[] x2) {
      var n = x1.Length;
      if (n != x2.Length) {
        throw new ArgumentException("x1 and x2 are not in the same length.");
      }

      var result = .0;
      for (var i = 0; i < n; i++) {
        result += Pow2(x2[i] - x1[i]);
      }

      return result / n;
    }

    #endregion

    #region sin, cos

    public const double DEG_TO_RAD = 180 / PI;
    public const double RAD_TO_DEG = PI / 180;



    /// <summary>
    ///   Converts euler angle from radiant to degree
    /// </summary>
    /// <param name="phi"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToDeg(double phi) {
      return DEG_TO_RAD * phi;
    }



    /// <summary>
    ///   Converts euler angle from degree to radiant
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToRad(double angle) {
      return RAD_TO_DEG * angle;
    }

    #endregion

    #region Interpolation

    private const double twoPi = Math.PI * 2;
    private const double threePi = Math.PI * 3;



    /// <summary>
    ///   Interpolates between phase1 and phase2 by weighting the shortes angle and adding it to phase1
    /// </summary>
    /// <param name="phase1">p</param>
    /// <param name="phase2"></param>
    /// <param name="a">weight, if zero it returns phase </param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double InterpolatePhase(double phase1, double phase2, double a) {
      var shortestAngle = ((phase2 % twoPi - phase1 % twoPi) % twoPi + threePi) % twoPi - Math.PI;
      return phase1 + shortestAngle * a;
    }



    /// <summary>
    ///   Bilinear interpolation of a square of four pixels.
    /// </summary>
    /// <param name="p00"></param>
    /// <param name="p01"></param>
    /// <param name="p10"></param>
    /// <param name="p11"></param>
    /// <param name="ax"></param>
    /// <param name="ay"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double InterpolateBilinear(double p00, double p01, double p10, double p11, double ax, double ay) {
      return ((1 - ax) * p00 + ax * p01) * (1 - ay) + ((1 - ax) * p10 + ax * p11) * ay;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex InterpolatePolarLinear(Complex p0, Complex p1, double a) {
      return Complex.FromPolarCoordinates(
        (1 - a) * p0.Magnitude + a * p1.Magnitude,
        InterpolatePhase(p0.Phase, p1.Phase, a)
      );
      //((1 - ax) * p00.Phase + ax * p01.Phase) * (1 - ay) + ((1 - ax) * p10.Phase + ax * p11.Phase) * ay);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex
      InterpolateBilinear(Complex p00, Complex p01, Complex p10, Complex p11, double ax, double ay) {
      return ((1 - ax) * p00 + ax * p01) * (1 - ay) + ((1 - ax) * p10 + ax * p11) * ay;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex InterpolatePolarBilinear(Complex p00,
                                                   Complex p01,
                                                   Complex p10,
                                                   Complex p11,
                                                   double ax,
                                                   double ay) {
      return Complex.FromPolarCoordinates(
        ((1 - ax) * p00.Magnitude + ax * p01.Magnitude) * (1 - ay) +
        ((1 - ax) * p10.Magnitude + ax * p11.Magnitude) * ay,
        InterpolatePhase(InterpolatePhase(p00.Phase, p01.Phase, ax), InterpolatePhase(p10.Phase, p11.Phase, ax), ay)
      );
      //((1 - ax) * p00.Phase + ax * p01.Phase) * (1 - ay) + ((1 - ax) * p10.Phase + ax * p11.Phase) * ay);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex InterpolateCartesianBilinear(Complex p00,
                                                       Complex p01,
                                                       Complex p10,
                                                       Complex p11,
                                                       double ax,
                                                       double ay) {
      return new Complex(
        ((1 - ax) * p00.Real + ax * p01.Real) * (1 - ay) + ((1 - ax) * p10.Real + ax * p11.Real) * ay,
        ((1 - ax) * p00.Imaginary + ax * p01.Imaginary) * (1 - ay) +
        ((1 - ax) * p10.Imaginary + ax * p11.Imaginary) * ay
      );
    }

    #endregion
  }
}
