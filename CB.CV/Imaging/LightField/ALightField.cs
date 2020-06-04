using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Accord.Imaging;
using Accord.Math;
using JetBrains.Annotations;



namespace CB.CV.Imaging.LightField {
  // TODO: use DataContractSerializer, and mark every property/field you want to serialize with [DataMember].



  [DataContract]
  [KnownType(typeof(CameraArrayLightField))]
  [KnownType(typeof(HeterodyneLightField))]
  [KnownType(typeof(PlenopticLightField))]
  public abstract class ALightField : IDisposable {
    #region static fields

    private static readonly DataContractSerializer Ser = new DataContractSerializer(typeof(ALightField));

    //public const float DEPTH_OF_FOCUS_MIN = -3f;
    //public const float DEPTH_OF_FOCUS_MAX = 1f;
    public const float DEPTH_OF_FOCUS_MIN = -2f;
    public const float DEPTH_OF_FOCUS_MAX = 0f;
    public const float DEPTH_OF_FOCUS_STEP = .2f;

    public const int DEPTH_OF_FOCUS_STEPS =
      (int)(((DEPTH_OF_FOCUS_MAX - DEPTH_OF_FOCUS_MIN)) / DEPTH_OF_FOCUS_STEP + 1.000001);

    public const int DEPTH_OF_FIELD_MIN = 1; // 0 = forbidden area, avoid all-in-focus
    public const int DEPTH_OF_FIELD_MAX = 8;

    protected const bool DEFAULT_INVERT_FOCUS = false;

    #endregion

    #region fields

    [DataMember]
    public int ViewRows { get; protected set; }

    [DataMember]
    public int ViewColumns { get; protected set; }

    [DataMember]
    // list of paths where the data is located
    public string[] FileNames { get; protected set; }

    [DataMember]
    public float MinimumDepthOfFocus => DEPTH_OF_FOCUS_MIN;

    [DataMember]
    public float MaximumDepthOfFocus => DEPTH_OF_FOCUS_MAX;

    [DataMember]
    public int MinimumDepthOfField => DEPTH_OF_FIELD_MIN;

    [DataMember]
    public int DepthOfFieldMax => (Math.Min(ViewRows, ViewColumns) - 1) / 2;

    [DataMember]
    public Point ViewPoint => new Point(ViewColumns / 2, ViewRows / 2);

    [DataMember]
    public UnmanagedImage[] Images { get; protected set; }

    /// <summary>
    ///   Indicates, if all required images in <see cref="FileNames" /> are loaded
    /// </summary>
    /// TODO: IsLoaded points on empty image array instead is bool field
    [DataMember]
    public bool Fetched => Images != null;

    #endregion

    #region constructor and creators

    protected ALightField(string[] fileNames) {
      if (fileNames.Length == 0) {
        throw new ArgumentException(nameof(fileNames) + " is empty.");
      }

      FileNames = fileNames;
    }



    protected ALightField(Bitmap[] images) {
      if (images == null ||
          images.Length == 0) {
        throw new ArgumentException(nameof(images) + " is null or empty.");
      }

      Images = images.Apply(UnmanagedImage.FromManagedImage);
    }



    protected ALightField(Bitmap image) {
      if (image == null) {
        throw new ArgumentException(nameof(image) + " is null.");
      }

      Images = new[] {UnmanagedImage.FromManagedImage(image)};
    }



    public static ALightField Create(string[] images, LightfieldType type) {
      switch (type) {
        case LightfieldType.CamArray:
          return new CameraArrayLightField(images);
        case LightfieldType.Heterodyned:
          return new HeterodyneLightField(images[0]);
        case LightfieldType.Plenoptic:
          return new PlenopticLightField(images[0]);
        case LightfieldType.Polymorphic:
          return Open(images[0]);
        default:
          throw new NotImplementedException();
      }
    }



    public static ALightField Create(Bitmap[] images, LightfieldType type) {
      switch (type) {
        case LightfieldType.CamArray:
          return new CameraArrayLightField(images);
        case LightfieldType.Heterodyned:
          return new HeterodyneLightField(images[0]);
        case LightfieldType.Plenoptic:
          return new PlenopticLightField(images[0]);
        default:
          throw new NotImplementedException();
      }
    }

    #endregion

