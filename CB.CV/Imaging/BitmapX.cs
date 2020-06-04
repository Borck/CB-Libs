using System.Drawing;
using System.Runtime.CompilerServices;
using Accord.Imaging;
using JetBrains.Annotations;



namespace CB.CV.Imaging {
  public static class BitmapX {
    [NotNull]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnmanagedImage ToUnmanaged(this Bitmap thisImage) {
      return UnmanagedImage.FromManagedImage(thisImage);
    }
  }
}
