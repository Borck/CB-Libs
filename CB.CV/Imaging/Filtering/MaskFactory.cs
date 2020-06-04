using System;



namespace CB.CV.Imaging.Filtering {
  public static class MaskFactory {
    public static void Sincd2Periodic(double[][] destinationArray, double x0, double y0) {
      var h = destinationArray.Length;
      var w = h > 0 ? destinationArray[0].Length : 0;

      // shift to array center
      var xx0 = (int)Math.Round(x0) - w / 2;
      var y = (int)Math.Round(y0) - h / 2;

      for (var yTo = y + h; y < yTo; y++) {
        var line = destinationArray[(y + h) % h];
        var v = (y - y0);
        var vv = v != .0 ? Math.Sin(v) / (h * Math.Sin(v / h)) : 1.0;
        var vv2 = vv / w;
        for (int x = xx0,
                 xTo = x + w;
             x < xTo;
             x++) {
          var u = (x - x0);
          line[(x + w) % w] = u != .0 ? vv2 * Math.Sin(u) / Math.Sin(u / w) : vv;
        }
      }
    }



    public static void Sincd(double[] destinationArray, double x0) {
      var n = destinationArray.Length;
      var v = -x0;
      for (var x = 0; x < n; x++, v++)
        //destinationArray[x] = v != .0 ? Math.Sin(v)/(n*Math.Sin(v/n)) : 1;
      {
        destinationArray[x] = v != .0 ? Math.Sin(Math.PI * v) / (n * Math.Sin(Math.PI * v / n)) : 1;
      }

      //destinationArray[x]=v!=.0 ? Math.Sin(Math.PI*v)/(Math.PI*v) : 1;
    }



    public static void Sincd2(double[][] destinationArray, double x0, double y0) {
      var h = destinationArray.Length;
      var w = h > 0 ? destinationArray[0].Length : 0;

      var sincX = new double[w];
      var sincY = new double[h];
      Sincd(sincX, x0);
      Sincd(sincY, y0);
      for (var j = 0; j < h; j++) {
        var line = destinationArray[j];
        var y = sincY[j];
        for (var i = 0; i < w; i++) {
          line[i] = y * sincX[i];
        }
      }
    }
  }
}
