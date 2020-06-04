using System;
using System.Drawing;
using System.Runtime.Serialization;
using Accord.Imaging;



namespace CB.CV.Imaging.LightField {
  [DataContract]
  public class CameraArrayLightField : ALightField {
    public const int DOWN_SCALED_WIDTH = 640;
    public const int DOWN_SCALED_HEIGHT = 480;

    #region instantiation

    public CameraArrayLightField(string[] fileNames)
      : base(fileNames) { }



    internal CameraArrayLightField(Bitmap[] images)
      : base(images) { }



    public CameraArrayLightField Create(Bitmap[] images) {
      return new CameraArrayLightField(images);
    }

    #endregion

    #region images

    public override void Load( /*Size scaledSize = default(Size)*/) {
      if (Fetched) {
        return;
      }

      var n = FileNames.Length;
      if (n < 2) {
        throw new InvalidImagePropertiesException("Camera array lightfield requires at least two images");
      }

      var images = DoFetchImages(FileNames);
      var w = images[0].Width;
      var h = images[0].Height;
      for (var i = 1; i < n; i++) {
        if (w != images[i].Width ||
            h != images[i].Height) {
          throw new InvalidImagePropertiesException("resolutions between images differ");
        }
      }

      ViewRows = n / (ViewColumns = (int)Math.Sqrt(n));
      Images = images;
    }



    protected override UnmanagedImage GetImage(int i) {
      return Images[i];
    }



    public void SetViewSize(int columns, int rows) {
      var n = FileNames.Length;
      columns = columns.Clamp(1, n);
      rows = rows.Clamp(1, n);

      var heightChanged = rows != ViewRows;

      while (true) {
        if (heightChanged) {
          columns = n / rows;
          if (columns * rows == n) {
            break;
          }

          rows += ViewRows < rows ? 1 : -1;
        } else {
          rows = n / columns;
          if (columns * rows == n) {
            break;
          }

          columns += ViewColumns < columns ? 1 : -1;
        }
      }

      ViewColumns = columns;
      ViewRows = rows;
    }



    public new UnmanagedImage GetUnmanagedImage(int x, int y) {
      return IsInBounds(x, y) ? Images[y * ViewColumns + x].Clone() : null; // make copy
    }

    #endregion
  }
}
