using System.Drawing;
using Accord.Imaging;
using JetBrains.Annotations;



namespace CB.CV.Imaging.Filtering {
  public interface IPatcher {
    /// <summary>
    ///   Processes the bounds of the patches using the given image
    /// </summary>
    /// <param name="image"></param>
    /// <returns>patches</returns>
    Rectangle[] ProcessPatches([NotNull] UnmanagedImage image);



    void MergePatches([NotNull] UnmanagedImage backImage,
                      [NotNull] UnmanagedImage[] patchData,
                      [NotNull] Rectangle[] patchBounds);
  }
}
