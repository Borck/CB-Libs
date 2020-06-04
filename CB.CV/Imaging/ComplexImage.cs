using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;
using Accord;
using JetBrains.Annotations;



namespace CB.CV.Imaging {
  public class ComplexImage {
    #region constants

    public const int MAX_MAGNITUDE = Complex2D.MAX_MAGNITUDE;

    #endregion

    #region Field

    public static readonly ImageCheck Compatible = new ImageCheck {
      PixelFormat.Format24bppRgb, PixelFormat.Format32bppArgb
    };

    public PixelFormat PixelFormat => PixelFormat.Format24bppRgb;

    public int Width => C0.Width;
    public int Height => C0.Height;
    public int Length => C0.Width * C0.Height;

    public Size Size => new Size(Width, Height);

    internal Complex2D C0;
    internal Complex2D C1;
    internal Complex2D C2;

    public Complex[] Channel0 => C0.Data;
    public Complex[] Channel1 => C1.Data;
    public Complex[] Channel2 => C2.Data;

    #endregion

    #region constructors

    protected ComplexImage() { }



    public ComplexImage(int width, int height) {
      C0 = new Complex2D(width, height);
      C1 = new Complex2D(width, height);
      C2 = new Complex2D(width, height);
    }



    public ComplexImage(Size size) {
      C0 = new Complex2D(size.Width, size.Height);
      C1 = new Complex2D(size.Width, size.Height);
      C2 = new Complex2D(size.Width, size.Height);
    }



    public ComplexImage(ComplexImage image) {
      C0 = new Complex2D(image.C0);
      C1 = new Complex2D(image.C1);
      C2 = new Complex2D(image.C2);
    }



    public ComplexImage(Complex2D channel0, Complex2D channel1, Complex2D channel2, bool copy = true) {
      if (channel0.Width != channel1.Width ||
          channel0.Width != channel2.Width ||
          channel0.Height != channel1.Height ||
          channel0.Height != channel2.Height) {
        throw new Exception("Not in the same size");
      }

      C0 = copy ? new Complex2D(channel0) : channel0;
      C1 = copy ? new Complex2D(channel1) : channel1;
      C2 = copy ? new Complex2D(channel2) : channel2;
    }



    public ComplexImage(Complex2D channel) {
      C0 = new Complex2D(channel);
      C1 = new Complex2D(channel);
      C2 = new Complex2D(channel);
    }



    public ComplexImage(Complex2D channel, bool copy = true)
      : this(copy ? (channel = new Complex2D(channel)) : channel, channel, channel, false) { }

    #endregion

    #region

    public void CopyTo(ComplexImage destination) {
      if (Width != destination.Width ||
          Height != destination.Height) {
        throw new ArgumentException(
          @"different " + (Width != destination.Width ? "width" : "height"),
          nameof(destination)
        );
      }

      var n = Width * Height;
      Array.Copy(Channel0, destination.Channel0, n);
      Array.Copy(Channel1, destination.Channel1, n);
      Array.Copy(Channel2, destination.Channel2, n);
    }

    #endregion

    #region foreach

    public void ForEachChannel(Action<Complex[]> action) {
      action(C0.Data);
      action(C1.Data);
      action(C2.Data);
    }



    public void ForEachChannel(Action<Complex> action) {
      C0.ForEachPixel(action);
      C1.ForEachPixel(action);
      C2.ForEachPixel(action);
    }



    public void ForEachRow(Action<Complex[]> action) {
      C0.ForEachRow(action);
      C1.ForEachRow(action);
      C2.ForEachRow(action);
    }



    public void ForEachRow(Action<Complex[]> action, ComplexImage output) {
      C0.ForEachRow(action, output.C0);
      C1.ForEachRow(action, output.C1);
      C2.ForEachRow(action, output.C2);
    }



    public void ForEachColumn(Action<Complex[]> action) {
      C0.ForEachColumn(action);
      C1.ForEachColumn(action);
      C2.ForEachColumn(action);
    }



    public void ForEachColumn(Action<Complex[]> action, ComplexImage output) {
      C0.ForEachColumn(action, output.C0);
      C1.ForEachColumn(action, output.C1);
      C2.ForEachColumn(action, output.C2);
    }



    public void ForEachPixel(Action<int, int, Complex, Complex, Complex> action) {
      var c1Data = C0.Data;
      var c2Data = C1.Data;
      var c3Data = C2.Data;

      var w = Width;
      var n = Length;
      for (var i = 0; i < n; i++) {
        action(i % w, i / w, c1Data[i], c2Data[i], c3Data[i]);
      }
    }



    public Complex2D CopyChannel(int channel) {
      switch (channel) {
        case 0:
          return new Complex2D(C0);
        case 1:
          return new Complex2D(C1);
        case 2:
          return new Complex2D(C2);
        default:
          throw new ArgumentException(channel + " is not allowed.");
      }
    }



    public double SumAbs(int channel) {
      switch (channel) {
        case 0:
          return C0.Data.SumAbs();
        case 1:
          return C1.Data.SumAbs();
        case 2:
          return C2.Data.SumAbs();
        default:
          throw new ArgumentException(channel + " is not allowed.");
      }
    }

    #endregion



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CheckComparable([NotNull] ComplexImage image1, [NotNull] ComplexImage image2) {
      if (image1.Width != image2.Width ||
          image1.Height != image2.Height) {
        throw new DimensionMismatchException();
      }
    }
  }
}
