﻿using CB.System;
using CB.Win32.Native;
using System;
using System.Runtime.InteropServices;
using System.Text;



namespace CB.Win32 {
  public static class Environments {
    private const string LOCALIZED_NAME_IMMUTABLE_POSTFIX = "#immutable1";



    /// <summary>
    ///   Loads localized string from resource files like libraries (*.dll)
    /// </summary>
    /// <param name="resource">resource file or comma separated string of resource file and string index</param>
    /// <returns></returns>
    public static string GetLocalizedName(string resource) {
      if (resource.StartsWith(
#if NETCOREAPP
        '@'
#else
        "@"
#endif
      )) {
        resource = resource.Substring(1);
      }

      if (resource.EndsWith(LOCALIZED_NAME_IMMUTABLE_POSTFIX)) {
        resource = resource.Substring(0, resource.Length - LOCALIZED_NAME_IMMUTABLE_POSTFIX.Length);
      }

      var (file, id) = resource.SeparateLast(
        ",-",
        StringComparison.InvariantCulture,
        idString => Math.Abs(int.Parse(idString))
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



    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr LoadLibrary(string lpFileName);



    /// <summary>
    ///   Loads string from resource files like libraries (*.dll)
    /// </summary>
    /// <param name="resource">resource file</param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetStringFromResource(string resource, int id) {
      var lib = LoadLibrary(resource);
      if (lib == IntPtr.Zero) {
        var errorCode = Marshal.GetLastWin32Error();
        throw new SystemException($"System failed to load library '{resource}' with error {errorCode}");
      }

      var result = new StringBuilder(2048);
      User32.LoadString(lib, id, result, result.Capacity);
      Kernel32.FreeLibrary(lib);
      return result.ToString();
    }
  }
}
