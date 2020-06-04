using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Accord.Imaging;



namespace CB.CV.Imaging.Filtering {
  /// <summary>
  ///   This class extracts patches from an image by searching in a grid for edges. Those structures will be given by
  ///   filtering the image with a linear scale space (lss) filter.
  /// </summary>
  public class ContentSensitivePatcher : RegularGridPatcher {
    private static readonly ImageCheck Ic = new ImageCheck {PixelFormat.Format8bppIndexed};
    private int _min;
    private int _max;

    public double MinThreshold {
      get { return (double)_min / (PatchWidth * PatchHeight); }
      set {
        if (value < 0 ||
            value > 1) {
          throw new ArgumentOutOfRangeException();
        }

        DoSetMin(value);
      }
    }

    public double MaxThreshold {
      get { return (double)_max / (PatchWidth * PatchHeight); }
      set {
        if (value < 0 ||
            value > 1) {
          throw new ArgumentOutOfRangeException();
        }

        DoSetMax(value);
      }
    }

    protected readonly LinearScaleSpace Lss;



    /// <summary>
    /// </summary>
    /// <param name="patchWidth"></param>
    /// <param name="patchHeight"></param>
    /// <param name="sigma">parameter for the gaussian filters in lss</param>
    /// <param name="overlap"></param>
    public ContentSensitivePatcher(int patchWidth, int patchHeight, double overlap, double sigma)
      : base(patchWidth, patchHeight, overlap) {
      Lss = new LinearScaleSpace(sigma);
      DoSetMin();
      DoSetMax();
    }



    public ContentSensitivePatcher(int patchWidth, int patchHeight, double sigma)
      : base(patchWidth, patchHeight) {
      Lss = new LinearScaleSpace(sigma);
      DoSetMin();
      DoSetMax();
    }



    public ContentSensitivePatcher(int patchWidth, int patchHeight)
      : base(patchWidth, patchHeight) {
      Lss = new LinearScaleSpace();
      DoSetMin();
      DoSetMax();
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoSetMin(double threshold = Preferences.CSP_MIN) {
      var min = _min = (int)Math.Ceiling(threshold * PatchWidth * PatchHeight);
      if (min > _max) {
        _max = min;
      }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoSetMax(double threshold = Preferences.CSP_MAX) {
      var max = _max = (int)Math.Ceiling(threshold * PatchWidth * PatchHeight);
      if (max < _min) {
        _min = max;
      }
    }



    public new Rectangle[] ProcessPatches(UnmanagedImage image) {
      Ic.CheckFormat(image.PixelFormat);

      var pw = PatchWidth;
      var ph = PatchHeight;
      var w = image.Width / pw * pw;
      var h = image.Height / ph * ph;
      var bytes = new byte[image.Width * image.Height];
      using (var img = Lss.Apply(image).Clone()) // clone required because of a bug
      {
        img.CopyTo(ref bytes);
      }

      var min = _min;
      var max = _max;

      var x0 = (image.Width - w) / 2;
      var y0 = (image.Height - h) / 2;
      var result = new List<Rectangle>((image.Width / PatchWidth) * (image.Height / PatchHeight));

      var ls = w - pw;
      for (int y = y0,
               xTo = x0 + w,
               yTo = y0 + h;
           y < yTo;
           y += PatchHeight) {
        for (var x = x0; x < xTo;) {
          var pc = 1;
          // for each pixel in patch
          for (int i = y * w + x,
                   iTo = i + ph * w;
               i < iTo;
               i += ls) {
            for (var iToLine = i + pw; i < iToLine; i++) {
              if (pc > max) {
                goto NextPatch;
              }

              if (bytes[i] == 0) {
                continue;
              }

              // zero crossing found
              pc++;
            }
          }

          if (pc >= min &&
              pc <= max) {
            result.Add(new Rectangle(x, y, PatchWidth, PatchHeight));
          }

          NextPatch:
          x += PatchWidth;
        }
      }

      return result.ToArray();
    }
  }
}
