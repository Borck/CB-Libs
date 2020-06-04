using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using CB.System.Runtime.InteropServices;
using JetBrains.Annotations;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;



namespace CB.CV.Imaging {
  public static class UnmanagedImageX {
    #region properties

    /// <summary>
    ///   Returns the size of a pixel in bytes
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetBytesPerPixel(this PixelFormat format) {
      return Image.GetPixelFormatSize(format) / 8;
    }



    /// <summary>
    ///   Returns the size of a pixel in bytes
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete("Use native extension: GetPixelFormatSizeInBytes()")]
    public static int GetBytesPerPixel(this UnmanagedImage image) {
      return image.GetPixelFormatSizeInBytes();
    }



    /// <summary>
    ///   Returns the effective size of the image in bytes
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNumBytes(this UnmanagedImage image) {
      return image.Width * image.GetPixelFormatSizeInBytes() * image.Height;
    }



    /// <summary>
    ///   Returns the number of bytes used for zero padding at the end of each row
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetLineForward(this UnmanagedImage image) {
      return image.Stride - image.Width * image.GetPixelFormatSizeInBytes();
    }



    /// <summary>
    ///   The Length of the byte array
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Length(this UnmanagedImage image) {
      return image.Stride * image.Height;
    }

    #endregion

    #region foreach

    public static void ForEachPixel(this UnmanagedImage image, Func<Color, Color> modify) {
      var w = image.Width;
      var h = image.Height;
      for (var y = 0; y < h; y++) {
        for (var x = 0; x < w; x++) {
          image.SetPixel(x, y, modify(image.GetPixel(x, y)));
        }
      }
    }



    public static void ForEachPixel(this UnmanagedImage image, Action<int, Color> generate) {
      var w = image.Width;
      var h = image.Height;
      for (var y = 0; y < h; y++) {
        var offset = y * w;
        for (var x = 0; x < w; x++) {
          generate(offset + x, image.GetPixel(x, y));
        }
      }
    }

    #endregion

    #region copy/clone

    public static void CopyTo(this UnmanagedImage srcImage, ref byte[] dstArray) {
      var wb = srcImage.Width * srcImage.GetPixelFormatSizeInBytes();
      var s = srcImage.Stride;
      var nj = wb * srcImage.Height;
      if (dstArray == null ||
          dstArray.Length < nj) {
        dstArray = new byte[nj];
      }

      int i = 0,
          j = 0;
      if (nj >= dstArray.Length) {
        nj -= wb;
        for (; j < nj; i += s, j += wb) {
          Marshal.Copy(srcImage.ImageData + i, dstArray, j, wb);
        }

        Marshal.Copy(srcImage.ImageData + i, dstArray, j, dstArray.Length - j);
      } else {
        var ni = s * (srcImage.Height - 1);
        for (; i < ni; i += s, j += wb) {
          Marshal.Copy(srcImage.ImageData + i, dstArray, j, wb);
        }

        Marshal.Copy(srcImage.ImageData + i, dstArray, j, i = ni + wb - i);
        Array.Clear(dstArray, j += i, dstArray.Length - j);
      }
    }



    public static unsafe void CopyTo(this UnmanagedImage image, float[] dstArray, float weight) {
      var src = (byte*)image.ImageData;
      var length = image.Length();
      for (var i = 0; i < length; i++) {
        dstArray[i] = src[i] * weight;
      }
    }



    public static unsafe void CopyFrom(this UnmanagedImage image, float[] src) {
      var ptr = (byte*)image.ImageData;
      var l = image.Width + image.GetPixelFormatSizeInBytes();
      var n = Math.Min(l * image.Height, src.Length);
      var padd = image.Stride - l;
      for (var i = 0; i < n;) {
        ptr[i++] = src[i].ToBase256();
        if (i % l == 0) // shift pointer by it's padding width at the end of line
        {
          ptr += padd;
        }
      }
    }



