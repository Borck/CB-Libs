using System;
using System.Runtime.InteropServices;
using CB.Win32.Native.Structures;



namespace CB.Win32.Native {
  public static partial class Shell32 {
    private const string IID_IImageList = "46EB5926-582E-4017-9FDF-E8998DAA0950";
    private const string IID_IImageList2 = "192B9D83-50FC-457B-90A0-2B82A8B5DAE1";

    public static readonly Guid IidImageList = new Guid(IID_IImageList);
    public static readonly Guid IidImageList2 = new Guid(IID_IImageList2);


    #region Public Enumerations

    #endregion



    /// <summary>
    ///   https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shgetimagelist
    ///   SHGetImageList is not exported correctly in XP.  See KB316931
    ///   http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316931
    ///   Apparently (and hopefully) ordinal 727 isn't going to change.
    /// </summary>
    /// <param name="iImageList"></param>
    /// <param name="riid">Reference to the image list interface identifier, normally IID_IImageList.</param>
    /// <param name="ppv"></param>
    /// <returns></returns>
    [DllImport("shell32", EntryPoint = "#727")]
    public static extern int SHGetImageList(SysImageListSize iImageList, ref Guid riid, ref IImageList ppv);



    [DllImport("shell32.dll", EntryPoint = "#727")]
    public static extern int SHGetImageListHandle(SysImageListSize iImageList, ref Guid riid, ref IntPtr handle);



    [DllImport("shell32")]
    public static extern uint SHGetIDListFromObject([MarshalAs(UnmanagedType.IUnknown)] object iUnknown,
                                                    out IntPtr ppidl);



    [DllImport("shell32")]
    public static extern IntPtr SHGetFileInfo(
      string pszPath,
      uint dwFileAttributes,
      ref ShFileInfo psfi,
      uint cbFileInfo,
      uint uFlags
    );
  }
}
