using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.Serialization;
using Accord.Imaging;
using CB.CV.Imaging.Filtering;



namespace CB.CV.Imaging.LightField {
  [DataContract]
  public class HeterodyneLightField : ALightField {
    public int F1X { get; private set; }
    public int F1Y { get; private set; }

    /* Number of angular samples */

    public int NAngles { get; private set; }
    public int Phi1 { get; private set; }
    public int Phi2 { get; private set; }
    public int CAngles { get; private set; }

    private int _m;
    private int _n;

    private ComplexImage4D _fftLightField;

    private ComplexImage _fft;

    //TODO: check, if can be removed
    //private ComplexImage _ifft;



    public HeterodyneLightField(string path)
      : base(new[] {path}) { }



    internal HeterodyneLightField(Bitmap image)
      : base(image) { }



    protected void CheckImages() {
      if (Images == null) {
        throw new InvalidOperationException(nameof(Images) + " not loaded. Use " + nameof(Load) + " first.");
      }
    }



    protected override UnmanagedImage GetImage(int nr) {
      CheckImages();
      var fft = new ComplexImage(F1X, F1Y);
      var w = fft.Width;
      var data1 = fft.Channel0;
      var data2 = fft.Channel1;
      var data3 = fft.Channel2;
      var r = _fftLightField.R;
      var g = _fftLightField.G;
      var b = _fftLightField.B;

      var mHalf = _m >> 1;
      var nHalf = _n >> 1;
      var theta1ByM = (nr - CAngles) / _m;
      var theta2ByN = (nr - CAngles) / _n;

      // TODO: optimieren
      var dd = -mHalf * theta1ByM + CAngles;
      var ccc = -nHalf * theta2ByN + CAngles;
      for (var y = 0; y < F1Y; y++) {
        var cc = ccc;
        for (int x = 0,
                 i = y * w;
             x < F1X;
             x++, i++) {
          data1[i] = r[x, y][cc, dd];
          data2[i] = g[x, y][cc, dd];
          data3[i] = b[x, y][cc, dd];
          cc += theta2ByN;
        }

        dd += theta1ByM;
      }

      fft.InverseFFT2(false);

      var min = double.MaxValue;
      var max = double.MinValue;

      fft.ForEachChannel(
        c => {
          var val = c.Real;
          if (val < min) {
            min = val;
          }

          if (val > max) {
            max = val;
          }
        }
      );

      var invdist = byte.MaxValue / (max - min);

      data1 = fft.Channel0;
      data2 = fft.Channel1;
      data3 = fft.Channel2;

      var dst = UnmanagedImage.Create(F1X, F1Y, fft.PixelFormat);
      for (var y = 0; y < F1Y; y++) {
        for (int x = 0,
                 i = y * w;
             x < F1X;
             x++, i++) {
          dst.SetPixel(
            x,
            y,
            Color.FromArgb(
              (int)((data1[i].Real - min) * invdist),
              (int)((data2[i].Real - min) * invdist),
              (int)((data3[i].Real - min) * invdist)
            )
          );
        }
      }

      return dst;
    }



    public void Rearrange(int f1X, int f1Y, int phi1, int phi2, int nAngles) {
      CheckImages();
      /* Size of captured image */
      _m = MathCV.OddFloor(_fft.Width);
      _n = MathCV.OddFloor(_fft.Height);

      const double f1Xd = 237; // f1x <= 0 ? (double)m / (double)nAngles : f1x;
      const double f1Yd = 191; // f1y <= 0 ? (double)n / (double)nAngles : f1y;

      F1X = (int)f1Xd;
      F1Y = (int)f1Yd;

      NAngles = nAngles % 2 != 0 ? nAngles : NAngles > nAngles ? --nAngles : ++nAngles;
      CAngles = nAngles / 2;

      Phi1 = phi1;
      Phi2 = phi2;

      var phi1Rad = phi1 * Math.PI / 180;
      var phi2Rad = phi2 * Math.PI / 180;
      var k = Complex.Sqrt(-1);
      var kphi1Rad = phi1Rad * k;
      var kphi2Rad = phi2Rad * k;

      var f12Y = F1Y / 2;
      var f12X = F1X / 2;

      /* compute spectral tile centers, peak strengths und phase */
      var centX = new int[nAngles];
      var centY = new int[nAngles];
      //Complex[,] Mat = new Complex[nAngles, nAngles];

      var m = (_m >> 1) - CAngles * f1Xd;
      var n = (_n >> 1) - CAngles * f1Yd;

      for (var i = 0; i < nAngles; i++) {
        centX[i] = (int)(m + i * f1Xd);
        centY[i] = (int)(n + i * f1Yd);
      }

      var f = new ComplexImage(_fft);
      Fourier.FFTShift(f); /* fft zentrieren */

      /* Rearrange tiles of 2D fft into 4d planes to obtain fft of 4d lightfield */
      _fftLightField = new ComplexImage4D(F1X, F1Y, nAngles, nAngles);

      var data1 = f.Channel0;
      var data2 = f.Channel1;
      var data3 = f.Channel2;
      var w = f.Width;

      for (var y = 0; y < F1Y; y++) {
        var isYcAngles = y == CAngles;
        for (var x = 0; x < F1X; x++) {
          var rField = _fftLightField.R[x, y];
          var gField = _fftLightField.G[x, y];
          var bField = _fftLightField.B[x, y];

          var factor = 1.0 / (isYcAngles && x == CAngles ? 20 : 1);
          for (var j = 0; j < nAngles; j++) {
            var iCh0 = (centY[j] + y - f12Y) * w + x - f12X;
            for (var i = 0; i < nAngles; i++) {
              var iCh = iCh0 + centX[i];
              rField[i, j] = data1[iCh] * factor;
              gField[i, j] = data2[iCh] * factor;
              bField[i, j] = data3[iCh] * factor;
            }
          }
        }
      }

      for (var x = 0; x < F1X; x++) {
        for (var y = 0; y < F1Y; y++) {
          var rField = _fftLightField.R[x, y];
          var gField = _fftLightField.G[x, y];
          var bField = _fftLightField.B[x, y];

          for (var i = 0; i < nAngles; i++) {
            var cAngle1 = kphi1Rad * (i - CAngles);
            for (var j = 0; j < nAngles; j++) {
              var factor = Complex.Exp(cAngle1 + kphi2Rad * (j - CAngles));
              rField[i, j] *= factor;
              gField[i, j] *= factor;
              bField[i, j] *= factor;
            }
          }
        }
      }
    }



    public override void Load() {
      if (Fetched) {
        return;
      }

      var images = DoFetchImages(FileNames);
      _fft = Fourier.FFT2(Images[0], false);
      ViewColumns = 9;
      ViewRows = 9;
      Images = images;
      Rearrange(0, 0, 300, 150, 9);
    }
  }
}