    public static unsafe void CopyFrom(this UnmanagedImage image, float[] src, float factor) {
      var ptr = (byte*)image.ImageData;
      var l = image.Width + image.GetPixelFormatSizeInBytes();
      var n = Math.Min(l * image.Height, src.Length);
      var padd = image.Stride - l;
      for (var i = 0; i < n;) {
        ptr[i++] = (src[i] * factor).ToBase256();
        if (i % l == 0) // shift pointer by it's padding width at the end of line
        {
          ptr += padd;
        }
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnmanagedImage Clone(this UnmanagedImage image) {
      return new UnmanagedImage(image.ImageData, image.Width, image.Height, image.Stride, image.PixelFormat);
    }

    #endregion

    #region implicit methodes

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Save(this UnmanagedImage thisImage, string path) {
      using (var img = thisImage.ToManagedImage()) {
        img.Save(path);
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitmap ToManagedAndDisposeThis(this UnmanagedImage thisImage) {
      var result = thisImage.ToManagedImage();
      thisImage.Dispose();
      return result;
    }

    #endregion

    #region transformation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnmanagedImage Resize(this UnmanagedImage thisImage, int width, int height) {
      var bmp1 = thisImage.ToManagedImage();
      var bmp2 = new Bitmap(bmp1, width, height);
      bmp1.Dispose();
      var result = UnmanagedImage.FromManagedImage(bmp2);
      bmp2.Dispose();
      return result;
    }



    public static UnmanagedImage Resize(this UnmanagedImage thisImage, int width, int height, bool disposeInput) {
      if (!disposeInput) {
        return Resize(thisImage, width, height);
      }

      var result = Resize(thisImage, width, height);
      thisImage.Dispose();
      return result;
    }

    #endregion

    #region arithmetic

    public static unsafe void AggregateTo(this UnmanagedImage image, float[] dstArray, float weight) {
      var src = (byte*)image.ImageData;
      var length = image.Length();
      for (var i = 0; i < length; i++) {
        dstArray[i] += src[i] * weight;
      }
    }



    public static unsafe int[] Subtract(this UnmanagedImage image1, [NotNull] UnmanagedImage image2) {
      ImageCheck.CheckSameSize(image1, image2);

      var w = image1.Width;
      var h = image1.Height;
      var s = image1.Stride;
      var dt1 = (byte*)image1.ImageData;
      var dt2 = (byte*)image2.ImageData;

      var dt1To = dt1 + h * s;
      var wb = w * image1.GetPixelFormatSizeInBytes();
      var result = new int[wb * h];
      var j = 0;
      for (; dt1 < dt1To; dt1 += s, dt2 += s) {
        for (var i = 0; i < wb; i++) {
          result[j++] = dt1[i] - dt2[i];
        }
      }

      return result;
    }

    #endregion

    #region subimage extraction

    public static UnmanagedImage GetSubImage(this UnmanagedImage image, int x, int y, int width, int height) {
      return new Crop(new Rectangle(x, y, width, height)).Apply(image);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnmanagedImage GetSubImage(this UnmanagedImage image, Rectangle bounds) {
      return new Crop(bounds).Apply(image);
    }



    public static unsafe ComplexImage GetSubComplexImage(this UnmanagedImage image,
                                                         int x,
                                                         int y,
                                                         int width,
                                                         int height,
                                                         bool zeroPadding = true) {
      const float norm = ComplexImage.MAX_MAGNITUDE / (float)byte.MaxValue;

      if (x < 0 ||
          y < 0 ||
          image.Width < width ||
          image.Height < height) {
        throw new DimensionMismatchException();
      }

      var result = zeroPadding
                     ? new ComplexImage((int)MathCV.UpperPow2((uint)width), (int)MathCV.UpperPow2((uint)height))
                     : new ComplexImage(width, height);
      var c0Data = result.Channel0;
      var c1Data = result.Channel1;
      var c2Data = result.Channel2;

      ComplexImage.Compatible.CheckFormat(image, out var pb);
      var srcDataPtr = (byte*)(image.ImageData + y * image.Stride + x * pb);
      var srcPadd = image.Stride - width * pb;
      var dstPadd = result.Width - width;

      for (int i = 0,
               n = result.Width * height;
           i < n;
           i += dstPadd, srcDataPtr += srcPadd) {
        for (var iTo = i + width; i < iTo; i++, srcDataPtr += pb) {
          c0Data[i] = srcDataPtr[RGB.R] * norm;
          c1Data[i] = srcDataPtr[RGB.G] * norm;
          c2Data[i] = srcDataPtr[RGB.B] * norm;
        }
      }

      return result;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComplexImage GetSubComplexImage(this UnmanagedImage image,
                                                  Rectangle bounds,
                                                  bool zeroPadding) {
      return GetSubComplexImage(image, bounds.X, bounds.Y, bounds.Width, bounds.Height, zeroPadding);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComplexImage GetSubComplexImage(this UnmanagedImage image, Rectangle bounds) {
      return GetSubComplexImage(image, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }



    public static unsafe UnmanagedImage GetChannelAsImage(this UnmanagedImage image, RGBA channel) {
      var c = (short)channel;
      var pb = image.GetPixelFormatSizeInBytes();
      if (c < 0 ||
          c >= pb) {
        throw new ArgumentOutOfRangeException(nameof(c));
      }

      var wb = pb * image.Width;
      var pad = image.Stride - wb;

      var result = UnmanagedImage.Create(image.Width, image.Height, PixelFormat.Format8bppIndexed);

      var d = (byte*)image.ImageData + c;
      var e = (byte*)result.ImageData;
      for (var dT0 = d + image.Stride * image.Height; d < dT0; d += pad) {
        for (var dToLine = d + wb; d < dToLine; d += pb, e++) {
          *e = *d;
        }
      }

      return result;
    }

    #endregion


    #region draw

    public static void Draw(this UnmanagedImage thisImage, UnmanagedImage srcImage, int x, int y) {
      if (srcImage.PixelFormat != thisImage.PixelFormat) {
        throw new InvalidOperationException(
          "Different image formats: '" + srcImage.PixelFormat + "' and '" + thisImage.PixelFormat + "'."
        );
      }

      var pixelBytes = srcImage.GetPixelFormatSizeInBytes();
      x *= pixelBytes;
      var sS = srcImage.Stride;
      var sD = thisImage.Stride;
      var nSrc = Math.Min(srcImage.Height, thisImage.Height - y) * sS; // bottom clip
      var srcClipStride = Math.Min(
        srcImage.Width * pixelBytes,
        thisImage.Width * pixelBytes - x
      ); // right clip
      var iSrc = 0;
      var iDst = x + (y - 1) * sD; //y-1 because increment before use
      if (y < 0) { // top clip
        iSrc -= y * sS;
        iDst -= y * sD;
      }

      if (x < 0) { // left clip
        srcClipStride += x;
        iSrc -= x;
        //nSrc -= x;  // TODO: required?
        iDst -= x; // x = 0
      }

      Debug.Assert(srcClipStride >= 0 && iSrc < nSrc, "There is always a intersect.");
      var d = srcImage.ImageData;
      var e = thisImage.ImageData;
      for (; iSrc < nSrc; iSrc += sS) {
        MarshalX.Copy(e + (iDst += sD), d + iSrc, srcClipStride);
      }
    }



    public static void Draw(this UnmanagedImage src, UnmanagedImage dst, Point location) {
      Draw(src, dst, location.X, location.Y);
    }



    /// <summary>
    ///   Draws the absolute part of an complex image to the destination image.
    /// </summary>
    /// <param name="dstPtr">pointer of the destination image, points to the first byte to draw.</param>
    /// <param name="stride">stride of the destination image</param>
    /// <param name="pixelBytes">number of byte per pixel of the destination image </param>
    /// <param name="srcImage">source image</param>
    /// <param name="i">start pixel of the source image</param>
    /// <param name="width">width to draw</param>
    /// <param name="height">height to draw</param>
    private static unsafe void DoDraw(IntPtr dstPtr,
                                      int stride,
                                      int pixelBytes,
                                      ComplexImage srcImage,
                                      int i,
                                      int width,
                                      int height) {
      const float norm = (float)byte.MaxValue / ComplexImage.MAX_MAGNITUDE;
      var srcPadd = i % srcImage.Width + srcImage.Width - width; // sum left and right padding
      var dstPadd = stride - width * pixelBytes;
      var d = (byte*)dstPtr;
      var c0 = srcImage.Channel0;
      var c1 = srcImage.Channel1;
      var c2 = srcImage.Channel2;

      switch (pixelBytes) {
        case 3:
          for (var iToB = i + height * srcImage.Width; i < iToB; i += srcPadd, d += dstPadd) {
            for (var iToL = i + width; i < iToL; i++, d += pixelBytes) {
              d[RGB.R] = (c0[i].Magnitude * norm).ClampToByte();
              d[RGB.G] = (c1[i].Magnitude * norm).ClampToByte();
              d[RGB.B] = (c2[i].Magnitude * norm).ClampToByte();
            }
          }

          break;
        case 4:
          for (var iToB = i + height * srcImage.Width; i < iToB; i += srcPadd, d += dstPadd) {
            for (var iToL = i + width; i < iToL; i++, d += pixelBytes) {
              d[RGB.R] = (c0[i].Magnitude * norm).ClampToByte();
              d[RGB.G] = (c1[i].Magnitude * norm).ClampToByte();
              d[RGB.B] = (c2[i].Magnitude * norm).ClampToByte();
              d[RGB.A] = byte.MaxValue;
            }
          }

          break;
        //default: already caught at the beginning
      }
    }



    /// <summary>
    ///   Draws the absolut part of an complex image to the destination image.
    /// </summary>
    /// <param name="thisImage"></param>
    /// <param name="srcImage"></param>
    public static void Draw(this UnmanagedImage thisImage, ComplexImage srcImage) {
      var pb = Preferences.Supported.CheckAndGetPixelBytes(thisImage);
      var w = Math.Min(srcImage.Width, thisImage.Width);
      var h = Math.Min(srcImage.Height, thisImage.Height);
      DoDraw(thisImage.ImageData, thisImage.Stride, pb, srcImage, 0, w, h);
    }



    /// <summary>
    ///   Draws the absolut part of an complex image to the destination image using the clipping bounds (x, y, width, height).
    /// </summary>
    /// <param name="thisImage"></param>
    /// <param name="srcImage"></param>
    /// <param name="x">x of clip</param>
    /// <param name="y">y of clip</param>
    /// <param name="width">width of clip</param>
    /// <param name="height">height of clip</param>
    public static void Draw(this UnmanagedImage thisImage, ComplexImage srcImage, int x, int y, int width, int height) {
      //width,height required because of zero padding
      Debug.Assert(srcImage.Width >= width);
      Debug.Assert(srcImage.Height >= height);
      var pb = Preferences.Supported.CheckAndGetPixelBytes(thisImage);

      var dstPtr = thisImage.ImageData;

      //clipping
      if (x + width > thisImage.Width) {
        width = thisImage.Width - x; // right clip
      }

      if (y + height > thisImage.Height) {
        height = thisImage.Height - y; // bottom clip
      }

      if (x <= 0) {
        x = -x; // left clip
      } else {
        dstPtr += x * pb;
        x = 0;
      }

      if (y <= 0) {
        y = -y; // left clip
      } else {
        dstPtr += y * thisImage.Stride;
        y = 0;
      }

      DoDraw(dstPtr, thisImage.Stride, pb, srcImage, y * srcImage.Width + x, width, height);
    }



    /// <summary>
    ///   Draws the absolut part of an complex image to the destination image using the clipping bounds.
    /// </summary>
    /// <param name="thisImage"></param>
    /// <param name="srcImage"></param>
    /// <param name="clip"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Draw(this UnmanagedImage thisImage, ComplexImage srcImage, Rectangle clip) {
      Draw(thisImage, srcImage, clip.X, clip.Y, clip.Width, clip.Height);
    }



    /// <summary>
    ///   Draws the real part of an complex image to the destination image.
    /// </summary>
    /// <param name="thisImage"></param>
    /// <param name="srcImage"></param>
    public static unsafe void DrawReals(this UnmanagedImage thisImage, ComplexImage srcImage) {
      const float norm = (float)byte.MaxValue / ComplexImage.MAX_MAGNITUDE;
      var pb = Preferences.Supported.CheckAndGetPixelBytes(thisImage);
      var w = Math.Min(srcImage.Width, thisImage.Width);
      var h = Math.Min(srcImage.Height, thisImage.Height);

      var dstPtr = (byte*)thisImage.ImageData;
      var dstStride = thisImage.Stride;
      var dstPadd = dstStride - w * pb;
      var c0 = srcImage.Channel0;
      var c1 = srcImage.Channel1;
      var c2 = srcImage.Channel2;

      switch (pb) {
        case 3:
          for (int i0 = 0,
                   n = srcImage.Width * h;
               i0 < n;
               i0 += srcImage.Width, dstPtr += dstPadd) {
            for (int i = i0,
                     m = i0 + w;
                 i < m;
                 i++, dstPtr += pb) {
              dstPtr[RGB.R] = (c0[i].Real * norm).ClampToByte();
              dstPtr[RGB.G] = (c1[i].Real * norm).ClampToByte();
              dstPtr[RGB.B] = (c2[i].Real * norm).ClampToByte();
            }
          }

          break;
        case 4:
          for (int i0 = 0,
                   n = srcImage.Width * h;
               i0 < n;
               i0 += srcImage.Width, dstPtr += dstPadd) {
            for (int i = i0,
                     m = i0 + w;
                 i < m;
                 i++, dstPtr += pb) {
              dstPtr[RGB.R] = (c0[i].Real * norm).ClampToByte();
              dstPtr[RGB.G] = (c1[i].Real * norm).ClampToByte();
              dstPtr[RGB.B] = (c2[i].Real * norm).ClampToByte();
              dstPtr[RGB.A] = byte.MaxValue;
            }
          }

          break;
        //default: already caught at the beginning
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte PhaseToByte(this Complex c) {
      const double aNorm = byte.MaxValue / (2 * Math.PI);
      return ((c.Phase + Math.PI) * aNorm).ClampToByte();
    }



    /// <summary>
    ///   Draws the phase part of an complex image to the destination image.
    /// </summary>
    /// <param name="thisImage"></param>
    /// <param name="srcImage"></param>
    public static unsafe void DrawPhases(this UnmanagedImage thisImage, ComplexImage srcImage) {
      var w = Math.Min(srcImage.Width, thisImage.Width);
      var h = Math.Min(srcImage.Height, thisImage.Height);
      var pb = Preferences.Supported.CheckAndGetPixelBytes(thisImage);

      var dstPtr = (byte*)thisImage.ImageData;
      var dstStride = thisImage.Stride;
      var dstPadd = dstStride - w * pb;
      var c0 = srcImage.Channel0;
      var c1 = srcImage.Channel1;
      var c2 = srcImage.Channel2;

      switch (pb) {
        case 3:
          for (int i0 = 0,
                   n = srcImage.Width * h;
               i0 < n;
               i0 += srcImage.Width, dstPtr += dstPadd) {
            for (int i = i0,
                     m = i0 + w;
                 i < m;
                 i++, dstPtr += pb) {
              dstPtr[RGB.R] = c0[i].PhaseToByte();
              dstPtr[RGB.G] = c1[i].PhaseToByte();
              dstPtr[RGB.B] = c2[i].PhaseToByte();
            }
          }

          break;
        case 4:
          for (int i0 = 0,
                   n = srcImage.Width * h;
               i0 < n;
               i0 += srcImage.Width, dstPtr += dstPadd) {
            for (int i = i0,
                     m = i0 + w;
                 i < m;
                 i++, dstPtr += pb) {
              dstPtr[RGB.R] = c0[i].PhaseToByte();
              dstPtr[RGB.G] = c1[i].PhaseToByte();
              dstPtr[RGB.B] = c2[i].PhaseToByte();
              dstPtr[RGB.A] = byte.MaxValue;
            }
          }

          break;
        //default: already caught at the beginning
      }
    }

    #endregion


    #region converter

    /// <summary>
    ///   Converts an unmanaged image to an complex image and uses zero padding if required.
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static ComplexImage ToComplexImage(this UnmanagedImage image) {
      var result = new ComplexImage(image.Width, image.Height);
      ToComplexImageZeroPad(image, result);
      return result;
    }



    /// <summary>
    ///   Converts an unmanaged image to an complex image and uses zero padding if required.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="dstWidth"></param>
    /// <param name="dstHeight"></param>
    /// <returns></returns>
    public static ComplexImage ToComplexImageZeroPad(this UnmanagedImage image, int dstWidth, int dstHeight) {
      if (dstWidth < 1 ||
          dstHeight < 1) {
        throw new ArgumentOutOfRangeException(dstWidth < 1 ? nameof(dstWidth) : nameof(dstHeight));
      }

      var result = new ComplexImage(dstWidth, dstHeight);
      ToComplexImageZeroPad(image, result);
      return result;
    }



    public static unsafe void ToComplexImageZeroPad(this UnmanagedImage image, [NotNull] ComplexImage outImage) {
      const float norm = ComplexImage.MAX_MAGNITUDE / (float)byte.MaxValue;
      //TODO: zero padding
      var wDst = outImage.Width;
      var w = Math.Min(image.Width, wDst);
      var h = Math.Min(image.Height, outImage.Height);
      var pb = ComplexImage.Compatible.CheckAndGetPixelBytes(image);
      var srcPadd = image.Stride - w * pb;
      var dt = (byte*)image.ImageData;
      var c0Data = outImage.Channel0;
      var c1Data = outImage.Channel1;
      var c2Data = outImage.Channel2;

      for (var i0 = 0; i0 < wDst * h; i0 += wDst, dt += srcPadd) {
        for (var i = i0; i < i0 + w; i++, dt += pb) {
          c0Data[i] = dt[RGB.R] * norm;
          c1Data[i] = dt[RGB.G] * norm;
          c2Data[i] = dt[RGB.B] * norm;
        }
      }
    }



    public static unsafe void ToComplexImageSymmetric(this UnmanagedImage image, [NotNull] ComplexImage outImage) {
      const float norm = ComplexImage.MAX_MAGNITUDE / (float)byte.MaxValue;
      //TODO: zero padding
      var wDst = outImage.Width;
      var w = Math.Min(image.Width, wDst);
      var h = Math.Min(image.Height, outImage.Height);
      var pb = ComplexImage.Compatible.CheckAndGetPixelBytes(image);
      var srcPadd = image.Stride - w * pb;
      var dt = (byte*)image.ImageData;
      var c0Data = outImage.Channel0;
      var c1Data = outImage.Channel1;
      var c2Data = outImage.Channel2;

      var w2 = w * 2;
      var wr = wDst % w2;
      var i = 0;
      for (int i0 = 0,
               i0To = wDst * h;
           i0 < i0To;
           i0 += wDst, dt += srcPadd) {
        for (; i < i0 + w; i++, dt += pb) {
          c0Data[i] = dt[RGB.R] * norm;
          c1Data[i] = dt[RGB.G] * norm;
          c2Data[i] = dt[RGB.B] * norm;
        }

        for (var j = i - 1; j < wDst; i++, j--) {
          c0Data[i] = c0Data[j];
          c1Data[i] = c1Data[j];
          c2Data[i] = c2Data[j];
        }

        for (var iTo = i0 + wDst - w2; i + w2 < iTo; i += w2) {
          Array.Copy(c0Data, i0, c0Data, i, w2);
          Array.Copy(c1Data, i0, c1Data, i, w2);
          Array.Copy(c2Data, i0, c2Data, i, w2);
        }

        if (wr <= 0) {
          continue;
        }

        Array.Copy(c0Data, i0, c0Data, i, wr);
        Array.Copy(c1Data, i0, c1Data, i, wr);
        Array.Copy(c2Data, i0, c2Data, i, wr);
      }

      var lDst = wDst * outImage.Height;
      var l = h * wDst;
      var l2 = l * 2;
      for (var j = i - wDst; i < lDst; i += wDst, j -= wDst) {
        Array.Copy(c0Data, j, c0Data, i, wr);
        Array.Copy(c1Data, j, c1Data, i, wr);
        Array.Copy(c2Data, j, c2Data, i, wr);
      }

      var lr = lDst % l2;
      for (int iTo = i - l2; i < iTo; i += l2) {
        Array.Copy(c0Data, 0, c0Data, i, l2);
        Array.Copy(c1Data, 0, c1Data, i, l2);
        Array.Copy(c2Data, 0, c2Data, i, l2);
      }

      if (lr <= 0) {
        return;
      }

      Array.Copy(c0Data, 0, c0Data, i, lr);
      Array.Copy(c1Data, 0, c1Data, i, lr);
      Array.Copy(c2Data, 0, c2Data, i, lr);
    }



    /// <summary>
    ///   Create an ComplexImage from this image with a width/height that is the smallest power of two which isgreater or equal
    ///   than the size of the input image.
    /// </summary>
    /// <param name="image"></param>
    public static ComplexImage ToComplexImageZeroPad(this UnmanagedImage image) {
      return ToComplexImageZeroPad(image, MathCV.UpperPow2(image.Width), MathCV.UpperPow2(image.Height));
    }

    #endregion
  }
}
