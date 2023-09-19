﻿using System;
using System.Windows;
using System.Windows.Threading;



namespace CB.WPF.Windows {
  public static class UiElementX {
    private static readonly Action EmptyDelegate = delegate { };



    public static void Refresh(this UIElement uiElement)
      => uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
  }
}
