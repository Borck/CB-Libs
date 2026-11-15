// The namespace of the type library this interface used to come from, so that code written against 0.4.x (which referenced the
// generated interop assembly) compiles unchanged.
namespace IWshRuntimeLibrary;

/// <summary>A Windows shortcut (.lnk file): the members of the Windows Script Host's <c>IWshShortcut</c>. Changes are written by <see cref="Save" />.</summary>
/// <remarks>
///   Declared here, not generated: before 1.0 this was the interface of a type library imported with tlbimp, which only builds on
///   Windows with Visual Studio's MSBuild. The members and their names are the same.
/// </remarks>
public interface IWshShortcut {
  /// <summary>Full path of the .lnk file.</summary>
  public string FullName { get; }

  public string Arguments { get; set; }

  public string Description { get; set; }

  public string Hotkey { get; set; }

  /// <summary>Icon as "path,index".</summary>
  public string IconLocation { get; set; }

  public string RelativePath { set; }

  public string TargetPath { get; set; }

  /// <summary>1 normal, 3 maximized, 7 minimized.</summary>
  public int WindowStyle { get; set; }

  public string WorkingDirectory { get; set; }

  public void Load(string pathLink);

  public void Save();
}
