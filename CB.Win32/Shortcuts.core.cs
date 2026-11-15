namespace CB.Win32;

using global::System;
using global::System.Collections.Generic;
using global::System.IO;
using global::System.Linq;
using global::System.Runtime.InteropServices;

/// <summary>A Windows shortcut (.lnk file), read and written through the Windows Script Host.</summary>
/// <remarks>
///   Late-bound COM (<c>WScript.Shell</c>) instead of a generated interop assembly: that needs no type library import, so the
///   project builds on every operating system; it runs on Windows only. Changes are written by <see cref="Save" />.
/// </remarks>
public sealed class Shortcut {
  private readonly dynamic _link;



  internal Shortcut(dynamic link) => _link = link;



  /// <summary>Full path of the .lnk file.</summary>
  public string FullName => _link.FullName;

  public string TargetPath {
    get => _link.TargetPath;
    set => _link.TargetPath = value;
  }

  public string Arguments {
    get => _link.Arguments;
    set => _link.Arguments = value;
  }

  public string WorkingDirectory {
    get => _link.WorkingDirectory;
    set => _link.WorkingDirectory = value;
  }

  public string Description {
    get => _link.Description;
    set => _link.Description = value;
  }

  /// <summary>Icon as "path,index".</summary>
  public string IconLocation {
    get => _link.IconLocation;
    set => _link.IconLocation = value;
  }

  public string Hotkey {
    get => _link.Hotkey;
    set => _link.Hotkey = value;
  }

  /// <summary>1 normal, 3 maximized, 7 minimized.</summary>
  public int WindowStyle {
    get => _link.WindowStyle;
    set => _link.WindowStyle = value;
  }



  public void Save() => _link.Save();
}



public static class Shortcuts {
  public const string EXTENSION = ".lnk";
  private const string FILE_PATTERN_SHORTCUT = "*" + EXTENSION;

  public static readonly string PathStartMenuLocalMachine =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");

  public static readonly string PathStartMenuCurrentUser =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");



  /// <summary>Loads a shortcut file or creates it, if it does not exist. Note: a created shortcut must be saved.</summary>
  public static Shortcut CreateShortcut(string filename) {
    var shellType = Type.GetTypeFromProgID("WScript.Shell")
                    ?? throw new PlatformNotSupportedException("The Windows Script Host (WScript.Shell) is not available.");
    dynamic shell = Activator.CreateInstance(shellType)!;
    try {
      return new Shortcut(shell.CreateShortcut(filename));
    } finally {
      Marshal.FinalReleaseComObject(shell);
    }
  }



  public static IEnumerable<Shortcut> GetStartMenuShortcuts() =>
    from startMenu in new[] { PathStartMenuCurrentUser, PathStartMenuLocalMachine }
    where Directory.Exists(startMenu)
    from filename in Directory.GetFiles(startMenu, FILE_PATTERN_SHORTCUT, SearchOption.AllDirectories)
    select CreateShortcut(filename);
}
