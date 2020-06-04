using System;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using CB.CV.Imaging;



namespace CB.CV {
  public static class Preferences {
    #region lightfield

    public static readonly bool LfTestImage = true;
    public const int LF_TEST_DO_FIELD = 3;
    public const float LF_TEST_DO_FOCUS = 0; //.5
    public const bool FOCUS_INVERTED = true;

    #endregion

    #region leave one in/out

    public static readonly bool LooEnabled = false;
    public static readonly bool LoiEnabled = true;

    public static readonly bool UseCsp = true;
    public static bool UseHog = false;
    public static bool EnableFilterRotation = false;
    public const int PATCH_SIZE = 64;
    public const bool MATCHING_CHANNELSEP = true;

    #endregion

    #region imaging

    public const int PATCH_WIDTH = PATCH_SIZE;
    public const int PATCH_HEIGHT = PATCH_SIZE;

    public static readonly ImageCheck Supported = new ImageCheck {
      PixelFormat.Format24bppRgb, PixelFormat.Format32bppArgb
    };

    #endregion

    #region color channels

    public static bool UseLabColorSpace = true;

    #endregion

    #region filtering

    public const double DIVIDE_BY_ZERO_EPSILON = .00001;
    public static bool UseBasicAngles = true; //90 180 270 360 degrees
    public static bool NormalizeRotatedFilters = false;
    public static bool FilterWindowing = false;

    #endregion

    #region patcher

    // used for deblurring image
    public const double CSP_MIN = .05;
    public const double CSP_MAX = .4;
    public const double LSS_SIGMA = .53;

    #endregion

    #region histograms

    /// <summary>
    ///   Change of angle (in degree) between two bins in weighted gradient argument histogram (wgah).
    /// </summary>
    public const int WGAH_BINS = 32;

    public const int HOI_BINS = 256;
    public const double WGAH_MAX_ANGLE = 360;

    #endregion

    #region lookup table

    public const int LUT_PART_SIZE = 8000; // size of the separated byte arrays for the database
    public const string LUT_DB_INSTANCE = "DeblurUsingAnalyseOfLightfields";
    public const string LUT_DB_NAME = "duallut";

    /// <summary>
    ///   Connection timeout in seconds
    /// </summary>
    public const int LUT_SQL_TIMEOUT = 0;

    /// <summary>
    ///   Batch size used for bulk coping the rows of the update
    /// </summary>
    public const int LUT_UPDATE_BATCH_SIZE = 5000;

    /// <summary>
    ///   Batch size used for fast clearing the lut database
    /// </summary>
    public const int LUT_CLEAR_BATCH_SIZE = 100000;

    #endregion

    #region parallelism

    //private static CancellationToken CancelToken => ServiceRegistry.AsyncService.Ct;
    private static int _maxDegreeOfParallels = Environment.ProcessorCount;

    public static int MaxDegreeOfParallelism {
      get { return _maxDegreeOfParallels; }
      set {
        if (value < 1) {
          throw new ArgumentOutOfRangeException(nameof(value), @"The degree should be at least 1.");
        }

        Po = new ParallelOptions {MaxDegreeOfParallelism = _maxDegreeOfParallels = value};
      }
    }

    public static ParallelOptions PoOne => new ParallelOptions {MaxDegreeOfParallelism = 1};

    public static ParallelOptions PoAll => new ParallelOptions {MaxDegreeOfParallelism = _maxDegreeOfParallels};

    internal static ParallelOptions Po = new ParallelOptions {MaxDegreeOfParallelism = _maxDegreeOfParallels,};

    #endregion
  }
}
