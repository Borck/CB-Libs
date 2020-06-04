using System;
using System.Drawing;



namespace CB.CV.Imaging.Filtering {
  public class SeparatsbleFIR2 {
    private double[] _xCoefficients;
    private double[] _yCoefficients;

    #region constructors

    public SeparatsbleFIR2(double[] xCoefficients, double[] yCoefficients) {
      if (xCoefficients == null ||
          yCoefficients == null ||
          xCoefficients.Length == 0 ||
          yCoefficients.Length == 0) {
        throw new ArgumentException("coefficient");
      }

      _xCoefficients = xCoefficients;
      _yCoefficients = yCoefficients;
    }



    public SeparatsbleFIR2(Size size, Func<int, double> funcX, Func<int, double> funcY) {
      var w = size.Width;
      var h = size.Height;
      if (w == 0 ||
          h == 0 ||
          funcX == null ||
          funcY == null) {
        throw new ArgumentException("");
      }

      var xOffset = size.Width / 2;
      var yOffset = size.Height / 2;

      _xCoefficients = new double[w];
      for (var x = 0; x < w; x++) {
        _xCoefficients[x] = funcX(x - xOffset);
      }

      _yCoefficients = new double[h];
      for (var y = 0; y < h; y++) {
        _yCoefficients[y] = funcY(y - yOffset);
      }
    }



    public SeparatsbleFIR2(Size size, Func<int, double> func)
      : this(size, func, func) { }

    #endregion

    #region create filter

    public static SeparatsbleFIR2 CreateGaussianKernel(int t, Size size) {
      if (t == 0) {
        return new SeparatsbleFIR2(size, x => x == 0 ? 1 : 0);
      }

      var negOneBy2T = -.5f / t;
      var constFac = -negOneBy2T / Math.PI;
      return new SeparatsbleFIR2(size, x => constFac * Math.Exp(x * x * negOneBy2T));
      // TODO: is normalized?
    }

    #endregion

    #region convolution

    public byte[,] Convolute(byte[,] f) {
      if (f == null) {
        return null;
      }

      var w = f.GetLength(0);
      var h = f.GetLength(1);
      if (w == 0 ||
          h == 0) {
        return f;
      }

      var result = new byte[w, h];

      var yOffset = _yCoefficients.Length / 2;
      throw new NotImplementedException();
    }



    private static byte[,] ConvoluteHorizontal(byte[,] f, float[] g) {
      var w = f.GetLength(0);
      var h = f.GetLength(1);
      var lg = g.Length;
      var offset = g.Length / 2;

      var result = new byte[w, h];

      // Middle convolution with no filter cut
      var xf = offset;
      var xMax = w - lg + offset;
      for (; xf < xMax; xf++) {
        for (var yf = 0; yf < h; yf++) {
          var value = .0;
          var xfStart = xf - offset;
          for (var xg = 0; xg < lg; xg++) {
            value += f[xfStart + xg, yf] * g[lg - xg - 1];
          }

          result[xf, yf] = (byte)value;
        }
      }

      var gBuffer = new byte[g.Length];
      xf = offset;

      return result;
    }

    #endregion
  }
}
