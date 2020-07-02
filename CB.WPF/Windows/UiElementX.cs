﻿using System;
using System.Windows;
using System.Windows.Threading;



namespace CB.WPF.Windows {
  public static class UiElementX {
    private static readonly Action _emptyDelegate = delegate { };



    public static void Refresh(this UIElement uiElement)
      => uiElement.Dispatcher.Invoke(DispatcherPriority.Render, _emptyDelegate);
  }
}
