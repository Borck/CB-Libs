using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using static Microsoft.Win32.Registry;



namespace CB.Win32.Registry {
  public static class Registry {
    #region constants

    private const string DEF_SEP = "\\";
    private static readonly char DEF_SEPC = DEF_SEP[0];
    internal const string HKCR = "HKEY_CLASSES_ROOT";
    internal const string HKCR2 = "HKCR";
    internal const string HKCU = "HKEY_CURRENT_USER";
    internal const string HKCU2 = "HKCU";
    internal const string HKLM = "HKEY_LOCAL_MACHINE";
    internal const string HKLM2 = "HKLM";
    internal const string HKU = "HKEY_USERS";
    internal const string HKPD = "HKEY_PERFORMANCE_DATA";
    internal const string HKCC = "HKEY_CURRENT_CONFIG";
    internal const string HKDD = "HKEY_DYNAMIC_DATA";
    internal const string HKDD2 = "HKEY_DYN_DATA";
    private const string REGKEY_IGNORE_PREFIX_COMPUTER = "COMPUTER" + DEF_SEP;

    private static readonly Dictionary<string, (RegistryHive Hive, RegistryKey Key, string ShortName)> Str2Hive =
      new Dictionary<string, (RegistryHive Hive, RegistryKey Key, string ShortName)>(StringComparer.OrdinalIgnoreCase) {
        {HKCR, (RegistryHive.ClassesRoot, ClassesRoot, HKCR2)},
        {HKCR2, (RegistryHive.ClassesRoot, ClassesRoot, HKCR2)},
        {HKCU, (RegistryHive.CurrentUser, CurrentUser, HKCU2)},
        {HKCU2, (RegistryHive.CurrentUser, CurrentUser, HKCU2)},
        {HKLM, (RegistryHive.LocalMachine, LocalMachine, HKLM2)},
        {HKLM2, (RegistryHive.LocalMachine, LocalMachine, HKLM2)},
        {HKU, (RegistryHive.Users, Users, HKU)},
        {HKPD, (RegistryHive.PerformanceData, PerformanceData, HKPD)},
        {HKCC, (RegistryHive.CurrentConfig, CurrentConfig, HKCC)},
      };

    #endregion

    #region Parsing

    private static (RegistryHive Hive, RegistryKey Key, string ShortName) DoCutHive(ref string regPath) {
      regPath = CleanPathStart(regPath);
      var hiveStr = CutHead(ref regPath, DEF_SEPC);
      if (!Str2Hive.TryGetValue(hiveStr, out var entry))
        throw new ArgumentException(@"No registry hive found", nameof(hiveStr));
      return entry;
    }



    /**
     * return the hive and writes the rest path back to regPath
     */
    public static string CutHiveString(ref string regPath) {
      return DoCutHive(ref regPath).Key.Name;
    }



    private static string CutHead(ref string text, char separator) {
      var i = text?.IndexOf(separator) ?? 0;
      var result = i != -1
                     ? text?.Substring(0, i)
                     : text;
      text = i + 1 < text?.Length ? text?.Substring(i + 1) : "";
      return result;
    }



    private static string CleanPathStart(string regPath) {
      return regPath.ToUpper().StartsWith(REGKEY_IGNORE_PREFIX_COMPUTER)
               ? regPath.Substring(REGKEY_IGNORE_PREFIX_COMPUTER.Length)
               : regPath;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RegistryHive GetHive(string regPath) {
      return DoCutHive(ref regPath).Hive;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RegistryHive CutHive(ref string regPath) {
      return DoCutHive(ref regPath).Hive;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RegistryKey GetHiveKey(string regPath) {
      return DoCutHive(ref regPath).Key;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RegistryKey CutHiveKey(ref string regPath) {
      return DoCutHive(ref regPath).Key;
    }



    public static RegistryKey OpenKey(string regPath) {
      var hiveKey = CutHiveKey(ref regPath);
      return string.IsNullOrEmpty(regPath)
               ? hiveKey
               : hiveKey.OpenSubKey(regPath);
    }

    #endregion


    #region extensions

    public static string GetShortName(this RegistryKey key) {
      var regPath = key.Name;
      return DoCutHive(ref regPath).ShortName + DEF_SEP + regPath;
    }

    #endregion
  }
}
