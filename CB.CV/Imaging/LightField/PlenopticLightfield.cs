using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Accord.Imaging;
using CB.System.Runtime.InteropServices;



namespace CB.CV.Imaging.LightField {
  [DataContract]
  public class PlenopticLightField : ALightField {
    // TODO: bring to life!!! then refactor!!!
    // TODO: rearange after open() without loading images


    #region field

    //TODO: check XmlIgnore's correspondes with SetParameters method

    private UnmanagedImage _image;
    private UnmanagedImage _shearedImage;

    [DataMember]
    public PlenopticLightfieldType CameraLightfieldType;

    [DataMember]
    public Rectangle Grid;

    [DataMember]
    public RectangleF GridSegment;

    [DataMember]
    public SizeF GridSegmentSize;

    [DataMember]
    public Point Sheared;

    [DataMember]
    public PointF StereoOffset;

    [DataMember]
    public SizeF PitchSize;

    [DataMember]
    public Point Shear { get; set; }

    [DataMember]
    public Size GridSize { get; set; }

    [DataMember]
    public int GridColumns = 1;

    [DataMember]
    public int GridRows = 1;

    public UnmanagedImage Image {
      get { return _shearedImage; }
      private set {
        if ((_image = value) == null) {
          return;
        }

        GridSize = new Size(value.Width, value.Height);
        if (Grid.Width == 0) {
          Grid.Width = value.Width;
        }

        if (Grid.Height == 0) {
          Grid.Height = value.Height;
        }

        Rearange();
      }
    }

    #endregion

    #region constructor

    public PlenopticLightField(string path, PlenopticLightfieldType type = PlenopticLightfieldType.Default)
      : base(new[] {path}) {
      CameraLightfieldType = type;
    }



    internal PlenopticLightField(Bitmap image, PlenopticLightfieldType type = PlenopticLightfieldType.Default)
      : base(image) {
      CameraLightfieldType = type;
    }

    #endregion



    private void Rearange() {
      if (GridSize.Height < 1 ||
          GridSize.Width < 1) {
        _image = null;
        return;
      }

      if (GridSegmentSize.Width <= 0) {
        GridSegmentSize.Width = 1;
        GridSegment.Width = 1;
      }

      if (GridSegmentSize.Height <= 0) {
        GridSegmentSize.Height = 1;
        GridSegment.Height = 1;
      }

      StereoOffset.X = Math.Max(StereoOffset.X, 1);
      StereoOffset.Y = Math.Max(StereoOffset.Y, 1);
      PitchSize.Width = Math.Max(PitchSize.Width, 1);
      PitchSize.Height = Math.Max(PitchSize.Height, 1);

      Grid.X = Math.Min(Grid.X, GridSize.Width - 1);
      Grid.Y = Math.Min(Grid.Y, GridSize.Height - 1);

      GridColumns = (int)((GridSize.Width - Grid.X) / GridSegmentSize.Width);
      GridRows = (int)((GridSize.Height - Grid.Y) / GridSegmentSize.Height);

      Grid.Width = (int)(GridColumns * GridSegmentSize.Width);
      Grid.Height = (int)(GridRows * GridSegmentSize.Height);

      if (GridSegment.X >= GridSegmentSize.Width) {
        GridSegment.X = 0;
      }

      if (GridSegment.Width <= 0 ||
          GridSegment.Width + GridSegment.X > GridSegmentSize.Width) {
        GridSegment.Width = GridSegmentSize.Width - GridSegment.X;
      }

      if (GridSegment.Y >= GridSegmentSize.Height) {
        GridSegment.Y = 0;
      }

      if (GridSegment.Height <= 0 ||
          GridSegment.Height + GridSegment.Y > GridSegmentSize.Height) {
        GridSegment.Height = GridSegmentSize.Height - GridSegment.Y;
      }

      PitchSize.Width = Math.Min(PitchSize.Width, GridSegment.Width);
      PitchSize.Height = Math.Min(PitchSize.Height, GridSegment.Height);

      ViewColumns = (int)((GridSegment.Width - PitchSize.Width) / StereoOffset.X + 1);
      ViewRows = (int)((GridSegment.Height - PitchSize.Height) / StereoOffset.Y + 1);

      if (_shearedImage != null &&
          Sheared.Equals(Shear)) {
        return;
      }

      var image = new Bitmap(_image.Width, _image.Height);
      var g = Graphics.FromImage(image);

      using (var imMan = _image.ToManagedImage()) {
        g.DrawImage(
          imMan,
          new[] {
            new Point(Shear.X, Shear.Y), new Point(_image.Width + Shear.X, 0), new Point(0, _image.Height + Shear.Y)
          }
        );
      }

      _shearedImage = UnmanagedImage.FromManagedImage(image);
      g.Dispose();
      Sheared = Shear;
      // TODO: check, if nothing is disposed/overwritten by shared
    }



