using System;
using System.Text;
using CB.System;
using CB.Win32.Native;
using JetBrains.Annotations;



namespace CB.Win32 {
  public static class Environments {
    /// <summary>
    ///   Loads localized string from resource files like libraries (*.dll)
    /// </summary>
    /// <param name="resource">resource file or comma separated string of resource file and string index</param>
    /// <returns></returns>
    public static string GetLocalizedName([NotNull] string resource) {
      if (resource.StartsWith('@')) {
        resource = resource.Length > 1
                     ? resource.Substring(1)
                     : string.Empty;
      }

      var (file, id) = resource.SeparateLast(
        ",-",
        idString => Math.Abs(int.Parse(idString)),
        StringComparison.InvariantCulture
      );
      return GetLocalizedName(file, id);
    }



    /// <summary>
    ///   Loads localized string from resource files like libraries (*.dll)
    /// </summary>
    /// <param name="resource">resource file</param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetLocalizedName(string resource, int id) => GetStringFromResource(
      Environment.ExpandEnvironmentVariables(resource),
      id
    );



    /// <summary>
    ///   Loads string from resource files like libraries (*.dll)
    /// </summary>
    /// <param name="resource">resource file</param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetStringFromResource(string resource, int id) {
      var lib = Kernel32.LoadLibrary(resource);
      var result = new StringBuilder(2048);
      User32.LoadString(lib, id, result, result.Capacity);
      Kernel32.FreeLibrary(lib);
      return result.ToString();
    }
  }
}
