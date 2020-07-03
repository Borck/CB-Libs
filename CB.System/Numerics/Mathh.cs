using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;



namespace CB.System.Numerics {
  public static class Mathh {
    // see: http://graphics.stanford.edu/~seander/bithacks.html

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


    public const double RAD_TO_DEG = 180 / PI;
    public const double DEG_TO_RAD = PI / 180;



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
    public static bool IsPow2(int a) {
      return (a & (a - 1)) == 0;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Pow2(int a) {
      return a * a;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Pow2(float a) {
      return a * a;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Pow2(double a) {
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
    public static uint UpperPow2(float a) {
      return UpperPow2((uint)Math.Ceiling(a));
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



    #region Convertions

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



    public static double AvgAbs(Complex[] thisArray) {
      return SumAbs(thisArray) / thisArray.Length;
    }



    public static double SumAbs(Complex[] thisArray) {
      return thisArray.Sum(c => c.Magnitude);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Base256(double value) {
      return value < 0 ? byte.MinValue : (value > 1 ? byte.MaxValue : (byte)(value * byte.MaxValue));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Base256(float value) {
      return value < 0 ? byte.MinValue : (value > 1 ? byte.MaxValue : (byte)(value * byte.MaxValue));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ClampToByte(double value) {
      return value < byte.MinValue ? byte.MinValue : (value > byte.MaxValue ? byte.MaxValue : (byte)value);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ClampToByte(this float value) {
      return value < byte.MinValue ? byte.MinValue : (value > byte.MaxValue ? byte.MaxValue : (byte)value);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Hypothenuse(double a, double b) {
      return Math.Sqrt(a * a + b * b);
    }

    #endregion



    #region Interpolation

    /// <summary>
    ///   Interpolates between phase1 and phase2 by weighting the shortes angle and adding it to phase1
    /// </summary>
    /// <param name="phase1">p</param>
    /// <param name="phase2"></param>
    /// <param name="a">weight, if zero it returns phase </param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double InterpolatePhase(double phase1, double phase2, double a = .5) {
      var shortestAngle = ((phase2 % (2 * PI) - phase1 % (2 * PI)) % (2 * PI) + 3 * PI) % (2 * PI) - PI;
      return phase1 + shortestAngle * a;
    }



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
