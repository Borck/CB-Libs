using System;
using System.Numerics;



namespace CB.CV.Imaging {
  public class ComplexImage4D {
    public Complex[,][,] R { get; }
    public Complex[,][,] G { get; }
    public Complex[,][,] B { get; }

    public int D1Size { get; }
    public int D2Size { get; }
    public int D3Size { get; }
    public int D4Size { get; }



    public ComplexImage4D(int d1Size, int d2Size, int d3Size, int d4Size) {
      D1Size = d1Size;
      D2Size = d2Size;
      D3Size = d3Size;
      D4Size = d4Size;

      R = new Complex[D1Size, D2Size][,];
      G = new Complex[D1Size, D2Size][,];
      B = new Complex[D1Size, D2Size][,];

      ForEachColor(
        c => {
          for (var i = 0; i < D1Size; i++) {
            for (var j = 0; j < D2Size; j++) {
              c[i, j] = new Complex[D3Size, D4Size];
            }
          }
        }
      );
    }



    public ComplexImage4D(ComplexImage4D image) {
      D1Size = image.D1Size;
      D2Size = image.D2Size;
      D3Size = image.D3Size;
      D4Size = image.D4Size;

      R = new Complex[D1Size, D2Size][,];
      G = new Complex[D1Size, D2Size][,];
      B = new Complex[D1Size, D2Size][,];

      var srcR = image.R;
      var srcG = image.G;
      var srcB = image.B;

      var count = D3Size * D4Size;
      for (var i = 0; i < D1Size; i++) {
        for (var j = 0; j < D2Size; j++) {
          Array.Copy(srcR[i, j], R[i, j], count);
          Array.Copy(srcG[i, j], G[i, j], count);
          Array.Copy(srcB[i, j], B[i, j], count);
        }
      }
    }



    public void ForEachColor(Action<Complex[,][,]> action) {
      action(R);
      action(G);
      action(B);
    }



    public void ForEachColor(Action<Complex> action) {
      ForEachColor(
        c => {
          foreach (var d12 in c) {
            foreach (var d34 in d12) {
              action(d34);
            }
          }
        }
      );
    }



    public void ForEachColor(Action<Complex[]> action, int dimension) {
      if (dimension < 0 ||
          dimension > 3) {
        throw new ArgumentException("Dimension muss zwischen 0 und 3 liegen");
      }

      ForEachColor(
        c => {
          Complex[] value;
          int count;
          switch (dimension) {
            case 0:
              count = c.GetLength(0);
              value = new Complex[count];
              for (var j = 0; j < c.GetLength(1); j++) {
                for (var m = 0; m < c[0, 0].GetLength(0); m++) {
                  for (var n = 0; n < c[0, 0].GetLength(1); n++) {
                    for (var i = 0; i < count; i++) {
                      value[i] = c[i, j][m, n];
                    }

                    action(value);
                    for (var i = 0; i < count; i++) {
                      c[i, j][m, n] = value[i];
                    }
                  }
                }
              }

              break;

            case 1:
              count = c.GetLength(1);
              value = new Complex[count];
              for (var i = 0; i < c.GetLength(0); i++) {
                for (var m = 0; m < c[0, 0].GetLength(0); m++) {
                  for (var n = 0; n < c[0, 0].GetLength(1); n++) {
                    for (var j = 0; j < count; j++) {
                      value[j] = c[i, j][m, n];
                    }

                    action(value);
                    for (var j = 0; j < count; j++) {
                      c[i, j][m, n] = value[j];
                    }
                  }
                }
              }

              break;

            case 2:
              count = c[0, 0].GetLength(0);
              value = new Complex[count];
              foreach (var d12 in c) {
                for (var n = 0; n < d12.GetLength(0); n++) {
                  for (var m = 0; m < count; m++) {
                    value[m] = d12[m, n];
                  }

                  action(value);
                  for (var m = 0; m < count; m++) {
                    d12[m, n] = value[m];
                  }
                }
              }

              break;

            case 3:
              count = c[0, 0].GetLength(1);
              value = new Complex[count];
              foreach (var d12 in c) {
                for (var m = 0; m < d12.GetLength(0); m++) {
                  for (var n = 0; n < count; n++) {
                    value[n] = d12[m, n];
                  }

                  action(value);
                  for (var n = 0; n < count; n++) {
                    d12[m, n] = value[n];
                  }
                }
              }

              break;
          }
        }
      );
    }
  }
}
