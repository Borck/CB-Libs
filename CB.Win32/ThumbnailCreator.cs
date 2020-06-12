using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using CB.System;
using CB.Win32.Native.Structures;



namespace CB.Win32 {
  /// <summary>
  ///   <seealso cref="https://stackoverflow.com/questions/28525925/get-icon-128128-file-type-c-sharp" />
  /// </summary>
  public class ThumbnailCreator {
    private readonly Dictionary<string, TargetType> _fileExtToTargetType =
      new Dictionary<string, TargetType> {
        {".jpg", TargetType.Image},
        {".jpeg", TargetType.Image},
        {".png", TargetType.Image},
        {".gif", TargetType.Image},
        {".exe", TargetType.ResourceFile},
        {".lnk", TargetType.ResourceFile},
      };

    private const string IMAGE_FILTER = ".jpg,.jpeg,.png,.gif";
    private const string EXE_FILTER = ".exe,.lnk";

    private static readonly SysImageListSize SysImageJumboSize = Environment.OSVersion.Version.Major >= 6
                                                                   ? SysImageListSize.JUMBO
                                                                   : SysImageListSize.EXTRALARGE;

    private static readonly Dictionary<string, Bitmap> IconDic = new Dictionary<string, Bitmap>();
    private static readonly Dictionary<string, Bitmap> ThumbDic = new Dictionary<string, Bitmap>();
    private static readonly SysImageList ImgList = new SysImageList(SysImageListSize.JUMBO);
    public int DefaultSize { get; set; }



    public enum IconSize {
      SMALL, LARGE, EXTRA_LARGE, JUMBO, THUMBNAIL
    }



    private class ThumbnailInfo {
      public readonly IconSize IconSize;
      public readonly Bitmap Bitmap;
      public readonly string FullPath;



      public ThumbnailInfo(Bitmap b, string path, IconSize size) {
        Bitmap = b;
        FullPath = path;
        IconSize = size;
      }
    }



    #region Win32api

    /// <summary>
    ///   Return large file icon of the specified file.
    /// </summary>
    internal static Icon SHGetFileIcon(string fileName, IconSize size) {
      var win32Size = ToWin32IconSize(size);
      string indexString;
      (fileName, indexString) = fileName.SeparateLast(',');
      return indexString != default
               ? Icons.ExtractIcon(fileName, int.Parse(indexString), win32Size)
               : Icons.ExtractIcon(fileName, win32Size);
    }

    #endregion

    #region Static Tools

    private static Win32.IconSize ToWin32IconSize(IconSize size) {
      switch (size) {
        case IconSize.SMALL:
          return Win32.IconSize.Small;
        case IconSize.LARGE:
          return Win32.IconSize.Large;
        case IconSize.EXTRA_LARGE:
          return Win32.IconSize.Extralarge;
        case IconSize.JUMBO:
        case IconSize.THUMBNAIL:
          return Win32.IconSize.Jumbo;
        default:
          throw new ArgumentOutOfRangeException(nameof(size), size, null);
      }
    }



    public static Size GetDefaultSize(IconSize size) {
      switch (size) {
        case IconSize.JUMBO:
          return new Size(256, 256);
        case IconSize.THUMBNAIL:
          return new Size(256, 256);
        case IconSize.EXTRA_LARGE:
          return new Size(48, 48);
        case IconSize.LARGE:
          return new Size(32, 32);
        default:
          return new Size(16, 16);
      }
    }



    /// <summary>
    ///   Create thumbnail image
    /// </summary>
    /// <param name="imgToResize"></param>
    /// <param name="size"></param>
    /// <param name="spacing"></param>
    /// <returns></returns>
    //http://blog.paranoidferret.com/?p=11 , modified a little.
    private static Bitmap ResizeImage(Bitmap imgToResize, Size size, int spacing) {
      var sourceWidth = imgToResize.Width;
      var sourceHeight = imgToResize.Height;

      var nPercentW = size.Width / (float)sourceWidth;
      var nPercentH = size.Height / (float)sourceHeight;

      var nPercent = nPercentH < nPercentW
                       ? nPercentH
                       : nPercentW;

      var destWidth = (int)(sourceWidth * nPercent - spacing * 4);
      var destHeight = (int)(sourceHeight * nPercent - spacing * 4);

      var leftOffset = (size.Width - destWidth) / 2;
      var topOffset = (size.Height - destHeight) / 2;

      var b = new Bitmap(size.Width, size.Height);
      using (var g = Graphics.FromImage(b)) {
        g.InterpolationMode = InterpolationMode.High;
        g.DrawLines(
          Pens.Silver,
          new[] {
            new PointF(leftOffset - spacing, topOffset + destHeight + spacing), //BottomLeft
            new PointF(leftOffset - spacing, topOffset - spacing), //TopLeft
            new PointF(leftOffset + destWidth + spacing, topOffset - spacing)
          }
        ); //TopRight

        g.DrawLines(
          Pens.Gray,
          new[] {
            new PointF(leftOffset + destWidth + spacing, topOffset - spacing), //TopRight
            new PointF(leftOffset + destWidth + spacing, topOffset + destHeight + spacing), //BottomRight
            new PointF(leftOffset - spacing, topOffset + destHeight + spacing),
          }
        ); //BottomLeft

        g.DrawImage(imgToResize, leftOffset, topOffset, destWidth, destHeight);
      }

      return b;
    }



