using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Accord;
using Accord.Imaging;



namespace CB.CV.Imaging.Filtering {
  /// <summary>
  ///   This class represents a 2D deblurring filter in frequency domain, which can be applied to an in image in spatial or
  ///   frequency domain.
  /// </summary>
  public class DeblurFilter : ComplexImage {
    private static readonly Complex Zero = Complex.Zero;

    public const int NUM_CHANNELS = 3;

    #region Constructors and Creators

    private DeblurFilter() { }



    public DeblurFilter(ComplexImage refFreqz, ComplexImage blurFreqz) {
      if (refFreqz == null ||
          blurFreqz == null) {
        throw new ArgumentNullException(blurFreqz == null ? "blurred" : "sharp");
      }

      if (!Equals(refFreqz.Width, refFreqz.Height, blurFreqz.Width, blurFreqz.Height)) {
        throw new ArgumentException("Bitmaps not in the same size.");
      }

      C0 = refFreqz.C0.DivideBy(blurFreqz.C0, Preferences.DIVIDE_BY_ZERO_EPSILON);
      C1 = refFreqz.C1.DivideBy(blurFreqz.C1, Preferences.DIVIDE_BY_ZERO_EPSILON);
      C2 = refFreqz.C2.DivideBy(blurFreqz.C2, Preferences.DIVIDE_BY_ZERO_EPSILON);
    }



    public DeblurFilter(UnmanagedImage reference, UnmanagedImage blurred)
      : this(Fourier.FFT2(reference), Fourier.FFT2(blurred)) { }



    /// <summary>
    ///   Creates a deblur filter from three channel in frequency domain.
    /// </summary>
    /// <param name="c0Freqz">channel 0 in frequency domain</param>
    /// <param name="c1Freqz">channel 1 in frequency domain</param>
    /// <param name="c2Freqz">channel 2 in frequency domain</param>
    /// <param name="normalize">
    ///   if true, each pixel c_n with 0 &lt; n &lt; N-1 will be divided by 1/sum|c_n|, c_0 will be set
    ///   to 1+j0
    /// </param>
    /// <returns></returns>
    public static DeblurFilter CreateFromFrequencies(Complex2D c0Freqz,
                                                     Complex2D c1Freqz,
                                                     Complex2D c2Freqz,
                                                     bool normalize = false) {
      var w = MathCV.Max(c0Freqz.Width, c1Freqz.Width, c2Freqz.Width);
      var h = MathCV.Max(c0Freqz.Height, c1Freqz.Height, c2Freqz.Height);

      var result = new DeblurFilter {
        C0 = c0Freqz.Width != w || c0Freqz.Height != h ? DoScale(c0Freqz, w, h) : new Complex2D(c0Freqz),
        C1 = c1Freqz.Width != w || c1Freqz.Height != h ? DoScale(c1Freqz, w, h) : new Complex2D(c1Freqz),
        C2 = c2Freqz.Width != w || c2Freqz.Height != h ? DoScale(c2Freqz, w, h) : new Complex2D(c2Freqz)
      };

      if (!normalize) {
        return result;
      }

      var n = w * h;
      var d0 = result.C0.Data;
      var d1 = result.C1.Data;
      var d2 = result.C2.Data;
      var norm0 = (n - 1) / (d0.SumAbs() - d0[0].Magnitude);
      var norm1 = (n - 1) / (d1.SumAbs() - d1[0].Magnitude);
      var norm2 = (n - 1) / (d2.SumAbs() - d2[0].Magnitude);
      d0[0] = d1[0] = d2[0] = Complex.One;
      for (var i = 1; i < n; i++) {
        d0[i] *= norm0;
        d1[i] *= norm1;
        d2[i] *= norm2;
      }

      return result;
    }



    public static DeblurFilter CreateFromFrequencies(ComplexImage reference, ComplexImage blurred) {
      return new DeblurFilter(reference, blurred);
    }

    #endregion

    #region transformation

    private static Complex2D DoScale(Complex2D channel, int dstWidth, int dstHeight) {
      var wFs = channel.Width;
      var hFs = channel.Height;

      var divX = dstWidth - wFs;
      var divY = dstHeight - hFs;
      Debug.Assert(divX != 0 || divY != 0, "No resize required.");
      var xOffset = divX / 2;
      var yOffset = divY / 2;
      var xEnd = xOffset + wFs;
      var yEnd = yOffset + hFs;

      if (divX < 0 ||
          divY < 0) {
        throw new Exception("New size is smaller than the input size");
      }

      var oldData = channel.Data;
      // TODO: correct zero padding -> FFT shifted??
      return new Complex2D(
        dstWidth,
        dstHeight,
        (i, j) => i >= xOffset && i < xEnd && j >= yOffset && j < yEnd
                    ? oldData[i - xOffset + (j - yOffset) * wFs]
                    : Zero
      );
    }



