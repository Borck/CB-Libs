using System.Drawing.Imaging;
using Accord.Imaging;



namespace CB.CV.Imaging {
  public static class ComplexImageX {
    #region type conversions

    public static UnmanagedImage ToUnmanagedImage(this ComplexImage thisImage) {
      //TODO: extract DoDraw out of Draw(ComplexImage, UnmanagedImage), with no checks
      var result = UnmanagedImage.Create(thisImage.Width, thisImage.Height, PixelFormat.Format24bppRgb);
      result.Draw(thisImage);
      return result;
    }

    #endregion
  }
}
