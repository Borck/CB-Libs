using System;
using System.ComponentModel;
using JetBrains.Annotations;



namespace CB.CV.Imaging.Filtering {
  public static class WindowFactory {
    public enum Window {
      Rectangular, Gaussian
    }



    [NotNull]
    public static double[,] GetRectangular2D(int size) {
      var result = new double[size, size];
      for (var j = 0; j < size; j++) {
        for (var i = 0; i < size; i++) {
          result[i, j] = 1;
        }
      }

      return result;
    }



    //http://de.mathworks.com/help/signal/ref/gausswin.html
    [NotNull]
    public static double[,] GetGaussian2D(int size, double alpha = 2.5) {
      var gaus1D = new double[size];
      var mu = size / 2;
      var b = -alpha * alpha / (size * mu);
      for (var i = 0; i < size; i++) {
        gaus1D[i] = Math.Exp(b * MathCV.Pow2(i - mu));
      }

      var result = new double[size, size];
      for (var j = 0; j < size; j++) {
        var val = gaus1D[j];
        for (var i = 0; i < size; i++) {
          result[i, j] = gaus1D[i] * val;
        }
      }

      return result;
    }



    public static double[,] GetWindow(Window type, int size) {
      switch (type) {
        case Window.Rectangular:
          return GetRectangular2D(size);
        case Window.Gaussian:
          return GetGaussian2D(size);
        default:
          throw new InvalidEnumArgumentException(nameof(type));
      }
    }
  }
}