    /// <summary>
    ///   Rotation of the amplitudes by the given angle around (0,0). The Values should not be shifted (
    ///   <see>
    ///     <cref>Fourier.FFTShift</cref>
    ///   </see>
    ///   ) and where be bilinear interpolated.
    /// </summary>
    /// <param name="angle">angle in degree</param>
    /// <returns>rotated <see cref="DeblurFilter" /></returns>
    public DeblurFilter Rotate(double angle) {
      //      if (Math.Abs(angle)<Fourier.D_ANGLE)
      //        return this;

      var c0 = C0.RotateNearestNeighbor(angle);
      var c1 = C1.RotateNearestNeighbor(angle);
      var c2 = C2.RotateNearestNeighbor(angle);
      //c0.MultiplyWith(C0.Data[0].Magnitude/c0.Data[0].Magnitude);
      //c1.MultiplyWith(C1.Data[0].Magnitude/c1.Data[0].Magnitude);
      //c2.MultiplyWith(C2.Data[0].Magnitude/c2.Data[0].Magnitude);

      return new DeblurFilter {C0 = c0, C1 = c1, C2 = c2};
    }



    /// <summary>
    ///   Rotation of the amplitudes by the given angle around (0,0). The Values should not be shifted (
    ///   <see>
    ///     <cref>Fourier.FFTShift</cref>
    ///   </see>
    ///   ) and where be bilinear interpolated.
    /// </summary>
    /// <param name="angle">angle in degree</param>
    /// <returns>rotated <see cref="DeblurFilter" /></returns>
    public DeblurFilter RotateNearestNeighbor(double angle) {
      //      if (Math.Abs(angle)<Fourier.D_ANGLE)
      //        return this;

      var c0 = C0.RotateNearestNeighbor(angle);
      var c1 = C1.RotateNearestNeighbor(angle);
      var c2 = C2.RotateNearestNeighbor(angle);
      //c0.MultiplyWith(C0.Data[0].Magnitude/c0.Data[0].Magnitude);
      //c1.MultiplyWith(C1.Data[0].Magnitude/c1.Data[0].Magnitude);
      //c2.MultiplyWith(C2.Data[0].Magnitude/c2.Data[0].Magnitude);

      return new DeblurFilter {C0 = c0, C1 = c1, C2 = c2};
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Equals(int width1, int height1, int width2, int height2) {
      return width1 == width2 && height1 == height2;
    }

    #endregion

    #region Apply

    private void DoApply(UnmanagedImage image, UnmanagedImage output) {
      var fimg = Fourier.FFT2(image);
      if (Width != fimg.Width ||
          Height != fimg.Height) {
        throw new DimensionMismatchException();
      }

      fimg.C0.MultiplyWith(C0);
      fimg.C1.MultiplyWith(C1);
      fimg.C2.MultiplyWith(C2);
      output.Draw(fimg.InverseFFT2(false));
    }



    // TODO: refactor
    public void Apply(UnmanagedImage image, UnmanagedImage outImage) {
      ImageCheck.CheckSameSize(image, outImage);
      ImageCheck.CheckSameFormat(image, outImage);
      DoApply(image, outImage);
    }



    public void Apply(UnmanagedImage image) {
      DoApply(image, image);
    }



    public void Apply(ComplexImage imgFreqz) {
      imgFreqz.C0.MultiplyWith(C0);
      imgFreqz.C1.MultiplyWith(C1);
      imgFreqz.C2.MultiplyWith(C2);
    }



    public void Apply(ComplexImage inFreqz, ComplexImage outFreqz) {
      Debug.Assert(inFreqz.Width == outFreqz.Width);
      Debug.Assert(inFreqz.Height == outFreqz.Height);
      outFreqz.C0 = inFreqz.C0 * C0;
      outFreqz.C1 = inFreqz.C1 * C1;
      outFreqz.C2 = inFreqz.C2 * C2;
    }

    #endregion



    /// <summary>
    ///   Calculates the mean squared error of this and another DeblurFactor
    /// </summary>
    /// <param name="deblurFilter"></param>
    /// <returns></returns>
    public Complex GetMse(DeblurFilter deblurFilter) {
      double errorRe = 0;
      double errorIm = 0;

      //var r1 = Data.Channel1;
      //var g1 = Data.Channel2;
      //var b1 = Data.Channel3;
      //var r2 = blurFactor.Data.Channel1;
      //var g2 = blurFactor.Data.Channel2;
      //var b2 = blurFactor.Data.Channel3;

      //var ilength = r1.Width;
      //var jlength = r1.Width;

      Action<Complex, Complex, int, int> action = delegate(Complex c1, Complex c2, int i, int j) {
                                                    var divRe = c2.Real - c1.Real;
                                                    var divIm = c2.Imaginary - c1.Imaginary;
                                                    errorRe += divRe * divRe;
                                                    errorIm += divIm * divRe;
                                                  };

      C0.FoldWith(deblurFilter.C0, action);
      C1.FoldWith(deblurFilter.C1, action);
      C2.FoldWith(deblurFilter.C2, action);

      //for (var j = 0; j < jlength; j++)
      //  for (var i = 0; i < ilength; i++)
      //  {
      //    var divR = r2[i][j].Real - r1[i][j].Real;
      //    var divG = g2[i][j].Real - g1[i][j].Real;
      //    var divB = b2[i][j].Real - b1[i][j].Real;
      //    var divRi = r2[i][j].Imaginary - r1[i][j].Imaginary;
      //    var divGi = g2[i][j].Imaginary - g1[i][j].Imaginary;
      //    var divBi = b2[i][j].Imaginary - b1[i][j].Imaginary;

      //    errorRe += divR*divR;
      //    errorRe += divG*divG;
      //    errorRe += divB*divB;
      //    errorIm += divRi*divRi;
      //    errorIm += divGi*divGi;
      //    errorIm += divBi*divBi;
      //  }
      return new Complex(errorRe, errorIm);
    }
  }
}