    private static Bitmap ResizeJumbo(Bitmap imgToResize, Size size, int spacing) {
      var destWidth = 80;
      var destHeight = 80;

      var leftOffset = (size.Width - destWidth) / 2;
      var topOffset = (size.Height - destHeight) / 2;

      var b = new Bitmap(size.Width, size.Height);
      using (var g = Graphics.FromImage(b)) {
        g.InterpolationMode = InterpolationMode.High;
        g.DrawLines(
          Pens.Silver,
          new[] {
            new PointF(0 + spacing, size.Height - spacing), //BottomLeft
            new PointF(0 + spacing, 0 + spacing), //TopLeft
            new PointF(size.Width - spacing, 0 + spacing)
          }
        ); //TopRight

        g.DrawLines(
          Pens.Gray,
          new[] {
            new PointF(size.Width - spacing, 0 + spacing), //TopRight
            new PointF(size.Width - spacing, size.Height - spacing), //BottomRight
            new PointF(0 + spacing, size.Height - spacing)
          }
        ); //BottomLeft

        g.DrawImage(imgToResize, leftOffset, topOffset, destWidth, destHeight);
      }

      return b;
    }



    private static string CreateCacheKey(string fileName,
                                         IconSize size,
                                         TargetType type) {
      var key = string.Empty;
      switch (type) {
        case TargetType.ResourceFile:
        case TargetType.Folder:
          key += fileName.ToLower();
          break;
        case TargetType.Image:
          key += size == IconSize.THUMBNAIL
                   ? fileName.ToLower()
                   : Path.GetExtension(fileName)?.ToLower();
          break;
        default:
          key += Path.GetExtension(fileName)?.ToLower();
          break;
      }

      switch (size) {
        case IconSize.THUMBNAIL:
          key += type == TargetType.Image ? "+T" : "+J";
          break;
        case IconSize.JUMBO:
          key += "+J";
          break;
        case IconSize.EXTRA_LARGE:
          key += "+XL";
          break;
        case IconSize.LARGE:
          key += "+L";
          break;
        case IconSize.SMALL:
          key += "+S";
          break;
      }

      return key;
    }

    #endregion

    #region Static Cache

    private Bitmap LoadJumbo(string lookup, TargetType type) {
      ImgList.ImageListSize = SysImageJumboSize;
      var icon = ImgList.Icon(ImgList.IconIndex(lookup, type == TargetType.Folder));
      var bitmap = icon.ToBitmap();
      icon.Dispose();

      var empty = Color.FromArgb(0, 0, 0, 0);

      if (bitmap.Width < 256) {
        bitmap = ResizeImage(bitmap, new Size(256, 256), 0);
      } else if (bitmap.GetPixel(100, 100) == empty &&
                 bitmap.GetPixel(200, 200) == empty &&
                 bitmap.GetPixel(200, 200) == empty) {
        ImgList.ImageListSize = SysImageListSize.LARGE;
        bitmap = ResizeJumbo(ImgList.Icon(ImgList.IconIndex(lookup)).ToBitmap(), new Size(200, 200), 5);
      }

      return bitmap;
    }

    #endregion

    #region Instance Cache

    public void ClearInstanceCache() {
      ThumbDic.Clear();
      //System.GC.Collect();
    }



    private Bitmap GetOrCreateCachedImage(string fileName,
                                          IconSize size,
                                          TargetType type) {
      var cache = size == IconSize.THUMBNAIL ||
                  type == TargetType.ResourceFile
                    ? ThumbDic
                    : IconDic;
      var key = CreateCacheKey(fileName, size, type);
      lock (cache)
        if (!cache.ContainsKey(key)) {
          var image = GetOrLoadImage(fileName, size, type);
          cache.Add(key, image);
          return image;
        }

      return cache[key];
    }



    private static IconSize ToIconSize(int iconSize) {
      switch (iconSize) {
        case int _ when iconSize <= 16:
          return IconSize.SMALL;
        case int _ when iconSize <= 32:
          return IconSize.LARGE;
        case int _ when iconSize <= 48:
          return IconSize.EXTRA_LARGE;
        case int _ when iconSize <= 72:
          return IconSize.JUMBO;
        default:
          return IconSize.THUMBNAIL;
      }
    }



