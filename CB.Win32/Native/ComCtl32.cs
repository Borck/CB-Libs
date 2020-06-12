using System;
using System.Runtime.InteropServices;
using CB.Win32.Native.Structures;



namespace CB.Win32.Native {
  public static class ComCtl32 {
    [DllImport("comctl32")]
    public static extern int ImageList_Draw(
      IntPtr hIml,
      int i,
      IntPtr hdcDst,
      int x,
      int y,
      int fStyle);



    [DllImport("comctl32")]
    public static extern int ImageList_DrawIndirect(
      ref ImageListDrawParams pimldp);



    [DllImport("comctl32")]
    public static extern int ImageList_GetIconSize(
      IntPtr himl,
      ref int cx,
      ref int cy);



    [DllImport("comctl32")]
    public static extern IntPtr ImageList_GetIcon(
      IntPtr himl,
      int i,
      ImageListDrawItemConstants flags);
  }
}
