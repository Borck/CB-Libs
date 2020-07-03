using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;



namespace CB.System.Numerics {
  public static class Statistics {
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
      return values.Sum(value => Mathh.Pow2(value - mu)) * oneByN;
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
      return values.Sum(value => Mathh.Pow2(value - mu)) * oneByN;
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
      return values.Sum(value => Mathh.Pow2(value - mu)) * oneByN;
    }



    /// <summary>
    ///   Calculates the variance of the values
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static double Variance(this int[] values) {
      var oneByN = 1.0 / values.Length;
      var mu = values.Sum() * oneByN;
      return values.Sum(value => Mathh.Pow2(value - mu)) * oneByN;
    }



    /// <summary>
    ///   Calculates the sum squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static double Sad(this double[] x1, double[] x2) {
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
    public static double Sad(this float[] x1, float[] x2) {
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
    public static int Sad(this int[] x1, int[] x2) {
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
    public static double Sse(this double[] x1, double[] x2) {
      var n = x1.Length;
      if (n != x2.Length) {
        throw new ArgumentException("x1 and x2 are not in the same length.");
      }

      var result = .0;
      for (var i = 0; i < n; i++) {
        result += Mathh.Pow2(x2[i] - x1[i]);
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
    public static int Sse(this int[] x1, int[] x2) {
      var n = x1.Length;
      //if (n != x2.Length)
      //  throw new ArgumentException("x1 and x2 are not in the same length.");
      var result = 0;
      for (var i = 0; i < n; i++) {
        result += Mathh.Pow2(x2[i] - x1[i]);
      }

      return result;
    }



    /// <summary>
    ///   Calculates the mean squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static double Mse(this int[] x1, int[] x2) {
      var n = x1.Length;
      if (n != x2.Length) {
        throw new ArgumentException("x1 and x2 are not in the same length.");
      }

      var result = 0;
      for (var i = 0; i < n; i++) {
        result += Mathh.Pow2(x2[i] - x1[i]);
      }

      return result / (double)n;
    }



    /// <summary>
    ///   Calculates the mean squared error.
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <returns></returns>
    public static double Mse(this double[] x1, double[] x2) {
      var n = x1.Length;
      if (n != x2.Length) {
        throw new ArgumentException("x1 and x2 are not in the same length.");
      }

      var result = .0;
      for (var i = 0; i < n; i++) {
        result += Mathh.Pow2(x2[i] - x1[i]);
      }

      return result / n;
    }
  }
}