    #region open/save

    public static ALightField Open(string fileName) {
      /* try open *.lf file */
      FileStream stream = null;
      ALightField result;
      try {
        stream = new FileStream(fileName, FileMode.Open);
        using (var reader = XmlReader.Create(stream)) {
          result = (ALightField)Ser.ReadObject(reader);
        }
      } finally {
        stream?.Dispose();
      }

      result.ConvertToAbsolute(fileName);
      return result;
    }



    /// <summary>
    ///   Loads all images into the light field
    /// </summary>
    public abstract void Load();



    protected static UnmanagedImage[] DoFetchImages(string[] fileNames) {
      var n = fileNames.Length;
      if (n == 0) {
        throw new InvalidDataException("no paths");
      }

      var images = new UnmanagedImage[n];

      var i = 0;
      try {
        foreach (var fileName in fileNames) {
          using (var image = new Bitmap(fileName)) {
            images[i++] = UnmanagedImage.FromManagedImage(image);
          }
        }
      } catch (OperationCanceledException e) {
        throw new OperationCanceledException("Loading light-field images canceled.", e);
      } catch (Exception e) {
        throw new FileLoadException("Error while fetching images", e);
      }

      return images;
    }



    /// <summary>
    ///   Saves a xml of the light field to the given location
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public void Save(string path) {
      var paths = new List<string>(FileNames);
      ConvertToRelative(path);
      FileStream stream = null;
      try {
        stream = new FileStream(path, FileMode.Create);
        using (var writer = XmlWriter.Create(stream)) {
          Ser.WriteObject(writer, this);
        }
      } finally {
        stream?.Dispose();
      }

      FileNames = paths.ToArray();
    }



