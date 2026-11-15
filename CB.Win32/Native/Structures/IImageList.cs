namespace CB.Win32.Native.Structures;

using global::System;
using global::System.Runtime.InteropServices;

/// <summary>
///   ImageList COM Interop (XP)
/// </summary>
[ComImport]
[Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IImageList {
  [PreserveSig]
  public int Add(
    IntPtr hbmImage,
    IntPtr hbmMask,
    ref int pi);



  [PreserveSig]
  public int ReplaceIcon(
    int i,
    IntPtr hicon,
    ref int pi);



  [PreserveSig]
  public int SetOverlayImage(
    int iImage,
    int iOverlay);



  [PreserveSig]
  public int Replace(
    int i,
    IntPtr hbmImage,
    IntPtr hbmMask);



  [PreserveSig]
  public int AddMasked(
    IntPtr hbmImage,
    int crMask,
    ref int pi);



  [PreserveSig]
  public int Draw(
    ref ImageListDrawParams pimldp);



  [PreserveSig]
  public int Remove(
    int i);



  [PreserveSig]
  public int GetIcon(
    int i,
    ImageListDrawItemConstants flags,
    ref IntPtr picon);



  [PreserveSig]
  public int GetImageInfo(
    int i,
    ref ImageInfo pImageInfo);



  [PreserveSig]
  public int Copy(
    int iDst,
    IImageList punkSrc,
    int iSrc,
    int uFlags);



  [PreserveSig]
  public int Merge(
    int i1,
    IImageList punk2,
    int i2,
    int dx,
    int dy,
    ref Guid riid,
    ref IntPtr ppv);



  [PreserveSig]
  public int Clone(
    ref Guid riid,
    ref IntPtr ppv);



  [PreserveSig]
  public int GetImageRect(
    int i,
    ref Rect prc);



  [PreserveSig]
  public int GetIconSize(
    ref int cx,
    ref int cy);



  [PreserveSig]
  public int SetIconSize(
    int cx,
    int cy);



  [PreserveSig]
  public int GetImageCount(
    ref int pi);



  [PreserveSig]
  public int SetImageCount(
    int uNewCount);



  [PreserveSig]
  public int SetBkColor(
    int clrBk,
    ref int pclr);



  [PreserveSig]
  public int GetBkColor(
    ref int pclr);



  [PreserveSig]
  public int BeginDrag(
    int iTrack,
    int dxHotspot,
    int dyHotspot);



  [PreserveSig]
  public int EndDrag();



  [PreserveSig]
  public int DragEnter(
    IntPtr hwndLock,
    int x,
    int y);



  [PreserveSig]
  public int DragLeave(
    IntPtr hwndLock);



  [PreserveSig]
  public int DragMove(
    int x,
    int y);



  [PreserveSig]
  public int SetDragCursorImage(
    ref IImageList punk,
    int iDrag,
    int dxHotspot,
    int dyHotspot);



  [PreserveSig]
  public int DragShowNolock(
    int fShow);



  [PreserveSig]
  public int GetDragImage(
    ref Point ppt,
    ref Point pptHotspot,
    ref Guid riid,
    ref IntPtr ppv);



  [PreserveSig]
  public int GetItemFlags(
    int i,
    ref int dwFlags);



  [PreserveSig]
  public int GetOverlayImage(
    int iOverlay,
    ref int piIndex);
}