    private TargetType GetTargetType(string path, out object typedPath) {
      if (string.IsNullOrEmpty(path)) {
        typedPath = null;
        return TargetType.DefaultFile;
      }

      if (path.EndsWith("}")) {
        if (path.StartsWith("shell:::{")) {
          typedPath = new Guid(path.Substring(9, 36));
          return TargetType.GuidObject;
        }
      }

      if (path.EndsWith("\\")) {
        typedPath = default;
        return TargetType.Folder;
      }

      var attr = File.GetAttributes(path);
      if (attr.HasFlag(FileAttributes.Directory)) {
        typedPath = default;
        return TargetType.Folder;
      }

      typedPath = default;
      return _fileExtToTargetType.TryGetValue(Path.GetExtension(path), out var type)
               ? type
               : TargetType.GenericFile;
    }



    // private static bool IsImage(string fileName)
    //   => IsFile( fileName, IMAGE_FILTER );
    //
    // private static bool IsExecutable(string fileName)
    //   => IsFile( fileName, EXE_FILTER );
    //
    //
    // private static bool IsFile(string fileName, string extFilter) {
    //   var ext = Path.GetExtension( fileName )?.ToLower();
    //   return !string.IsNullOrEmpty( ext ) &&
    //          extFilter.IndexOf( ext, StringComparison.Ordinal ) != -1 &&
    //          File.Exists( fileName );
    // }
    //
    //
    //
    //
    // private static bool IsFolder(string path) {
    //   return path.EndsWith( "\\" ) || Directory.Exists( path );
    // }



    private static bool TryGetIconName(Guid guid, out string filename) {
      var regKeyGuid = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"CLSID\{guid:B}\DefaultIcon");
      filename = regKeyGuid?.GetValue(default) as string;
      return filename != default;
    }



    public Bitmap GetImage(Guid guid, IconSize iconSize) {
      return TryGetIconName(guid, out var fileName)
               ? GetOrCreateCachedImage(fileName, iconSize, TargetType.ResourceFile)
               : GetOrCreateCachedImage(default, iconSize, TargetType.DefaultFile);
    }



    public Bitmap GetImage(string fileName, IconSize iconSize) {
      var type = GetTargetType(fileName, out var typedPath);
      return type == TargetType.GuidObject
               ? GetImage((Guid)typedPath, iconSize)
               : GetOrCreateCachedImage(fileName, iconSize, type);
    }



    public Bitmap GetImage(string fileName, int iconSize)
      => GetImage(fileName, ToIconSize(iconSize));



    public Bitmap GetImage(string fileName)
      => GetImage(fileName, DefaultSize);

    #endregion

    #region Instance Tools

    private Bitmap GetOrLoadImage(string fileName,
                                  IconSize size,
                                  TargetType type) {
      // var key = CreateCacheKey(fileName, size, type);
      var lookup =
        // key.StartsWith( "." )
        // ? "aaa" + Path.GetExtension( fileName )?.ToLower()
        // :
        fileName;

      // if (type == TargetType.Executable) {
      //   var spacing = size == IconSize.JUMBO || size == IconSize.THUMBNAIL ? 5 : 0;
      //   using (var origBitmap = SHGetFileIcon(fileName, size).ToBitmap()) {
      //     return ResizeAndWriteThumbnail(origBitmap, size, spacing);
      //   }
      // }

      Icon icon;
      switch (size) {
        case IconSize.THUMBNAIL:
          if (type == TargetType.Image) {
            //Load as jumbo icon first.
            // var bitmap = GetOrCreateCachedImage(fileName, IconSize.JUMBO, type);

            //BitmapSource bitmapSource = addToDic(fileName, IconSize.jumbo) as BitmapSource;                            
            //WriteableBitmap bitmap = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Bgra32, null);
            //copyBitmap(bitmapSource, bitmap, false);
            // ThreadPool.QueueUserWorkItem(
            //   PollThumbnailCallback,
            //   new ThumbnailInfo(bitmap, fileName, size)
            // );

            //Non UIThread
            // var thumbInfo = (ThumbnailInfo)state;
            // var fileName = thumbInfo.FullPath;
            using (var origBitmap = new Bitmap(fileName)) {
              return ResizeJumbo(origBitmap, GetDefaultSize(size), 5);
            }
          } else {
            return GetOrLoadImage(lookup, IconSize.JUMBO, type);
          }
        case IconSize.JUMBO:

          return new Bitmap(LoadJumbo(lookup, type));
        case IconSize.EXTRA_LARGE:
          ImgList.ImageListSize = SysImageListSize.EXTRALARGE;
          icon = ImgList.Icon(ImgList.IconIndex(lookup, type == TargetType.Folder));
          return icon.ToBitmap();
        default:
          icon = SHGetFileIcon(lookup, size);
          return icon.ToBitmap();
      }
    }



    private enum TargetType {
      Folder,
      ResourceFile,
      Image,
      GuidObject,
      GenericFile,
      DefaultFile,
    }

    #endregion
  }
}
