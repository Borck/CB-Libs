namespace CB.Win32;

using global::System;
using global::System.Collections.Generic;
using global::System.IO;
using global::System.Linq;
using global::System.Runtime.InteropServices;
using IWshRuntimeLibrary;



public static class Shortcuts {
  public const string EXTENSION = ".lnk";
  private const string FILE_PATTERN_SHORTCUT = "*" + EXTENSION;

  public static readonly string PathStartMenuLocalMachine =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");

  public static readonly string PathStartMenuCurrentUser =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");



  /// <summary>Loads a shortcut file or creates it, if it does not exist. Note: a created shortcut must be saved.</summary>
  public static IWshShortcut CreateShortcut(string filename) {
    var shellType = Type.GetTypeFromProgID("WScript.Shell")
                    ?? throw new PlatformNotSupportedException("The Windows Script Host (WScript.Shell) is not available.");
    dynamic shell = Activator.CreateInstance(shellType)
                    ?? throw new PlatformNotSupportedException("The Windows Script Host (WScript.Shell) could not be created.");
    try {
      return new LateBoundShortcut(shell.CreateShortcut(filename));
    } finally {
      Marshal.FinalReleaseComObject(shell);
    }
  }



  public static IEnumerable<IWshShortcut> GetStartMenuShortcuts() =>
    from startMenu in new[] { PathStartMenuCurrentUser, PathStartMenuLocalMachine }
    where Directory.Exists(startMenu)
    from filename in Directory.GetFiles(startMenu, FILE_PATTERN_SHORTCUT, SearchOption.AllDirectories)
    select CreateShortcut(filename);
}
