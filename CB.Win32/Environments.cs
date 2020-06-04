using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;



namespace CB.Win32 {
  public static class Environmentt {
    //public enum AutostartLocation {Shortcut, HKCU, HKLM, Any};
    private const string FILE_PATTERN_APP = "*.exe";

    private const string RUN_LOCATION = @"Software\Microsoft\Windows\CurrentVersion\Run";


    //----------------------------------------
    // needed references
    //----------------------------------------
    // Microsoft Shell Controls and Automation
    // Windows Script Host Object Model


    internal const uint DONT_RESOLVE_DLL_REFERENCES = 0x00000001;
    internal const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;


    private const string REGFILES_SEARCHMASK = "*(+).reg;*({0}+).reg;*({0}.{1}+).reg";
    private const string REGISTRY_FILENAME = "regedit.exe";



    public static string GetOsVersionString(bool shortname = false) {
      var osInfo = Environment.OSVersion;
      var strVers = string.Empty;
      if (osInfo.Platform == PlatformID.Win32Windows) {
        // Windows 98 / 98SE oder Windows Me. Windows 95 unterstützt .NET nicht 
        if (osInfo.Version.Minor == 10)
          strVers = "Windows 98";
        if (osInfo.Version.Minor == 90)
          strVers = "Windows Me";
      }

      if (osInfo.Platform == PlatformID.Win32NT) {
        // Windows NT 4.0, 2000, XP oder Server 2003. Windows NT 3.51 unterstützt .NET nicht 
        switch (osInfo.Version.Major) {
          case 4: {
            strVers = "Windows NT 4.0";
            break;
          }
          case 5: {
            switch (osInfo.Version.Minor) {
              case 0:
                strVers = "Windows 2000";
                break;
              case 1:
                strVers = "Windows XP";
                break;
              case 2:
                strVers = "Windows Server 2003";
                break;
            }

            break;
          }
          case 6: {
            switch (osInfo.Version.Minor) {
              case 0:
                strVers = "Windows Vista";
                break;
              case 1:
                strVers = "Windows 7";
                break;
              case 2:
                strVers = "Windows 8";
                break;
              case 3:
                strVers = "Windows 8.1";
                break;
            }

            break;
          }
        }
      }

      if (strVers == "") {
        strVers = "Windows";
      } else {
        var sp = osInfo.ServicePack;
        if (shortname) {
          strVers = strVers.Replace("Windows ", "Win");
          strVers += " " + sp.Replace("Service Pack ", "SP");
          // +", Revision " + osInfo.Version.Revision.ToString() + ", " + osInfo.VersionString;
        } else {
          strVers += " " + sp;
        }
      }

      return strVers;
    }



    public static string GetLocalizedName([NotNull] string resource) {
      if (resource == null)
        throw new ArgumentNullException(nameof(resource));
      var id = 0;
      string file;

      if (resource.StartsWith("@"))
        resource = resource.Substring(1);

      var idPos = resource.LastIndexOf(",", StringComparison.Ordinal);

      if (idPos != 0) {
        file = Environment.ExpandEnvironmentVariables(resource.Substring(0, idPos));
        id = int.Parse(resource.Substring(idPos + 1));
        id = Math.Abs(id);
      } else {
        file = resource;
      }

      return GetLocalizedName(file, id);
    }



    public static string GetLocalizedName(string file, int id) {
      var sb = new StringBuilder(500);
      var res = "";
      var len = sb.Capacity;

      var hMod = UnsafeNativeMethods.LoadLibraryEx(
        file,
        IntPtr.Zero,
        DONT_RESOLVE_DLL_REFERENCES | LOAD_LIBRARY_AS_DATAFILE
      );

      if (hMod == IntPtr.Zero) {
        return res;
      }

      if (UnsafeNativeMethods.LoadString(hMod, id, sb, len) != 0) {
        res = sb.ToString();
      }

      UnsafeNativeMethods.FreeLibrary(hMod);

      return res;
    }



    private static class UnsafeNativeMethods {
      [DllImport("user32.dll", CharSet = CharSet.Unicode)]
      internal static extern int LoadString(IntPtr hModule, int resourceId, StringBuilder resourceValue, int len);



      //[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadLibraryW")]
      //internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);
      [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
      internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);



      [DllImport("kernel32.dll", ExactSpelling = true)]
      internal static extern int FreeLibrary(IntPtr hModule);
    }



    public static IEnumerable<string> GetApplicationFiles(string rootDirectory)
      => Directory.Exists(rootDirectory)
           ? Directory.GetFiles(rootDirectory, FILE_PATTERN_APP, SearchOption.AllDirectories)
           : throw new IOException("Folder does not exist: " + rootDirectory);
  }
}