    public void SetParameters(PlenopticLightFieldParameters parameters) {
      PitchSize.Width = parameters.Pitch * parameters.MaskRatio;
      PitchSize.Height = parameters.Pitch;

      StereoOffset.X = parameters.StereoOffset * parameters.MaskRatio;
      StereoOffset.Y = parameters.StereoOffset;

      Grid.X = parameters.Offset.X;
      Grid.Y = parameters.Offset.Y;

      GridSegmentSize.Width = parameters.MaskSize * parameters.MaskRatio;
      GridSegmentSize.Height = parameters.MaskSize;

      var cut = parameters.Cut;
      GridSegment.X = cut.X;
      GridSegment.Y = cut.Y;
      GridSegment.Width = GridSegmentSize.Width - cut.Width;
      GridSegment.Height = GridSegmentSize.Height - cut.Height;

      Shear = new Point(parameters.Shear.X, parameters.Shear.Y);
      CameraLightfieldType = parameters.CameraLightfieldType;

      Rearange();
    }



    public PlenopticLightFieldParameters GetParameters() {
      return new PlenopticLightFieldParameters {
        Offset = new Point(Grid.X, Grid.Y),
        MaskSize = GridSegmentSize.Height,
        MaskRatio = GridSegmentSize.IsEmpty
                      ? 1
                      : GridSegmentSize.Width / GridSegmentSize.Height,
        Pitch = PitchSize.Height,
        StereoOffset = StereoOffset.Y,
        Cut =
          new RectangleF(
            GridSegment.X,
            GridSegment.Y,
            GridSegmentSize.Width - GridSegment.Width,
            GridSegmentSize.Height - GridSegment.Height
          ),
        Shear = new Point(Shear.X, Shear.Y),
        CameraLightfieldType = CameraLightfieldType
      };
    }



    public override void Load() {
      if (Fetched) {
        return;
      }

      Images = DoFetchImages(FileNames);
      AutoAdjustRawLightfield();
    }



    private void CheckLoadImages() {
      if (Image == null) {
        throw new InvalidOperationException(nameof(Image) + " not loaded. Use " + nameof(Load) + " first.");
      }
    }



    protected override UnmanagedImage GetImage(int i) {
      CheckLoadImages();

      // possible error source: two kinds of rows/columns (check rev <10)
      var srcPtr = _image.ImageData;
      var srcWidth = _image.Width;
      var srcHeight = _image.Height;
      var destWidth = (int)(PitchSize.Width * GridColumns);
      var destHeight = (int)(PitchSize.Height * GridRows);

      var dst = UnmanagedImage.Create(destWidth, destHeight, _image.PixelFormat);
      var pixelBytes = dst.GetPixelFormatSizeInBytes();

      var dstPtr = dst.ImageData;

      var column = i % ViewColumns;
      var row = i / ViewColumns;

      try {
        Parallel.For(
          0,
          GridRows,
          Preferences.PoOne,
          y => {
            for (var x = 0; x < GridColumns; x++) {
              for (var gridRow = 0; gridRow < PitchSize.Height; gridRow++) {
                var srcX = (int)(Grid.Left + x * GridSegmentSize.Width + GridSegment.Left + column * StereoOffset.X);
                var srcY = (int)(Grid.Top +
                                 y * GridSegmentSize.Height +
                                 GridSegment.Top +
                                 gridRow +
                                 row * StereoOffset.Y);
                int desX;
                int desY;
                if (CameraLightfieldType == PlenopticLightfieldType.Keplerian) {
                  desX = (int)((GridColumns - x - 1) * PitchSize.Width);
                  desY = (int)((GridRows - y - 1) * PitchSize.Height) + gridRow;
                } else {
                  desX = (int)(x * PitchSize.Width);
                  desY = (int)(y * PitchSize.Height) + gridRow;
                }

                var srcI = srcY * srcWidth + srcX;
                var desI = desY * destWidth + desX;

                var length = (int)PitchSize.Width + 1;
                if (desX + length >= destWidth ||
                    srcX + length >= srcWidth) {
                  length--;
                }

                if (desI < 0 ||
                    srcI < 0 ||
                    desI + length >= destWidth * destHeight ||
                    srcI + length >= srcWidth * srcHeight) {
                  continue;
                }

                MarshalX.Copy(dstPtr + desI * pixelBytes, srcPtr + srcI * pixelBytes, length * pixelBytes);
              }
            }
          }
        );
      } catch (OperationCanceledException) {
        Console.Error.WriteLine("Image calculation canceled.");
        return null;
      }

      var result = dst.ToManagedImage(false);
      if (CameraLightfieldType == PlenopticLightfieldType.Keplerian) {
        result.RotateFlip(RotateFlipType.RotateNoneFlipXY);
      }

      // TODO: refactor
      return UnmanagedImage.FromManagedImage(result);
    }



