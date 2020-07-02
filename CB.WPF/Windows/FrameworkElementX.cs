using System.Windows;



namespace CB.WPF.Windows {
  public static class FrameworkElementX {
    public static bool Intersects(this FrameworkElement el1, FrameworkElement el2) {
      var bounds = el1.TransformToAncestor(el2)
                      .TransformBounds(
                        new Rect(0.0, 0.0, el1.ActualWidth, el1.ActualHeight)
                      );
      var rect = new Rect(0.0, 0.0, el2.ActualWidth, el2.ActualHeight);
      return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
    }
  }
}
