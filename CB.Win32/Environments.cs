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
      var (file, idString) = resource.SeparateLast(',');
      file = Environment.ExpandEnvironmentVariables(file);
      var id = idString != default
                 ? Math.Abs(int.Parse(idString))
                 : 0;
      return GetStringFromResource(file, id);
    }



    /// <summary>
    ///   Loads localized string from resource files like libraries (*.dll)
    /// </summary>
    /// <param name="resource">resource file</param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetLocalizedName(string resource, int id) => GetStringFromResource(resource, id);



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