    public Bitmap GetMask(PointF zoomPosRelativ) {
      CheckLoadImages();

      const int maxPartSize = 10;
      var columns = Math.Min(GridColumns, maxPartSize);
      var rows = Math.Min(GridRows, maxPartSize);

      var dstSize = new Size(
        (int)(columns == 0 ? GridSegmentSize.Width : GridSegmentSize.Width * columns),
        (int)(rows == 0 ? GridSegmentSize.Height : GridSegmentSize.Height * rows)
      );

      var dst = UnmanagedImage.Create(dstSize.Width, dstSize.Height, _image.PixelFormat);
      var dstData = dst.ImageData;

      var zoomPos = new Point(
        (int)(zoomPosRelativ.X * (GridSize.Width - 1)),
        (int)(zoomPosRelativ.Y * (GridSize.Height - 1))
      );

      var srcData = Image.ImageData;
      var pixelBytes = Image.GetPixelFormatSizeInBytes();
      var stride = Image.Stride;
      var row = Enumerable.Repeat(byte.MaxValue, stride).ToArray(); // TODO: caching required?

      /* Zoom */
      var srcX = zoomPos.X - dstSize.Width / 2;
      var destX = 0;
      var xDrift = srcX < 0 ? srcX :
                   srcX + dstSize.Width >= GridSize.Width ? srcX + dstSize.Width - GridSize.Width + 1 : 0;
      if (xDrift < 0) {
        srcX -= xDrift;
      }

      if (xDrift < 0) {
        destX -= xDrift;
      }

      var length = dstSize.Width - Math.Abs(xDrift);

      var leftOffsetRow = Grid.Left - srcX;
      if (leftOffsetRow < 0) {
        leftOffsetRow = 0;
      }

      var rightOffsetRow = srcX + length - Grid.Right;
      if (rightOffsetRow < 0) {
        rightOffsetRow = 0;
      }

      var leftOffsetColumn = Grid.Left - srcX;
      if (leftOffsetColumn < 0) {
        leftOffsetColumn = (int)(leftOffsetColumn % GridSegmentSize.Width);
      }

      if (length <= 0) {
        return dst.ToManagedImage(false);
      }

      Parallel.For(
        0,
        dstSize.Height,
        Preferences.PoOne,
        i => {
          var srcY = i + zoomPos.Y - dstSize.Height / 2;
          var destY = i;

          if (srcY < 0 ||
              srcY >= GridSize.Height) {
            return;
          }

          var srcI = srcY * GridSize.Width + srcX;
          var destI = destY * dstSize.Width + destX;

          MarshalX.Copy(dstData + destI * 4, srcData + srcI * 4, length * 4);
        }
      );

      Parallel.For(
        0,
        dstSize.Height,
        Preferences.PoOne,
        i => {
          var srcY = i + zoomPos.Y - dstSize.Height / 2;
          var destY = i;

          if (srcY < 0 ||
              srcY >= GridSize.Height) {
            return;
          }

          var destI = destY * dstSize.Width + destX;
          /* weiße Linien */
          if (srcY < Grid.Top ||
              srcY >= Grid.Bottom) {
            return;
          }

          if (length - leftOffsetRow - rightOffsetRow <= 0) {
            return;
          }

          /* Spalten */
          for (float j = leftOffsetColumn; j < length - rightOffsetRow; j += GridSegmentSize.Width) {
            for (var cutX = 0; cutX <= GridSegment.Left; cutX++) {
              var x = cutX + (int)j;
              var k = destI + x;

              if (x < 0 ||
                  x >= length) {
                continue;
              }

              Marshal.Copy(row, 0, dstData + k * pixelBytes, pixelBytes);
            }

            for (var cutX = (int)(GridSegment.Right + 1); cutX < GridSegmentSize.Width; cutX++) {
              var x = cutX + (int)j;
              if (x < 0 ||
                  x >= length) {
                continue;
              }

              Marshal.Copy(row, 0, dstData + (destI + x) * pixelBytes, pixelBytes);
            }
          }

          /* Zeilen */
          var segmentRow = (srcY - Grid.Top) % GridSegmentSize.Height;
          if (segmentRow >= 0 && segmentRow < GridSegment.Top + 1 ||
              segmentRow > GridSegment.Bottom && segmentRow < GridSegmentSize.Height) {
            Marshal.Copy(
              row,
              0,
              dstData + (destI + leftOffsetRow) * pixelBytes,
              (length - leftOffsetRow - rightOffsetRow) * pixelBytes
            );
          }
        }
      );

      return dst.ToManagedImage(false);
    }



