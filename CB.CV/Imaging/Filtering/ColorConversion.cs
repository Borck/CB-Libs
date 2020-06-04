using System;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Accord.Imaging;
using JetBrains.Annotations;



namespace CB.CV.Imaging.Filtering {
  public class ColorConversion {
    #region constants

    // references for white point D65
    private const double XN_X = 0.950456;
    private const double XN_Y = 1;
    private const double XN_Z = 1.08883;

    private const short RGB_R = RGB.R;
    private const short RGB_G = RGB.G;
    private const short RGB_B = RGB.B;
    private const short RGB_A = RGB.A;
    private const short RGB_R_CL = 0;
    private const short RGB_G_CL = 1;
    private const short RGB_B_CL = 2;
    private const short RGB_A_CL = 3;
    private const short LAB_L = RGB.R;
    private const short LAB_A = RGB.G;
    private const short LAB_B = RGB.B;

    #endregion

    #region static fields

    private static readonly ImageCheck SupportedImages = new ImageCheck {
      PixelFormat.Format24bppRgb, PixelFormat.Format32bppArgb
    };

    #endregion

    #region init

    /// <summary>
    ///   Converts a rgb image to an rgba image.
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static unsafe float[] ToFloats(UnmanagedImage image) {
      const float alpha = 255;
      const int pwR = 4;
      var pwI = image.GetPixelFormatSizeInBytes();

      var w = image.Width;
      var h = image.Height;
      var s = w * 4;
      var result = new float[s * h];

      var dtI = (byte*)image.ImageData;

      var wpR = w * pwR;
      var paddI = image.Stride - w * pwI;

      var i = 0;
      switch (image.PixelFormat) {
        case PixelFormat.Format32bppRgb:
        case PixelFormat.Format24bppRgb:
          for (var dtrTo = h * s; i < dtrTo; dtI += paddI) {
            for (var dtrLineTo = i + wpR; i < dtrLineTo; dtI += pwI, i += pwR) {
              result[i + RGB_R_CL] = dtI[RGB_R];
              result[i + RGB_G_CL] = dtI[RGB_G];
              result[i + RGB_B_CL] = dtI[RGB_B];
              result[i + RGB_A_CL] = alpha;
            }
          }

          break;
        case PixelFormat.Format32bppArgb:
          for (var dtrTo = h * s; i < dtrTo; dtI += paddI) {
            for (var dtrLineTo = i + wpR; i < dtrLineTo; dtI += pwI, i += pwR) {
              result[i + RGB_R_CL] = dtI[RGB_R];
              result[i + RGB_G_CL] = dtI[RGB_G];
              result[i + RGB_B_CL] = dtI[RGB_B];
              result[i + RGB_A_CL] = dtI[RGB_A];
            }
          }

          break;
        default:
          throw new BadImageFormatException(nameof(image));
      }

      return result;
    }



    /// <summary>
    ///   Converts a rgb image to an rgba image.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="outImage"></param>
    /// <returns></returns>
    public static unsafe void CreateFromFloats(float[] data, UnmanagedImage outImage) {
      const int pwI = 4;
      var w = outImage.Width;
      var h = outImage.Height;
      var sDt = w * 4;

      var pwR = outImage.GetPixelFormatSizeInBytes();
      var dtR = (byte*)outImage.ImageData;

      var wpI = w * pwI;
      var paddR = outImage.Stride - w * pwR;

      if (pwR == 4) // apply alpha to?
      {
        for (int i = 0,
                 iTo = h * sDt;
             i < iTo;
             dtR += paddR) {
          for (var iToLine = i + wpI; i < iToLine; dtR += pwR, i += pwI) {
            dtR[RGB_R] = data[i + RGB_R_CL].ClampToByte();
            dtR[RGB_G] = data[i + RGB_G_CL].ClampToByte();
            dtR[RGB_B] = data[i + RGB_B_CL].ClampToByte();
            dtR[RGB_A] = data[i + RGB_A_CL].ClampToByte();
          }
        }
      } else {
        for (int i = 0,
                 iTo = h * sDt;
             i < iTo;
             dtR += paddR) {
          for (var iToLine = i + wpI; i < iToLine; dtR += pwR, i += pwI) {
            dtR[RGB_R] = data[i + RGB_R_CL].ClampToByte();
            dtR[RGB_G] = data[i + RGB_G_CL].ClampToByte();
            dtR[RGB_B] = data[i + RGB_B_CL].ClampToByte();
          }
        }
      }
    }

    #endregion

