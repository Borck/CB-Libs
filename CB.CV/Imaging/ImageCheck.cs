using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Accord;
using Accord.Imaging;



namespace CB.CV.Imaging {
  [Serializable]
  public class ImageCheck : HashSet<PixelFormat> {
    #region check support

    protected ImageCheck(SerializationInfo info, StreamingContext context)
      : base(info, context) { }



    public ImageCheck() { }



    /// <summary>
    ///   Checks if it contains the format, otherwise it throws a <see cref="BadImageFormatException" />
    /// </summary>
    /// <param name="format">the pixel format to check</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckFormat(PixelFormat format) {
      if (!Contains(format)) {
        throw new BadImageFormatException(format.ToString());
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckFormat(PixelFormat format, out int pixelBytes) {
      pixelBytes = CheckAndGetPixelBytes(format);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckFormat(UnmanagedImage image) {
      CheckFormat(image.PixelFormat);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckFormat(UnmanagedImage image, out int pixelBytes) {
      pixelBytes = CheckAndGetPixelBytes(image.PixelFormat);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CheckAndGetPixelBytes(PixelFormat format) {
      CheckFormat(format);
      return format.GetBytesPerPixel();
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CheckAndGetPixelBytes(UnmanagedImage image) {
      return CheckAndGetPixelBytes(image.PixelFormat);
    }



    private static void DoChSameFormat(params UnmanagedImage[] images) {
      var pf = images[0].PixelFormat;
      if (pf != images[1].PixelFormat) {
        throw new BadImageFormatException("Images are not in the same pixel format.");
      }

      for (var i = 2; i < images.Length; i++) {
        if (pf != images[i].PixelFormat) {
          throw new BadImageFormatException("Images are not in the same pixel format.");
        }
      }
    }



    public void Check(ImageCheckOptions checkImageCheckOption, params UnmanagedImage[] images) {
      if (images.Length <= 0) {
        return;
      }

      if (checkImageCheckOption.HasFlag(ImageCheckOptions.FORMAT_SUPPORT)) {
        CheckFormat(images[0].PixelFormat);
      }

      if (images.Length <= 1) {
        return;
      }

      if (checkImageCheckOption.HasFlag(ImageCheckOptions.FORMAT_SAME)) {
        DoChSameFormat(images);
      }

      if (checkImageCheckOption.HasFlag(ImageCheckOptions.SIZE_SAME)) {
        DoChSameSize(images);
      }
    }



    public static void CheckSameFormat(params UnmanagedImage[] images) {
      if (images.Length > 1) {
        DoChSameFormat(images);
      }
    }



    private static void DoChSameSize(UnmanagedImage[] images) {
      var im = images[0];
      var w = im.Width;
      var h = im.Height;

      im = images[1];
      if (w != im.Width ||
          h != im.Height) {
        throw new DimensionMismatchException("Image 1");
      }

      for (var i = 2; i < images.Length; i++) {
        im = images[i];
        if (w != im.Width ||
            h != im.Height) {
          throw new DimensionMismatchException("Image " + i);
        }
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CheckSameSize(params UnmanagedImage[] images) {
      if (images.Length > 1) {
        DoChSameSize(images);
      }
    }



    /// <summary>
    ///   Checks if the images are supported and have the same format.
    /// </summary>
    /// <param name="pixelBytes">pixel size in bytes</param>
    /// <param name="images">images to check</param>
    public void CheckFormatAll(out int pixelBytes, params UnmanagedImage[] images) {
      if (images.Length <= 0) {
        pixelBytes = -1;
        return;
      }

      DoChSameSize(images);
      pixelBytes = CheckAndGetPixelBytes(images[0].PixelFormat);
    }

    #endregion

    #region is supported

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSupported(PixelFormat format) {
      return Contains(format);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSupported(UnmanagedImage image) {
      return Contains(image.PixelFormat);
    }

    #endregion

    #region hidden methodes

    public new bool Add(PixelFormat pixelFormat) {
      if (new StackFrame(1).GetMethod().MemberType != MemberTypes.Constructor) {
        throw new NotSupportedException();
      }

      return base.Add(pixelFormat);
    }



    public new bool IntersectWith(IEnumerable<PixelFormat> other) {
      throw new NotSupportedException();
    }



    public new bool Remove(PixelFormat pixelFormat) {
      throw new NotSupportedException();
    }



    public new bool RemoveWhere(Predicate<PixelFormat> match) {
      throw new NotSupportedException();
    }



    public new bool SymmetricExceptWith(IEnumerable<PixelFormat> other) {
      throw new NotSupportedException();
    }



    public new bool TrimExcess() {
      throw new NotSupportedException();
    }



    public new bool UnionWith(IEnumerable<PixelFormat> other) {
      throw new NotSupportedException();
    }

    #endregion
  }



  [Flags]
  public enum ImageCheckOptions {
    FORMAT_SUPPORT = 1,
    FORMAT_SAME = 2,
    FORMAT_SUPPORT_SAME = 3,
    SIZE_SAME = 4,
    ALL = FORMAT_SUPPORT | FORMAT_SAME | SIZE_SAME
  }
}
