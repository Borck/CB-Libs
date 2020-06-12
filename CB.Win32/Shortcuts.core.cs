using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;



namespace CB.Win32 {
  public static class Shortcuts {
    public const string EXTENSION = ".lnk";
    private const string FILE_PATTERN_SHORTCUT = "*" + EXTENSION;

    public static readonly string PathStartMenuLocalMachine =
      Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) + @"\Programs";

    public static readonly string PathStartMenuCurrentUser =
      Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\Programs";



    /// <summary>
    ///   Loaded a shortcut file or creates it, if not exists. Note: Created shortcuts must be saved
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static IWshShortcut CreateShortcut(string filename)
      => new WshShell().CreateShortcut(filename) as IWshShortcut;



    public static IEnumerable<IWshShortcut> GetStartMenuShortcuts() {
      return (from startMenu in new[] {PathStartMenuCurrentUser, PathStartMenuLocalMachine}
              from filename in Directory.GetFiles(startMenu, FILE_PATTERN_SHORTCUT, SearchOption.AllDirectories)
              where Path.GetExtension(filename) == EXTENSION
              select CreateShortcut(filename));
    }
  }
}
