using System;
using System.Runtime.InteropServices;



namespace CB.Win32.Native {
  public static class User32 {
    [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
    public static extern int DestroyIcon(IntPtr hIcon);



    /// <summary>
    ///   Creates an array of handles to icons that are extracted from a specified file.
    ///   https://docs.microsoft.com/de-de/windows/win32/api/winuser/nf-winuser-privateextracticonsa
    /// </summary>
    /// <param name="libraryName">The path and name of the file from which the icon(s) are to be extracted.</param>
    /// <param name="iconIndex">
    ///   The zero-based index of the first icon to extract. For example, if this value is zero, the
    ///   function extracts the first icon in the specified file.
    /// </param>
    /// <param name="iconWidth">The horizontal icon size wanted. See Remarks.</param>
    /// <param name="iconHeight">The vertical icon size wanted. See Remarks.</param>
    /// <param name="iconHandles">A pointer to the returned array of icon handles.</param>
    /// <param name="iconId">
    ///   A pointer to a returned resource identifier for the icon that best fits the current display
    ///   device. The returned identifier is 0xFFFFFFFF if the identifier is not available for this format. The returned
    ///   identifier is 0 if the identifier cannot otherwise be obtained.
    /// </param>
    /// <param name="numberIcons">
    ///   The number of icons to extract from the file. This parameter is only valid when extracting
    ///   from .exe and .dll files.
    /// </param>
    /// <param name="flags">
    ///   Specifies flags that control this function. These flags are the LR_* flags used by the LoadImage
    ///   function.
    /// </param>
    /// <returns>
    ///   If the phiconparameter is NULL and this function succeeds, then the return value is the number of icons in the file.
    ///   If the function fails then the return value is 0.
    ///   If the phicon parameter is not NULL and the function succeeds, then the return value is the number of icons
    ///   extracted.Otherwise, the return value is 0xFFFFFFFF if the file is not found.
    /// </returns>
    [DllImport("User32.dll")]
    public static extern int PrivateExtractIcons(string libraryName,
                                                 int iconIndex,
                                                 int iconWidth,
                                                 int iconHeight,
                                                 out IntPtr iconHandles,
                                                 out IntPtr iconId,
                                                 int numberIcons,
                                                 IntPtr flags);
  }
}