    private void ConvertToRelative(string path) {
      if (string.IsNullOrEmpty(path)) {
        throw new ArgumentNullException(nameof(path));
      }

      var uri = new Uri(path);

      for (var i = 0; i < FileNames.Length; i++) {
        FileNames[i] = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(FileNames[i])).ToString())
                          .Replace('/', Path.DirectorySeparatorChar);
      }
    }



    private void ConvertToAbsolute(string path) {
      if (string.IsNullOrEmpty(path)) {
        throw new ArgumentNullException(nameof(path));
      }

      var uri = new Uri(path);
      for (var i = 0; i < FileNames.Length; i++) {
        FileNames[i] = (new Uri(uri, FileNames[i].Replace('/', Path.DirectorySeparatorChar))).LocalPath;
      }
    }

    #endregion

    #region get images

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsInBounds(int x, int y) {
      return x >= 0 && x < ViewColumns && y >= 0 && y < ViewRows;
    }



    [CanBeNull]
    protected abstract UnmanagedImage GetImage(int i);



    public UnmanagedImage GetUnmanagedImage(int x, int y) {
      return DoGetUnmanImage(x, y)?.Clone();
    }



    public UnmanagedImage GetUnmanagedImage(Point viewpoint) {
      return GetUnmanagedImage(viewpoint.X, viewpoint.Y);
    }



    private UnmanagedImage DoGetUnmanImage(int x, int y) {
      return IsInBounds(x, y) ? GetImage(y * ViewColumns + x) : null;
    }



    [CanBeNull]
    public Bitmap GetImage(int x, int y) {
      return DoGetUnmanImage(x, y)?.ToManagedImage();
    }



    /// <summary>
    ///   Return the image with the central view.
    /// </summary>
    /// <returns></returns>
    [NotNull]
    public Bitmap GetViewImage() {
      var result = GetImage(ViewPoint);
      if (result == null) {
        throw new Exception(
          "There is some bug in " + GetType().Name + ". Light field not returns there central view image."
        );
      }

      return result;
    }



    /// <summary>
    ///   Return the image with the central view.
    /// </summary>
    /// <returns></returns>
    [NotNull]
    public UnmanagedImage GetViewUnmanagedImage() {
      var result = GetUnmanagedImage(ViewPoint);
      if (result == null) {
        throw new Exception(
          "There is some bug in " + GetType().Name + ". Light field not returns there central view image."
        );
      }

      return result;
    }



    [CanBeNull]
    public Bitmap GetImage(Point index) {
      return GetImage(index.X, index.Y);
    }



    private static unsafe void IntegrateImage(UnmanagedImage srcImg,
                                              float[] sumData,
                                              float xFocus,
                                              float yFocus,
                                              float norm) {
      var srcFactorX0 = (float)Math.Floor(xFocus) + 1 - xFocus;
      var srcFactorY0 = (float)Math.Floor(yFocus) + 1 - yFocus;

      var src00 = srcFactorX0 * srcFactorY0 * norm;
      var src10 = (1 - srcFactorX0) * srcFactorY0 * norm;
      var src01 = srcFactorX0 * (1 - srcFactorY0) * norm;
      var src11 = (1 - srcFactorX0) * (1 - srcFactorY0) * norm;

      var pixelBytes = srcImg.GetPixelFormatSizeInBytes();
      var stride = srcImg.Stride;

      var d = (byte*)srcImg.ImageData;

      var desXMin = Math.Max(-(int)Math.Ceiling(xFocus) + 1, 0);
      var desYMin = Math.Max(-(int)Math.Ceiling(yFocus) + 1, 0);
      var desXMax = (int)Math.Ceiling(srcImg.Width - Math.Max(xFocus, -1) - 1);
      var desYMax = (int)Math.Ceiling(srcImg.Height - Math.Max(yFocus, -1) - 1);
      var desIMin = desXMin * pixelBytes;
      var desIMax = desXMax * pixelBytes;

      var srcI = (int)Math.Floor(yFocus) * stride + (int)Math.Floor(xFocus) * pixelBytes;

      Parallel.For(
        desYMin,
        desYMax,
        Preferences.PoOne,
        y => {
          int iEx;
          var i = (iEx = y * stride) + desIMin;
          iEx += desIMax;
          for (; i < iEx; i += pixelBytes) {
            var srcI0 = i + srcI;
            var srcI1 = srcI0 + pixelBytes;
            var srcI2 = srcI0 + stride;
            var srcI3 = srcI2 + pixelBytes;

            for (var bit = 0; bit < pixelBytes; bit++) {
              sumData[i + bit] += src00 * d[srcI0 + bit] +
                                  src10 * d[srcI1 + bit] +
                                  src01 * d[srcI2 + bit] +
                                  src11 * d[srcI3 + bit];
            }
          }
        }
      );
    }



    /// <summary>
    ///   Codes a image using the light field based on the given parameters.
    /// </summary>
    /// <param name="parameter">
    ///   <see cref="LightfieldParam" />
    /// </param>
    /// <returns></returns>
    public unsafe UnmanagedImage GetUnmanagedImage(LightfieldParam parameter) {
      var lightFieldSize = ViewRows * ViewColumns;
      if (lightFieldSize == 0) {
        return null;
      }

      var doField = Math.Min(MaximumDepthOfField(parameter.ViewPoint), parameter.DepthOfField);
      var focusPoint = parameter.ViewPoint;
      if (focusPoint == default(Point)) {
        focusPoint = ViewPoint;
      }

      var doFocus = parameter.DepthOfFocus;
      var x0Focus = Math.Min(Math.Max(focusPoint.X, doField), ViewColumns - doField - 1);
      var y0Focus = Math.Min(Math.Max(focusPoint.Y, doField), ViewRows - doField - 1);

      var nLightFields = MathCV.Pow2(doField + doField + 1);
      var invNormLightFields = 1 / (float)nLightFields;

      var img = DoGetUnmanImage(x0Focus, y0Focus);
      if (img == null) {
        return null;
      }

      var format = img.PixelFormat;

      var h = img.Height;
      var s = img.Stride;
      var n = s * h;
      var dt = (byte*)img.ImageData;
      Preferences.Supported.CheckFormat(format);

      var dxFocus = doFocus;
      var dyFocus = parameter.InvertFocus ? -doFocus : doFocus;

      var sumData = new float[img.Stride * h];
      for (var i = 0; i < sumData.Length; i++) {
        sumData[i] = dt[i] * invNormLightFields;
      }

      var result = UnmanagedImage.Create(img.Width, img.Height, format);
      dt = (byte*)result.ImageData;

      var xMax = x0Focus + doField;
      var yMin = y0Focus - doField;
      var yMax = y0Focus + doField;

      for (var x = x0Focus - doField; x <= xMax; x++) {
        for (var y = yMin; y <= yMax; y++) {
          if (x == x0Focus &&
              y == y0Focus) {
            continue;
          }

          var view = DoGetUnmanImage(x, y);
          if (view == null) {
            return null;
          }

          if (view.PixelFormat != format) {
            throw new BadImageFormatException(nameof(view));
          }

          try {
            IntegrateImage(
              view,
              sumData,
              dxFocus * (x - x0Focus),
              dyFocus * (y - y0Focus),
              invNormLightFields
            );
          } catch (OperationCanceledException) {
            return null;
          }
        }
      }

      for (var i = 0; i < n; i++) {
        dt[i] = sumData[i].ClampToByte();
      }

      //      throw new NotSupportedException("Not tested use of (float).LimitToByte(), or is (float).ToBase256() correct?");
      return result;
    }



    public Bitmap GetImage(LightfieldParam parameter) {
      return GetUnmanagedImage(parameter).ToManagedAndDisposeThis();
    }

    #endregion

    #region properties

    public int MaximumDepthOfField(Point view) {
      var result = Math.Min(
        MathCV.Min(
          view.X,
          Math.Abs(ViewColumns - view.X),
          view.Y,
          Math.Abs(ViewRows - view.Y)
        ),
        DEPTH_OF_FIELD_MAX
      );
      Debug.Assert(result >= 0, "There are some blurred image");
      return result;
    }



    public int GetNumOfSetups(Point view) {
      return (MaximumDepthOfField(view) + 1 - MinimumDepthOfField) * DEPTH_OF_FOCUS_STEPS;
    }

    #endregion

    #region for each

    public void ForEachSetup(Action<UnmanagedImage> action) {
      ForEachSetup(ViewPoint, DEFAULT_INVERT_FOCUS, null, action);
    }



    public void ForEachSetup(Point view, Action<UnmanagedImage> action) {
      ForEachSetup(view, DEFAULT_INVERT_FOCUS, null, action);
    }



    public void ForEachSetup(Point view, bool invertFocus, Action<UnmanagedImage> action) {
      ForEachSetup(view, invertFocus, null, action);
    }



    public void ForEachSetup(Point view, ParallelOptions po, Action<UnmanagedImage> action) {
      ForEachSetup(view, DEFAULT_INVERT_FOCUS, po, action);
    }



    public void ForEachSetup(bool invertFocus, Action<UnmanagedImage> action) {
      ForEachSetup(ViewPoint, invertFocus, null, action);
    }



    public void ForEachSetup(bool invertFocus, ParallelOptions po, Action<UnmanagedImage> action) {
      ForEachSetup(ViewPoint, invertFocus, po, action);
    }



    public void ForEachSetup(ParallelOptions po, Action<UnmanagedImage> action) {
      ForEachSetup(ViewPoint, DEFAULT_INVERT_FOCUS, po, action);
    }



    public void ForEachSetup(Point view, bool invertFocus, ParallelOptions po, Action<UnmanagedImage> action) {
      if (Images == null) {
        throw new InvalidOperationException("Light field not loaded.");
      }

      //var ar = new string[MaximumDepthOfField(view)+1-DEPTH_OF_FIELD_MIN, DEPTH_OF_FOCUS_STEPS];
      var n = (MaximumDepthOfField(view) + 1 - DEPTH_OF_FIELD_MIN);
      var l = n * DEPTH_OF_FOCUS_STEPS;
      var lmax = l - 1;
      Parallel.For(
        0,
        l,
        po ?? Preferences.PoOne,
        i => {
          i = lmax - i;
          var p = new LightfieldParam {
            ViewPoint = view,
            DepthOfField = i % n + DEPTH_OF_FIELD_MIN,
            DepthOfFocus = (i / n) * DEPTH_OF_FOCUS_STEP + DEPTH_OF_FOCUS_MIN,
            InvertFocus = invertFocus
          };

          var img = GetUnmanagedImage(p);
          action(img);
        }
      );
    }



    public void ForEachSetup(Point view, Action<UnmanagedImage, FocusSetup> action) {
      ForEachSetup(view, DEFAULT_INVERT_FOCUS, null, action);
    }



    public void ForEachSetup(Point view, bool invertFocus, Action<UnmanagedImage, FocusSetup> action) {
      ForEachSetup(view, invertFocus, null, action);
    }



    public void ForEachSetup(Point view, ParallelOptions po, Action<UnmanagedImage, FocusSetup> action) {
      ForEachSetup(view, DEFAULT_INVERT_FOCUS, po, action);
    }



    public void ForEachSetup(bool invertFocus, Action<UnmanagedImage, FocusSetup> action) {
      ForEachSetup(ViewPoint, invertFocus, null, action);
    }



    public void ForEachSetup(bool invertFocus, ParallelOptions po, Action<UnmanagedImage, FocusSetup> action) {
      ForEachSetup(ViewPoint, invertFocus, po, action);
    }



    public void ForEachSetup(ParallelOptions po, Action<UnmanagedImage, FocusSetup> action) {
      ForEachSetup(ViewPoint, DEFAULT_INVERT_FOCUS, po, action);
    }



    public void ForEachSetup(Point view,
                             bool invertFocus,
                             ParallelOptions po,
                             Action<UnmanagedImage, FocusSetup> action) {
      if (Images == null) {
        throw new InvalidOperationException("Lightfield not loaded.");
      }

      //var ar = new string[MaximumDepthOfField(view)+1-DEPTH_OF_FIELD_MIN, DEPTH_OF_FOCUS_STEPS];
      var n = MaximumDepthOfField(view) + 1 - DEPTH_OF_FIELD_MIN;
      var l = n * DEPTH_OF_FOCUS_STEPS;
      var lmax = l - 1;
      Parallel.For(
        0,
        l,
        po ?? Preferences.PoOne,
        i => {
          i = lmax - i;
          var p = new LightfieldParam {
            ViewPoint = view,
            DepthOfField = i % n + DEPTH_OF_FIELD_MIN,
            DepthOfFocus = (i / n) * DEPTH_OF_FOCUS_STEP + DEPTH_OF_FOCUS_MIN,
            InvertFocus = invertFocus
          };

          //ar[j/DEPTH_OF_FOCUS_STEPS, j%DEPTH_OF_FOCUS_STEPS] = $"Generate: i={j} --- {p.DepthOfField} --- {p.DepthOfFocus}";
          //Log.Debug($"Generate: i={j} --- {p.DepthOfField} --- {p.DepthOfFocus}");
          var img = GetUnmanagedImage(p);
          action(img, new FocusSetup {Id = i, DepthOfField = p.DepthOfField, DepthOfFocus = p.DepthOfFocus});
        }
      );
    }



    public void ForEachSetup(Point view,
                             bool invertFocus,
                             ParallelOptions po,
                             Action<UnmanagedImage, FocusSetup> action,
                             Predicate<FocusSetup> condition) {
      if (Images == null) {
        throw new InvalidOperationException("Light field not loaded.");
      }

      var n = MaximumDepthOfField(view) + 1 - DEPTH_OF_FIELD_MIN;
      var l = n * DEPTH_OF_FOCUS_STEPS;
      var lmax = l - 1;
      Parallel.For(
        0,
        l,
        po ?? Preferences.PoOne,
        i => {
          i = lmax - i;
          var field = i % n + DEPTH_OF_FIELD_MIN;
          var focus = (i / n) * DEPTH_OF_FOCUS_STEP + DEPTH_OF_FOCUS_MIN;

          var f = new FocusSetup {Id = i, DepthOfField = field, DepthOfFocus = focus};
          if (!condition(f)) {
            return;
          }

          var p = new LightfieldParam {
            ViewPoint = view, DepthOfField = field, DepthOfFocus = focus, InvertFocus = invertFocus
          };
          var img = GetUnmanagedImage(p);
          action(img, f);
        }
      );
    }

    #endregion



    // Dispose() calls Dispose(true)
    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }



    // NOTE: Leave out the finalizer altogether if this class doesn't 
    // own unmanaged resources itself, but leave the other methods
    // exactly as they are. 
    ~ALightField() {
      // Finalizer calls Dispose(false)
      Dispose(false);
    }



    // The bulk of the clean-up code is implemented in Dispose(bool)
    protected virtual void Dispose(bool disposing) {
      var images = Images;
      if (images == null) {
        return;
      }

      Images = null;
      foreach (var image in images) {
        image.Dispose();
      }
    }
  }
}
