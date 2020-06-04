using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Accord.Imaging;
using Accord.Math;
using Accord.Math.Transforms;
using CB.System;
using CB.System.Collections;
using JetBrains.Annotations;



namespace CB.CV.Imaging.Filtering {
  public static class Fourier {
    //https://code.google.com/p/tope-fft/source/browse/src/fft2d.c?r=f21634ae71afef613b708d7f2cc3641785fe32ee


    #region fft and ifft

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DoWeight(Complex[][] thisData, double a) {
      var n = thisData.Length;
      for (var i = 0; i < n; i++) {
        var dl = thisData[i];
        for (var j = 0; j < dl.Length; j++) {
          dl[j] *= a;
        }
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DoWeight(Complex2D c, double a) {
      var d = c.Data;
      for (int i = 0,
               n = d.Length;
           i < n;
           i++) {
        d[i] *= a;
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DoFFT2Rows(this Complex[][] thisData) {
      //TODO: weighting;
      foreach (var row in thisData) {
        FourierTransform2.FFT(row, FourierTransform.Direction.Forward);
      }

      DoWeight(thisData, 1.0 / thisData[0].Length);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DoFFT2Cols(this Complex[][] thisData) {
      var d = thisData.Transpose();
      foreach (var col in d) {
        FourierTransform2.FFT(col, FourierTransform.Direction.Forward);
      }

      d.CopyTransposed(thisData);
      DoWeight(thisData, 1.0 / thisData.Length);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DoIFFT2Rows(this Complex[][] thisData) {
      foreach (var row in thisData) {
        FourierTransform2.FFT(row, FourierTransform.Direction.Backward);
      }

      DoWeight(thisData, thisData[0].Length);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DoInverseFFT2Cols(this Complex[][] thisData) {
      var d = thisData.Transpose();
      foreach (var col in d) {
        FourierTransform2.FFT(col, FourierTransform.Direction.Backward);
      }

      d.CopyTransposed(thisData);
      DoWeight(thisData, thisData.Length);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DoFFT2(this Complex2D thisImage, Complex2D outImage) {
      var data = thisImage.ToJagged();
      FourierTransform2.FFT2(data, FourierTransform.Direction.Forward);

      //TODO create new release of CB.System

      data.Copy(outImage.Data);
      DoWeight(outImage, 1d / outImage.Data.Length);
    }



    public static Complex2D FFT2([NotNull] this Complex2D thisChannel, bool useCopy = true) {
      var result = useCopy ? new Complex2D(thisChannel.Width, thisChannel.Height) : thisChannel;
      DoFFT2(thisChannel, result);
      return result;
    }



    public static void FFT2(this Complex2D thisChannel, Complex2D outChannel) {
      Complex2D.CheckComparable(thisChannel, outChannel);
      DoFFT2(thisChannel, outChannel);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DoFFT2(this ComplexImage thisImage, ComplexImage outImage) {
      DoFFT2(thisImage.C0, outImage.C0);
      DoFFT2(thisImage.C1, outImage.C1);
      DoFFT2(thisImage.C2, outImage.C2);
    }



    public static ComplexImage FFT2([NotNull] this ComplexImage thisImage, bool useCopy = true) {
      var result = useCopy ? new ComplexImage(thisImage.Width, thisImage.Height) : thisImage;
      DoFFT2(thisImage, result);
      return result;
    }



    public static void FFT2(this ComplexImage thisImage, ComplexImage outImage) {
      ComplexImage.CheckComparable(thisImage, outImage);
      DoFFT2(thisImage, outImage);
    }



    public static ComplexImage FFT2(UnmanagedImage image, bool zeroPadding = true) {
      var result = zeroPadding
                     ? image.ToComplexImageZeroPad(
                       (int)MathCV.UpperPow2((uint)image.Width),
                       (int)MathCV.UpperPow2((uint)image.Height)
                     )
                     : image.ToComplexImageZeroPad(image.Width, image.Height);
      return result.FFT2(false);
    }



    public static ComplexImage FFT2(Bitmap image, bool zeroPadding = true) {
      return FFT2(image.ToUnmanaged(), zeroPadding);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DoInverseFFT2(this Complex2D thisChannel, Complex2D outChannel) {
      var data = thisChannel.ToJagged();
      FourierTransform2.FFT2(data, FourierTransform.Direction.Backward);
      data.Copy(outChannel.Data);
      DoWeight(outChannel, outChannel.Data.Length);
    }



    public static Complex2D InverseFFT2([NotNull] this Complex2D thisChannel, bool useCopy = true) {
      var result = useCopy ? new Complex2D(thisChannel.Width, thisChannel.Height) : thisChannel;
      DoInverseFFT2(thisChannel, result);
      return result;
    }



    public static void InverseFFT2(this Complex2D thisChannel, Complex2D outChannel) {
      Complex2D.CheckComparable(thisChannel, outChannel);
      DoInverseFFT2(thisChannel, outChannel);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DoInverseFFT2(this ComplexImage thisImage, ComplexImage outImage) {
      DoInverseFFT2(thisImage.C0, outImage.C0);
      DoInverseFFT2(thisImage.C1, outImage.C1);
      DoInverseFFT2(thisImage.C2, outImage.C2);
    }



    public static ComplexImage InverseFFT2([NotNull] this ComplexImage thisImage, bool useCopy = true) {
      var result = useCopy ? new ComplexImage(thisImage.Width, thisImage.Height) : thisImage;
      DoInverseFFT2(thisImage, result);
      return result;
    }



    public static void InverseFFT2(this ComplexImage thisImage, ComplexImage outImage) {
      ComplexImage.CheckComparable(thisImage, outImage);
      DoInverseFFT2(thisImage, outImage);
    }

    #endregion

    #region shifts

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DoFFTShift(ComplexImage image1, ComplexImage image2) {
      DoFFTShift(image1.C0, image2.C0);
      DoFFTShift(image1.C1, image2.C1);
      DoFFTShift(image1.C2, image2.C2);
    }



    public static ComplexImage FFTShift([NotNull] this ComplexImage thisImage, bool useCopy = true) {
      var result = useCopy ? new ComplexImage(thisImage.Width, thisImage.Height) : thisImage;
      DoFFTShift(thisImage, result);
      return result;
    }



    public static void FFTShift(this ComplexImage thisImage, ComplexImage outImage) {
      ComplexImage.CheckComparable(thisImage, outImage);
      DoFFTShift(thisImage, outImage);
    }



    /// <summary>
    /// </summary>
    /// <param name="c1">source channel</param>
    /// <param name="c2">destination channel</param>
    private static void DoFFTShift(Complex2D c1, Complex2D c2) {
      var d1 = c1.Data;
      var d2 = c2.Data;
      var w = c1.Width;
      var wSkip = w - w / 2;

      var h = d1.Length / (w * 2); // half height
      var k0 = w * h; // first index of quandrant three

      w = w - wSkip; // half width
      var l0 = k0 + w; // first index of quandrant four

      for (var i = 0; i < k0; i += wSkip) {
        for (var iToEx = i + w; i < iToEx; i++) {
          // swap lu and rd
          var tmp = d1[i]; // introduce additional variable, if srcData==dstData 
          d2[i] = d1[i + l0];
          d2[i + l0] = tmp;

          // swap ld and ru
          tmp = d1[i + w];
          d2[i + w] = d1[i + k0];
          d2[i + k0] = tmp;
        }
      }
    }



    public static Complex2D FFTShift(this Complex2D thisChannel, bool useCopy = true) {
      var result = useCopy ? new Complex2D(thisChannel.Width, thisChannel.Height) : thisChannel;
      DoFFTShift(thisChannel, result);
      return result;
    }



    public static void FFTShift(this Complex2D thisChannel, Complex2D outChannel) {
      Complex2D.CheckComparable(thisChannel, outChannel);
      DoFFTShift(thisChannel, outChannel);
    }



    // ReSharper disable once InconsistentNaming
    public static void FFTShift(ComplexImage4D input) {
      // get length of each dimention
      var di = input.D1Size;
      var dj = input.D2Size;
      var dk = input.D3Size;
      var dl = input.D4Size;

      var diA = di / 2;
      var diB = di - diA;
      var djA = dj / 2;
      var djB = dj - djA;
      var dkA = dk / 2;
      var dkB = dk - dkA;
      var dlA = dl / 2;
      var dlB = dl - dlA;

      input.ForEachColor(
        c => {
          for (var i = 0; i < diA; i++) {
            var x = (diB + i) % di;

            for (var j = 0; j < djA; j++) {
              var y = (djB + j) % dj;
              var cij = c[i, j];
              var cxy = c[x, y];

              // TODO: refactor
              for (var k = 0; k < dkA; k++) {
                var z = (dkB + k) % dk;
                for (var l = 0; l < dlA; l++) {
                  ObjectX.Swap(ref cij[k, l], ref cxy[z, (dlB + l) % dl]);
                }
              }
            }
          }
        }
      );
    }



    public static void InverseFFTShift(ComplexImage4D input) {
      // TODO: correct?
      FFTShift(input);
    }



    private static void DoFFTShiftFreqz(Complex2D c1, Complex2D c2) {
      var d1 = c1.Data;
      var d2 = c2.Data;
      var w = c1.Width;

      if (w % 2 == 1) {
        for (var i = 1; i < d1.Length; i += 2) {
          d2[i] = d1[i] * -1;
        }

        return;
      }

      var i0 = 0;
      for (var j = 0; j < d1.Length; j += w) {
        for (int i = j - (i0 = ~i0),
                 iTo = j + w;
             i < iTo;
             i += 2) {
          d2[i] = d1[i] * -1;
        }
      }
    }



    /// <summary>
    ///   Applies a fft shift in frequency domain.
    /// </summary>
    /// <param name="thisChannel"></param>
    /// <param name="useCopy"></param>
    /// <returns></returns>
    public static Complex2D FFTShiftFreqz([NotNull] this Complex2D thisChannel, bool useCopy = true) {
      var result = useCopy ? new Complex2D(thisChannel.Width, thisChannel.Height) : thisChannel;
      DoFFTShiftFreqz(thisChannel, result);
      return result;
    }



    public static void FFTShiftFreqz(this Complex2D thisChannel, Complex2D outChannel) {
      Complex2D.CheckComparable(thisChannel, outChannel);
      DoFFTShiftFreqz(thisChannel, outChannel);
    }



    private static void DoFFTShiftPhaseV(Complex2D c1, Complex2D c2, int y0) {
      var d1 = c1.Data;
      var d2 = c2.Data;
      var w = c1.Width;
      var dPhi = -2 * Math.PI * y0 / c1.Height;
      for (int i = w,
               v = 1;
           i < d1.Length;
           v++) {
        var a = Complex.FromPolarCoordinates(1, dPhi * v);
        for (var iTo = i + w; i < iTo; i++) {
          d2[i] = d1[i] * a;
        }
      }
    }



    public static Complex2D FFTShiftPhaseV(this Complex2D thisChannel, int y0, bool useCopy = true) {
      var result = useCopy ? new Complex2D(thisChannel.Width, thisChannel.Height) : thisChannel;
      DoFFTShiftPhaseV(thisChannel, result, y0);
      return result;
    }



    public static void FFTShiftPhaseV(this Complex2D thisChannel, Complex2D outChannel, int y0) {
      Complex2D.CheckComparable(thisChannel, outChannel);
      DoFFTShiftPhaseV(thisChannel, outChannel, y0);
    }

    #endregion

    #region flip

    public static void FlipUd(Complex2D c) {
      var w = c.Width;
      var d = c.Data;
      var tmp = new Complex[w];
      var n = w * c.Height / 2;
      for (int i = 0,
               j = w * (c.Height - 1);
           i < n;
           i += w, j -= w) {
        Array.Copy(d, i, tmp, 0, w);
        Array.Copy(d, j, d, i, w);
        Array.Copy(tmp, 0, d, j, w);
      }
    }



    /// <summary>
    ///   Flip the image upside down in fourier space
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    private static void DoFlipUdPhase(Complex2D c1, Complex2D c2) {
      var w = c1.Width;
      var d1 = c1.Data;
      var d2 = c2.Data;
      var tmp = new Complex[w];
      var n = w * c1.Height / 2;

      if (d1 == d2) {
        for (int i = w,
                 j = w * (c1.Height - 1);
             i < n;
             i += w, j -= w) {
          Array.Copy(d1, i, tmp, 0, w);
          Array.Copy(d1, j, d1, i, w);
          Array.Copy(tmp, 0, d1, j, w);
        }
      } else {
        for (int i = w,
                 j = w * (c1.Height - 1);
             i < n;
             i += w, j -= w) {
          Array.Copy(d1, i, d2, j, w);
          Array.Copy(d1, j, d2, i, w);
        }
      }

      //FFTShiftPhaseV(c,-1);
    }



    /// <summary>
    ///   Flip the image upside down in fourier space
    /// </summary>
    /// <param name="thisChannel"></param>
    /// <param name="useCopy">use a copy of this channel for transformation</param>
    public static Complex2D FlipUdPhase([NotNull] this Complex2D thisChannel, bool useCopy = true) {
      var result = useCopy ? new Complex2D(thisChannel.Width, thisChannel.Height) : thisChannel;
      DoFlipUdPhase(thisChannel, result);
      return result;
    }



    /// <summary>
    ///   Flip the image upside down in fourier space
    /// </summary>
    /// <param name="thisChannel"></param>
    /// <param name="outChannel"></param>
    public static void FlipUdPhase(this Complex2D thisChannel, Complex2D outChannel) {
      Complex2D.CheckComparable(thisChannel, outChannel);
      DoFlipUdPhase(thisChannel, outChannel);
    }

    #endregion

    #region rotate

    /// <summary>
    /// </summary>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="phi"></param>
    /// <param name="zCc">z* - z with  complex conjugation</param>
    /// <returns>z</returns>
    public static Complex2D DoPrepareRotationFilters(int w, int h, double phi, out Complex2D zCc) {
      var result = new Complex2D(w, h);
      zCc = new Complex2D(w, h);
      var dtR = result.Data;
      var dtC = zCc.Data;

      var wh = w / 2;
      var hh = h / 2;

      var a1 = -2 * Math.PI / w * Math.Sin(phi);
      var b = Math.PI / w * Math.Cos(phi);

      //TODO: should one divided by h instead of w?
      for (int q = hh,
               k = 0;
           q > -hh;
           q--) {
        var qq = q * q;
        var a2 = q * a1;
        for (var p = -wh; p < wh; p++, k++) {
          var zPhi = (p * p - qq) * b + a2 * p;
          dtR[k] = Complex.FromPolarCoordinates(1, zPhi);
          dtC[k] = Complex.FromPolarCoordinates(1, -zPhi);
        }
      }

      //TODO: cache in LRU cache

      return result;
    }



    /// <summary>
    ///   Flip array up to down. flipud(A) returns A with its rows flipped in the up-down direction (that is, about a
    ///   horizontal axis).
    ///   If A is a column vector, then flipud(A) returns a vector of the same length with the order of its elements
    ///   reversed.If A is a row vector, then flipud(A) simply returns A.For multidimensional arrays, flipud operates on the
    ///   planes formed by the first and second dimensions.
    /// </summary>
    public const double D_ANGLE = 0.01;



    /// <summary>
    ///   Rotations the image anti-clockwise in frequency domain.
    /// </summary>
    /// <param name="thisFrequencies">the image</param>
    /// <param name="angle">angle in degree</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Complex2D RotateColcer(Complex2D thisFrequencies, double angle) {
      if (Math.Abs(angle) < D_ANGLE) {
        return new Complex2D(thisFrequencies);
      }

      angle = MathCV.ToRad(angle);

      var w = thisFrequencies.Width;
      var h = thisFrequencies.Height;
      var l = w * h;

      var result = new Complex2D(thisFrequencies);

      //marker 1
      DoFFTShiftFreqz(result, result);
      DoFFTShift(result, result);

      Complex2D zi;
      var z = DoPrepareRotationFilters(w, h, angle % Math.PI, out zi);

      result.DivideBy(l);
      result.MultiplyWith(z);

      DoFFT2(result, result);
      DoFFT2(zi, zi);

      result.MultiplyWith(zi);

      DoFlipUdPhase(result, result);
      DoFFTShiftFreqz(result, result);
      return result;
    }



    private const double DPHI = 0.0001;



    /// <summary>
    ///   Fits a coordinate a in the fourier space fourier one extra cycle in each direction to comply with its cycle property
    /// </summary>
    /// <param name="u">fouier space coordinate</param>
    /// <param name="ul">number of frequencies along u</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FitToCycle(this int u, int ul) {
      return u < 0 ? u + ul : u >= ul ? u - ul : u;
    }



    public static Complex2D RotateNearestNeighbor(this Complex2D thisFrequencies, double angle) {
      angle = (angle % 360 + 360) % 360;

      if (Math.Abs(angle) < D_ANGLE) {
        return new Complex2D(thisFrequencies);
      }

      var w = thisFrequencies.Width;
      var h = thisFrequencies.Height;
      var sinPhi = Math.Sin(angle = MathCV.ToRad(angle));
      var cosPhi = Math.Cos(angle);

      var m = w / 2;
      var n = h / 2;

      var tmp = new Complex2D(w, h);
      FFTShift(thisFrequencies, tmp);
      var d1 = tmp.Data;

      var result = new Complex2D(w, h);
      var d2 = result.Data;

      var x0 = m + .5;
      var y0 = n + .5;

      for (int v = -n,
               k = 0;
           v < h - n;
           v++) {
        for (var u = -m; u < w - m; u++, k++) {
          var x = ((int)(u * cosPhi - v * sinPhi + x0)).FitToCycle(w);
          var y = ((int)(u * sinPhi + v * cosPhi + y0)).FitToCycle(h);
          d2[k] = d1[y * w + x];
        }
      }

      FFTShift(result, false);
      return result;
    }



    public static Complex2D RotateBilinear(this Complex2D thisFrequencies, double angle) {
      if (Math.Abs(angle) < D_ANGLE) {
        return new Complex2D(thisFrequencies);
      }

      var w = thisFrequencies.Width;
      var h = thisFrequencies.Height;
      var sinPhi = Math.Sin(angle = MathCV.ToRad(angle));
      var cosPhi = Math.Cos(angle);

      var m = w / 2;
      var n = h / 2;

      var tmp = new Complex2D(w, h);
      FFTShift(thisFrequencies, tmp);
      var d1 = tmp.Data;

      var result = new Complex2D(w, h);
      var d2 = result.Data;

      for (int v = -n,
               k = 0;
           v < h - n;
           v++) {
        for (var u = -m; u < w - m; u++, k++) {
          var x = (u * cosPhi - v * sinPhi + m);
          var y = (u * sinPhi + v * cosPhi + n);
          var x1 = (int)x;
          var y1 = (int)y;
          var ax = x1 - x;
          var ay = y1 - y;

          x1 = x1.FitToCycle(w);
          y1 = y1.FitToCycle(h);
          var x2 = (x1 + 1).FitToCycle(w);
          var y2 = (y1 + 1).FitToCycle(h);

          var i01 = y1 * w;
          var i02 = y2 * w;
          //d2[k] = MathCV.InterpolateBilinear(d1[i01 + x1], d1[i01 + x2], d1[i02 + x1], d1[i02 + x2], ax, ay);
          d2[k] = MathCV.InterpolatePolarBilinear(d1[i01 + x1], d1[i01 + x2], d1[i02 + x1], d1[i02 + x2], ax, ay);
        }
      }

      FFTShift(result, false);
      return result;
    }



    private static Complex ScalarProduct(Complex[] data, double[][] mask) {
      var h = mask.Length;
      var w = mask[0].Length;
      var result = Complex.Zero;
      for (int y = 0,
               i = 0;
           y < h;
           y++) {
        var mLine = mask[y];
        for (var x = 0; x < w; x++, i++) {
          result += data[i] * mLine[x];
        }
      }

      return result;
    }



    private static object InitializeJaggedArray(Type type, int index, int[] lengths) {
      var array = Array.CreateInstance(type, lengths[index]);
      var elementType = type.GetElementType();
      if (elementType == null)
        return array;
      for (var i = 0; i < lengths[index]; i++)
        array.SetValue(InitializeJaggedArray(elementType, index + 1, lengths), i);
      return array;
    }



    public static Complex2D RotateSincInterp(this Complex2D thisFrequencies, double angle) {
      if (Math.Abs(angle) < D_ANGLE) {
        return new Complex2D(thisFrequencies);
      }

      var w = thisFrequencies.Width;
      var h = thisFrequencies.Height;
      var sinPhi = Math.Sin(angle = MathCV.ToRad(angle));
      var cosPhi = Math.Cos(angle);

      var m = w / 2;
      var n = h / 2;

      var tmp = new Complex2D(w, h);
      FFTShift(thisFrequencies, tmp);
      var d1 = tmp.Data;

      var result = new Complex2D(w, h);
      var d2 = result.Data;

      var mask = ArrayX.CreateJaggedArray<double[][]>(w, h);
      for (int v = -n,
               k = 0;
           v < h - n;
           v++) {
        for (var u = -m; u < w - m; u++, k++) {
          var x = u * cosPhi - v * sinPhi + m;
          var y = u * sinPhi + v * cosPhi + n;
          //MaskFactory.Sincd2Periodic(mask,x,y);

          MaskFactory.Sincd2(mask, x, y);
          d2[k] = ScalarProduct(d1, mask);

          //          var x = ((int)(u*cosPhi-v*sinPhi+x0));
          //          var y = ((int)(u*sinPhi+v*cosPhi+y0));
          //          if (x < 0 || x >= w || y < 0 || y >= h) 
          //            continue;
          //          
          //          d2[k]=d1[y*w+x];
        }
      }

      FFTShift(result, false);
      return result;
    }



    /// <summary>
    ///   Rotation the Complex2D in frequency domain. This algorithms is derived from an algorithms in:
    ///   Owen et.al., "High Quality Alias Free Image Rotation", [1996].
    /// </summary>
    /// <param name="thisFrequencies"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static Complex2D Rotate(this Complex2D thisFrequencies, double angle) {
      angle = (angle % 360 + 360) % 360;
      var rot = MathCV.RoundToInt(angle / 180);
      angle -= rot * 180;
      angle = MathCV.ToRad(angle);

      if (Math.Abs(angle) < DPHI) {
        if (rot == 0) {
          return thisFrequencies.Clone();
        }

        var result = new Complex2D(thisFrequencies.Width, thisFrequencies.Height);
        var dc = thisFrequencies.Data;
        var dr = result.Data;
        for (var i = 0; i < dc.Length; i++) {
          dr[i] = Complex.Conjugate(dc[i]);
        }

        return result;
      }

      var tph = Math.Tan(angle / 2);
      var k = thisFrequencies.Width;
      var l = thisFrequencies.Height;

      var m = (int)MathCV.UpperPow2(k * (1 + tph * tph));
      var n = MathCV.UpperPow2(2 * l);
      var d = new Complex[n / 2][];
      //-- step 1: first widening - horizontal zero padding
      {
        var dc = thisFrequencies.Data;

        if (rot % 2 == 0) {
          for (int i = 0,
                   y = 0;
               y < l;
               y++) {
            var d1Row = d[y] = new Complex[m];
            for (var j = 0; j < m; j += 4, i += 2) {
              d1Row[j] = 2 * dc[i];
              d1Row[j + 2] = -2 * dc[i + 1];
            }
          }
        } else {
          for (int i = 0,
                   y = 0;
               y < l;
               y++) {
            var d1Row = d[y] = new Complex[m];
            for (var j = 0; j < m; j += 4, i += 2) {
              d1Row[j] = Complex.Conjugate(2 * dc[i]);
              d1Row[j + 2] = Complex.Conjugate(-2 * dc[i + 1]);
            }
          }
        }
      }

      //-- step 2: IFFT on columns
      DoInverseFFT2Cols(d);

      //-- step 3: first skew (horizontal)
      {
        var a = 2.0 / (n / 2 - 1);
        var b = tph / 2 * k;
        var c = -2 * Math.PI / m;
        var ab = a * b;

        var y = 0;
        foreach (var d1Row in d) {
          var delta = c * Math.Round(ab * y++ - b);
          for (var x = 0; x < d1Row.Length; x++) {
            d1Row[x] *= Complex.FromPolarCoordinates(1, x * delta);
          }
        }
      }

      //-- step 4: fft on columns
      DoFFT2Cols(d);

      //-- step 5:  second widening
      {
        var d2 = new Complex[n][];
        for (int y2 = 0,
                 y1 = 0;
             y2 < n;
             y2 += 4, y1 += 2) {
          d2[y2 + 1] = new Complex[m];
          d2[y2 + 3] = new Complex[m];

          var row1 = d2[y2] = d[y1]; // even rows
          var row2 = d2[y2 + 2] = d[y1 + 1]; // odd rows
          for (int x = 0; x < row1.Length; x++) { // negate
            row1[x] *= 2;
            row2[x] *= -2;
          }
        }

        d = d2;
      }

      //-- step 6: IFFT on rows
      DoIFFT2Rows(d);

      //-- step 7: second skew (vertical)
      {
        var a = 2.0 / (n - 1);
        var b = Math.Sin(angle) * n / 2;
        var c = 2 * Math.PI / m;
        var ab = a * b;
        var delt = new double[m, n];
        for (var x = 0; x < m; x++) {
          var delta = c * Math.Round(ab * x - b);
          for (var y = 0; y < n; y++) {
            d[y][x] *= Complex.FromPolarCoordinates(1, delt[x, y] = y * delta);
          }
        }
      }

      //-- step 8: fft on rows
      DoFFT2Rows(d);

      //-- step 9: masking with squared window
      {
        var wWin = m / 2;
        var x0Win = wWin / 2;
        var y0Win = n / 4;
        var yMax = n - 1;
        var y = 0;
        for (var yTo = y0Win; y < yTo; y++) {
          Array.Clear(d[y], x0Win, wWin);
          Array.Clear(d[yMax - y], x0Win, wWin);
        }

        for (var yTo = y0Win * 2; y < yTo; y++) {
          Array.Clear(d[y], 0, m);
          Array.Clear(d[yMax - y], 0, m);
        }
      }

      //-- step 10: IFFT on columns
      DoInverseFFT2Cols(d);

      //-- step 11: third skew
      {
        var a = 2.0 / (n - 1);
        var b = -tph / 2 * m;
        var c = 2 * Math.PI / m;
        var ab = a * b;

        var y = 0;
        foreach (var d1Row in d) {
          var delta = c * Math.Round(ab * y++ - b);
          for (var x = 0; x < d1Row.Length; x++) {
            d1Row[x] *= Complex.FromPolarCoordinates(1, x * delta);
          }
        }
      }

      //-- step 11b: IFFT on rows
      DoIFFT2Rows(d);

      //-- step 12: clipping/cropping
      {
        var result = new Complex2D(k, l);
        var dtR = result.Data;
        {
          var x0 = (m - k) / 2;
          for (int y = (n - l) / 2,
                   i = 0,
                   yTo = y + l;
               y < yTo;
               y++, i += k) {
            Array.Copy(d[y], x0, dtR, i, k);
          }
        }
        return FFT2(result, false);
      }
    }

    #endregion
  }
}
