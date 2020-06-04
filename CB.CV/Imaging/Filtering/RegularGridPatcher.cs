using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using Accord.Imaging;



namespace CB.CV.Imaging.Filtering {
  /// <summary>
  ///   This class extracts a regular patch grid for a image and can remerge the patches to an image.
  /// </summary>
  public class RegularGridPatcher : IPatcher {
    public readonly int PatchWidth;
    public readonly int PatchHeight;
    public readonly double Overlap;



    public RegularGridPatcher(int patchWidth, int patchHeight, double overlap = 0) {
      if (overlap >= 1 ||
          overlap < 0) {
        throw new ArgumentOutOfRangeException(nameof(overlap) + "has to be in the interval [0,1)");
      }

      if (patchWidth <= 0 ||
          patchHeight <= 0) {
        throw new ArgumentOutOfRangeException(
          (patchWidth <= 0 ? nameof(patchWidth) : nameof(patchHeight)) + "has to be in the interval [0,infinit)"
        );
      }

      PatchWidth = patchWidth;
      PatchHeight = patchHeight;
      Overlap = overlap;
    }



    public Rectangle[] ProcessPatches(UnmanagedImage image) {
      var horPatches = GetNumPatches(image.Width, PatchWidth);
      var verPatches = GetNumPatches(image.Height, PatchHeight);
      var dx = PatchWidth - GetRight(PatchWidth);
      var dy = PatchHeight - GetRight(PatchHeight);
      var x0 = GetOffset(image.Width, PatchWidth);
      var y0 = GetOffset(image.Height, PatchHeight);
      var xMax = horPatches * dx - x0;
      var yMax = verPatches * dy - y0;

      var patches = new Rectangle[horPatches * verPatches];
      if (patches.Length == 0) {
        return patches;
      }

      var k = 0;
      for (var y = y0;
           y < yMax;
           y += dy) {
        for (var x = x0; x < xMax; x += dx) {
          patches[k++] = new Rectangle(x, y, PatchWidth, PatchHeight);
        }
      }

      Debug.WriteLine(patches.ToString());
      return patches;
    }



    /// <summary>
    ///   Calculates the nummer of patches in one dimension, which fit into the length of l by taking int account of patch
    ///   length and overlapping (given by the patcher)
    /// </summary>
    /// <param name="l">length of the carrier in one dimension</param>
    /// <param name="lPatch">patch length in the same dimension</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetNumPatches(int l, int lPatch) {
      return (l - GetRight(lPatch)) / (lPatch - GetRight(lPatch));
    }



    /// <summary>
    ///   returns the offset between two patches in one dimension
    /// </summary>
    /// <param name="lPatch">length of patch</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int GetRight(int lPatch) {
      return (int)(lPatch * Overlap);
    }



    /// <summary>
    ///   Returns the position offset of the first patch for on dimension. It is zero, if l is n times of lPatch
    /// </summary>
    /// <param name="l">length of the carrier in one dimension</param>
    /// <param name="lPatch">patch length in the same dimension</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int GetOffset(int l, int lPatch) {
      if (lPatch <= 0) {
        throw new ArgumentException($"{nameof(lPatch)} is less then one");
      }

      var right = GetRight(lPatch);
      var left = lPatch - right;
      return (l - (l - right) / left * left - right) >> 1;
    }



    public void MergePatches(UnmanagedImage backImage, UnmanagedImage[] patchData, Rectangle[] patchBounds) {
      if (patchData.Length != patchBounds.Length) {
        throw new ArgumentException($"{nameof(patchData)} and {patchBounds} not in the same length");
      }

      for (var i = 0; i < patchData.Length; i++) {
        backImage.Draw(patchData[i], patchBounds[i].X, patchBounds[i].Y);
      }
    }
  }
}
