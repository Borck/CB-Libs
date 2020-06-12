namespace CB.Win32 {
  /// <summary>
  ///   Valid size to extract icons with shell32
  ///   <seealso cref="https://docs.microsoft.com/de-de/windows/win32/api/shellapi/nf-shellapi-shgetimagelist" />
  /// </summary>
  public enum IconSize {
    /// <summary>
    ///   The image size is normally 32x32 pixels. However, if the Use large icons option is selected from the Effects section
    ///   of the Appearance tab in Display Properties, the image is 48x48 pixels.
    /// </summary>
    Large = 0x0,

    /// <summary>
    ///   These images are the Shell standard small icon size of 16x16, but the size can be customized by the user.
    /// </summary>
    Small = 0x1,

    /// <summary>
    ///   These images are the Shell standard extra-large icon size. This is typically 48x48, but the size can be customized by
    ///   the user.
    /// </summary>
    Extralarge = 0x2,

    /// <summary>
    ///   These images are the size specified by GetSystemMetrics called with SM_CXSMICON and GetSystemMetrics called with
    ///   SM_CYSMICON.
    /// </summary>
    Syssmall = 0x3,

    /// <summary>
    ///   Windows Vista and later. The image is normally 256x256 pixels.
    /// </summary>
    Jumbo = 0x4,
  }
}
