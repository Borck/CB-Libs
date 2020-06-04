using System;
using System.Drawing;
using System.Numerics;



namespace CB.CV.Imaging {
  public static class Complex2DFactory {
    /// <summary>
    ///   Create a sin funktion with frequency f along the given direction
    /// </summary>
    /// <param name="f"></param>
    /// <param name="angle">direction of the sin in degrees</param>
    /// <returns></returns>
    public static Complex2D GenerateSin(int w, int h, double f, double angle) {
      var result = new Complex2D(w, h);
      var a = 2 * Math.PI * f / w * Math.Cos(angle = MathCV.ToRad(angle));
      var b = 2 * Math.PI * f / h * Math.Sin(angle);
      var d = result.Data;
      for (int y = 0,
               i = 0;
           y < h;
           y++) {
        var v = y * b;
        for (var x = 0; x < w; x++, i++) {
          d[i] = Math.Sin(v + x * a);
        }
      }

      return result;
    }



    public static Complex2D GenerateRectImpuls(Size imageSize, Size impuls) {
      var x0 = (imageSize.Width - impuls.Width) / 2;
      var y0 = (imageSize.Height - impuls.Height) / 2;
      var x1 = x0 + impuls.Width;
      var y1 = y0 + impuls.Height;
      return new Complex2D(
        imageSize.Width,
        imageSize.Height,
        (x, y) => x >= x0 && x < x1 && y >= y0 && y < y1 ? Complex.One : Complex.Zero
      );
    }



    public static Complex2D GenerateUniquePixels() {
      var result = new Complex2D(16, 16);
      var d = result.Data;
      for (var i = 0; i < d.Length; i++) {
        d[i] = i;
      }

      return result;
    }



    /// <summary>
    ///   Creates an image in frequenzy domain which one or more diagonal sinus
    /// </summary>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="cycles0">index where the first sinus begin, a higher value creates a sinus with a higher frequenz</param>
    /// <param name="numSin">number - 1 of the next higher sinus</param>
    /// <returns></returns>
    public static Complex2D CreateDiagFreqz(int w, int h, int cycles0, int numSin) {
      var l = w * h;
      var c = new Complex2D(w, h);
      var d = c.Data;

      d[0] = Complex.FromPolarCoordinates(.5 * l, 0);
      var mag = .25 * l / numSin;
      var cplx = Complex.FromPolarCoordinates(mag, 0);
      for (var i = 0; i < numSin; i++) {
        d[(cycles0 + i) * (w + 1)] = d[w * h - 1 - (h + 1) * (cycles0 + i - 1)] = cplx;
      }

      return c;
    }
  }
}
