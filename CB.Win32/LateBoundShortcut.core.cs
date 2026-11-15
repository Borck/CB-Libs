namespace CB.Win32;

using IWshRuntimeLibrary;



/// <summary>An <see cref="IWshShortcut" /> over late-bound COM (<c>WScript.Shell</c>): no type library import, so the project builds on every operating system; it runs on Windows only.</summary>
internal sealed class LateBoundShortcut(object link) : IWshShortcut {
  private readonly dynamic _link = link;

  public string FullName => _link.FullName;

  public string Arguments {
    get => _link.Arguments;
    set => _link.Arguments = value;
  }

  public string Description {
    get => _link.Description;
    set => _link.Description = value;
  }

  public string Hotkey {
    get => _link.Hotkey;
    set => _link.Hotkey = value;
  }

  public string IconLocation {
    get => _link.IconLocation;
    set => _link.IconLocation = value;
  }

  public string RelativePath {
    set => _link.RelativePath = value;
  }

  public string TargetPath {
    get => _link.TargetPath;
    set => _link.TargetPath = value;
  }

  public int WindowStyle {
    get => _link.WindowStyle;
    set => _link.WindowStyle = value;
  }

  public string WorkingDirectory {
    get => _link.WorkingDirectory;
    set => _link.WorkingDirectory = value;
  }



  public void Load(string pathLink) => _link.Load(pathLink);



  public void Save() => _link.Save();
}