    private static float GetInterval(IList<double> gradients, out int offset) {
      var size = gradients.Count;
      offset = 0;

      double max = 0.0f;
      var maxPos = 0;
      for (var i = 0; i < size; i++) {
        if (gradients[i] > max) {
          max = gradients[i];
          maxPos = i;
        }
      }

      var highPoints = new List<int>();
      for (var i = 0; i < size; i++) {
        if (gradients[i] / max >= 0.4f) {
          highPoints.Add(i);
        }
      }

      var distances = new Dictionary<int, int>();
      for (var i = 0; i < highPoints.Count - 1; i++) {
        var distance = highPoints[i + 1] - highPoints[i];
        if (distance < 2) {
          continue;
        }

        if (!distances.ContainsKey(distance)) {
          distances.Add(distance, 0);
        }

        distances[distance]++;
      }

      int[] lowestKey = {int.MaxValue};
      var maxCount = distances.Values.Concat(new[] {0}).Max();

      foreach (var key in distances.Keys.Where(key => lowestKey[0] > key && distances[key] > maxCount / 2)) {
        lowestKey[0] = key;
      }

      /*foreach (int key in distances.Keys)
          if (lowestKey > key && distances[key] > maxCount / 2)
              lowestKey = key;   
       */

      if (lowestKey[0] == int.MaxValue) {
        return 0.0f;
      }

      var contKey = lowestKey[0];
      while (distances.ContainsKey(contKey++)) { }

      var distanceF = (lowestKey[0] + contKey - 1) * 0.5f;
      offset = (int)(maxPos % distanceF);
      return distanceF;
    }



    public void AutoAdjustRawLightfield() {
      var width = Image.Width;
      var height = Image.Height;

      /* Spalten */
      var cols = width - 1;
      var rowGradients = new double[cols];
      //for(var i = 0; i < cols; i++) rowGradients[i] = 0;
      // TODO: check it!!
      if (Math.Abs(rowGradients[0]) > 0.00001) {
        throw new InvalidOperationException("uncomment upper initiation loop");
      }

      for (var y = 0; y < height; y++) {
        var y1 = y;
        Parallel.For(
          0,
          width - 1,
          Preferences.PoOne,
          x => {
            rowGradients[x] += Math.Abs(
              Image.GetPixel(x, y1).GetBrightness() -
              Image.GetPixel(x + 1, y1).GetBrightness()
            );
          }
        );
      }

      int leftOffset;
      var gridSegmentWidth = GetInterval(rowGradients, out leftOffset);

      /* Zeilen */
      var rows = height - 1;
      var columnGradients = new double[rows];
      //for(var i = 0; i < rows - 1; i++) columnGradients[i] = 0;
      // TODO: check it!!
      if (Math.Abs(columnGradients[0]) > 0.00001) {
        throw new InvalidOperationException("uncomment upper initiation loop");
      }

      for (var x = 0; x < width; x++) {
        var x1 = x;
        Parallel.For(
          0,
          height - 1,
          Preferences.PoOne,
          y => {
            columnGradients[y] += Math.Abs(
              Image.GetPixel(x1, y).GetBrightness() -
              Image.GetPixel(x1, y + 1).GetBrightness()
            );
          }
        );
      }

      int topOffset;
      SetParameters(
        new PlenopticLightFieldParameters {
          MaskSize = Math.Max(gridSegmentWidth, GetInterval(columnGradients, out topOffset)),
          MaskRatio = 1.0f,
          Pitch = 1,
          StereoOffset = 1,
          Offset = new Point(leftOffset, topOffset),
          Cut = new RectangleF(),
          Shear = new Point(),
          CameraLightfieldType = PlenopticLightfieldType.Default
        }
      );
    }



    protected override void Dispose(bool disposing) {
      _image?.Dispose();
      _shearedImage?.Dispose();
    }
  }
}
