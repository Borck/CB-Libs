using System;
using System.Text;
using CB.System;
using CB.Win32.Native;
using CB.Win32.Native.Structures;
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
      var sb = new StringBuilder(2048);
      var hMod = Kernel32.LoadLibraryEx(
        resource,
        IntPtr.Zero,
        LoadLibraryExFlags.DontResolveDllReferences | LoadLibraryExFlags.LoadLibraryAsDatafile
      );

      var res = User32.LoadString(hMod, id, sb, sb.Capacity) == 0
                  ? sb.ToString()
                  : default;

      Kernel32.FreeLibrary(hMod);

      return res;
    }
  }
}