    #region rgb <-> xyz

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Rgb2Xyz(byte r, byte g, byte b, out double x, out double y, out double z) {
      const double crx = 0.412453 / XN_X;
      const double cgx = 0.357580 / XN_X;
      const double cbx = 0.180423 / XN_X;
      const double cry = 0.212671 / XN_Y;
      const double cgy = 0.715160 / XN_Y;
      const double cby = 0.072169 / XN_Y;
      const double crz = 0.019334 / XN_Z;
      const double cgz = 0.119193 / XN_Z;
      const double cbz = 0.950227 / XN_Z;

      const double invNorm = 1d / byte.MaxValue;

      // RGB -> XYZ:
      var rn = r * invNorm;
      var gn = g * invNorm;
      var bn = b * invNorm;

      x = crx * rn + cgx * gn + cbx * bn;
      y = cry * rn + cgy * gn + cby * bn;
      z = crz * rn + cgz * gn + cbz * bn;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Xyz2Rgb(double x, double y, double z, out byte r, out byte g, out byte b) {
      const double cxr = 3.240479 * XN_X;
      const double cyr = -1.537150 * XN_Y;
      const double czr = -0.498535 * XN_Z;
      const double cxg = -0.969256 * XN_X;
      const double cyg = 1.875992 * XN_Y;
      const double czg = 0.041556 * XN_Z;
      const double cxb = 0.055648 * XN_X;
      const double cyb = -0.204043 * XN_Y;
      const double czb = 1.057311 * XN_Z;

      r = (cxr * x + cyr * y + czr * z).ToBase256();
      g = (cxg * x + cyg * y + czg * z).ToBase256();
      b = (cxb * x + cyb * y + czb * z).ToBase256();
    }

    #endregion

    #region rgb -> cie lab

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Rgb2Lab(byte rgbR, byte rgbG, byte rgbB, out byte labL, out byte labA, out byte labB) {
      double x,
             y,
             z;
      Rgb2Xyz(rgbR, rgbG, rgbB, out x, out y, out z);

      const double t = 0.008856; // threshold
      const double pot = 1d / 3;
      const double off = 16d / 166;

      var y3 = Math.Pow(y, pot);

      // XYZ -> Lab:
      var fX = x > t ? Math.Pow(x, pot) : 7.787 * x + off;
      var fY = y > t ? y3 : 7.787 * y + 16d / 116d;
      var fZ = z > t ? Math.Pow(z, pot) : 7.787 * y + off;

      // RGB in [0,1] -> RGB in [0,255]. Dabei auch die Wert in dieses
      // Intervall zwingen:

      const double la = 116 * 2.55;
      const double lb = -16 * 2.55;
      const double lc = 903.3 * 2.55;
      const double aa = 500 * 1.02;
      const double ab = 150 * 1.02;
      const double ba = 200 * 1.02;
      const double bb = 100 * 1.02;

      labL = (y > t ? la * y3 + lb : lc * y).ClampToByte();
      labA = (aa * (fX - fY) + ab).ClampToByte();
      labB = (ba * (fY - fZ) + bb).ClampToByte();
    }



    public static void Rgb2Lab([NotNull] UnmanagedImage srcImage, [NotNull] UnmanagedImage dstImage) {
      SupportedImages.Check(ImageCheckOptions.ALL, srcImage, dstImage);
      DoRgb2Lab(srcImage, dstImage);
    }



    private static unsafe void DoRgb2Lab(UnmanagedImage srcImage, UnmanagedImage dstImage) {
      var pb = srcImage.GetPixelFormatSizeInBytes();
      var hasAlpha = pb == 4;

      var srcPtr = (byte*)srcImage.ImageData;
      var dstPtr = (byte*)dstImage.ImageData;

      var line = srcImage.Width * pb;
      var padd = srcImage.Stride - line;

      if (hasAlpha) {
        for (var srcPtrTo = srcPtr + srcImage.Stride * srcImage.Height;
             srcPtr < srcPtrTo;
             srcPtr += padd, dstPtr += padd) {
          for (var srcLineTo = srcPtr + line; srcPtr < srcLineTo; srcPtr += pb, dstPtr += pb) {
            Rgb2Lab(
              srcPtr[RGB_R],
              srcPtr[RGB_G],
              srcPtr[RGB_B],
              out dstPtr[LAB_L],
              out dstPtr[LAB_A],
              out dstPtr[LAB_B]
            );
            dstPtr[RGB_A] = srcPtr[RGB_A];
          }
        }
      } else {
        for (var srcPtrTo = srcPtr + srcImage.Stride * srcImage.Height;
             srcPtr < srcPtrTo;
             srcPtr += padd, dstPtr += padd) {
          for (var srcLineTo = srcPtr + line; srcPtr < srcLineTo; srcPtr += pb, dstPtr += pb) {
            Rgb2Lab(
              srcPtr[RGB_R],
              srcPtr[RGB_G],
              srcPtr[RGB_B],
              out dstPtr[LAB_L],
              out dstPtr[LAB_A],
              out dstPtr[LAB_B]
            );
          }
        }
      }
    }



    public static UnmanagedImage Rgb2Lab([NotNull] UnmanagedImage image, bool useCopy = true) {
      SupportedImages.CheckFormat(image);
      var result = useCopy ? image.Clone() : image;
      DoRgb2Lab(image, result);
      return result;
    }

    #endregion

    #region rgb <- cie lab

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Lab2Rgb(byte srcL, byte srcA, byte srcB, out byte dstR, out byte dstG, out byte dstB) {
      const double t1 = 0.008856;
      const double t2 = 0.206893;

      const double la = 1 / 2.55;
      const double aa = 1 / 1.02;
      const double ba = 1 / 1.02;

      var l = srcL * la;
      var a = srcA * aa - 150;
      var b = srcB * ba - 100;

      const double sixteenBy116 = 16d / 116;
      const double oneBy116 = 1d / 116;

      const double ya = 1 / 903.3;

      // Y calculation:
      var fY = l * oneBy116 + sixteenBy116;
      var fY3 = fY * fY * fY;
      var y = fY3 > t1 ? fY3 : l * ya;

      // fY - modified version of y:
      fY = fY3 > t1 ? fY : 7.787 * fY3 + sixteenBy116;

      const double oneBy7787 = 1 / 7.787;

      const double fXa = 1d / 500;
      const double fZa = 1d / 200;

      // X calculation:
      var fX = a * fXa + fY;
      var x = fX > t2 ? fX * fX * fX : (fX - sixteenBy116) * oneBy7787;

      // Z calculation:
      var fZ = fY - b * fZa;
      var z = fZ > t2 ? fZ * fZ * fZ : (fZ - sixteenBy116) * oneBy7787;

      // XYZ -> RGB
      Xyz2Rgb(x, y, z, out dstR, out dstG, out dstB);
    }



    public static unsafe void Lab2Rgb(UnmanagedImage srcImage, UnmanagedImage dstImage) {
      if (srcImage == null ||
          dstImage == null) {
        throw new NullReferenceException(srcImage == null ? nameof(srcImage) : nameof(dstImage));
      }

      var bytesPerPixel = srcImage.GetPixelFormatSizeInBytes();
      var hasAlpha = bytesPerPixel == 4;
      if (bytesPerPixel != 3 &&
          !hasAlpha) {
        throw new NotSupportedException(nameof(bytesPerPixel) + " != 4");
      }

      var srcPtr = (byte*)srcImage.ImageData;
      var dstPtr = (byte*)dstImage.ImageData;

      var line = srcImage.Width * bytesPerPixel;
      var padd = srcImage.Stride - line;

      if (hasAlpha) {
        for (var srcPtrTo = srcPtr + srcImage.Stride * srcImage.Height;
             srcPtr < srcPtrTo;
             srcPtr += padd, dstPtr += padd) {
          for (var srcLineTo = srcPtr + line; srcPtr < srcLineTo; srcPtr += bytesPerPixel, dstPtr += bytesPerPixel) {
            Lab2Rgb(
              srcPtr[LAB_L],
              srcPtr[LAB_A],
              srcPtr[LAB_B],
              out dstPtr[RGB_R],
              out dstPtr[RGB_G],
              out dstPtr[RGB_B]
            );
            dstPtr[RGB_A] = srcPtr[RGB_A];
          }
        }
      } else {
        for (var srcPtrTo = srcPtr + srcImage.Stride * srcImage.Height;
             srcPtr < srcPtrTo;
             srcPtr += padd, dstPtr += padd) {
          for (var srcLineTo = srcPtr + line; srcPtr < srcLineTo; srcPtr += bytesPerPixel, dstPtr += bytesPerPixel) {
            Lab2Rgb(
              srcPtr[LAB_L],
              srcPtr[LAB_A],
              srcPtr[LAB_B],
              out dstPtr[RGB_R],
              out dstPtr[RGB_G],
              out dstPtr[RGB_B]
            );
          }
        }
      }
    }



    public static UnmanagedImage Lab2Rgb([NotNull] UnmanagedImage image, bool useCopy = true) {
      SupportedImages.CheckFormat(image);
      var result = useCopy ? image.Clone() : image;
      Lab2Rgb(image, result);
      return result;
    }

    #endregion
  }
}
