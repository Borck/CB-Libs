using System;
using CB.System.Collections;



namespace CB.CV.Imaging.Filtering {
  public sealed class HammingWindow {
    private readonly LruMap<string, double[]> _winCache = new LruMap<string, double[]>(20);

    private readonly int _width;
    private readonly int _height;



    public HammingWindow(int width, int height) {
      _width = width;
      _height = height;
    }



    private static double[] CreateWindow1D(int n) {
      const double a = 25d / 46;
      const double b = 1 - a;
      var result = new double[n];
      var dPhi = MathCV.TWO_PI / (n - 1);
      for (var i = 0; i < n; i++) {
        result[i] = a - b * Math.Cos(i * dPhi);
      }

      return result;
    }



    public void Apply(Complex2D c) {
      var key = $"{_width}x{_height}";

      double[] window;
      if (!_winCache.TryGetValue(key, out window)) {
        var hWin = CreateWindow1D(_width);
        var vWin = _height == _width ? hWin : CreateWindow1D(_height);

        window = new double[_width * _height];
        for (int j = 0,
                 k = 0;
             j < _height;
             j++) {
          var fy = vWin[j];
          for (var i = 0; i < _width; i++, k++) {
            window[k] = fy * hWin[i];
          }
        }

        _winCache.Add(key, window);
      }

      var d = c.Data;

      // elementwise multiply
      if (c.Width == _width) { //non zero padded?
        for (var i = 0; i < window.Length; i++) {
          d[i] *= window[i];
        }

        return;
      }

      //zero padded
      var lf = c.Width - _width;
      for (int i = 0,
               j = 0;
           i < window.Length;
           j += lf) {
        for (var iLe = i + _width; i < iLe; i++, j++) {
          d[j] *= window[i];
        }
      }
    }



    public void Apply(ComplexImage image) {
      Apply(image.C0);
      Apply(image.C1);
      Apply(image.C2);
    }
  }
}
