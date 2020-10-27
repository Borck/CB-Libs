using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CB.System;
using Microsoft.Win32;
using static Microsoft.Win32.Registry;



namespace CB.Win32.Registry {
  public static class Registry {
    #region constants

    private const string DEF_SEP = "\\";
    public static readonly char PathSeparator = DEF_SEP[0];
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

    private static readonly ICollection<RegistryHiveInfo> HiveInfos =
      new List<RegistryHiveInfo> {
        (ClassesRoot, RegistryHive.ClassesRoot, HKCR, HKCR2),
        (CurrentUser, RegistryHive.CurrentUser, HKCU, HKCU2),
        (LocalMachine, RegistryHive.LocalMachine, HKLM, HKLM2),
        (Users, RegistryHive.Users, HKU, default(string)),
        (PerformanceData, RegistryHive.PerformanceData, HKPD, default(string)),
        (CurrentConfig, RegistryHive.CurrentConfig, HKCC, default(string))
      };

    private static readonly Dictionary<string, RegistryHiveInfo> Str2Hive;



    static Registry() {
      Str2Hive = HiveInfos.SelectMany(
                            info => info.ShortName != default
                                      ? new[] {(info.Name, info), (info.ShortName, info)}
                                      : new[] {(info.Name, info)}
                          )
                          .ToDictionary(info => info.Item1, info => info.info);
    }

    #endregion

    #region Parsing

    internal static RegistryHiveInfo DoCutHive(ref string regPath) {
      regPath = CleanPathStart(regPath);
      string hiveStr;

      (hiveStr, regPath) = regPath.Separate(PathSeparator);
      return Str2Hive.TryGetValue(hiveStr, out var entry)
               ? entry
               : default;
    }



    /**
     * return the hive and writes the rest path back to regPath
     */
    public static string CutHiveString(ref string regPath) {
      return DoCutHive(ref regPath).Key.Name;
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



    private static bool DoTryGetNormalized<T>(string registryKeyName,
                                              out T hive,
                                              out string subKeyName,
                                              Func<RegistryHiveInfo, T> getHive) {
      if (registryKeyName.StartsWith(REGKEY_IGNORE_PREFIX_COMPUTER, StringComparison.InvariantCultureIgnoreCase)) {
        registryKeyName = registryKeyName.Substring(REGKEY_IGNORE_PREFIX_COMPUTER.Length);
      }

      var hasSubKey = registryKeyName.TrySeparate(PathSeparator, out var hiveName, out var subKeyName2);
      subKeyName = hasSubKey ? subKeyName2 : default;
      var hiveFound = Str2Hive.TryGetValue(hiveName, out var hiveInfo);
      hive = hiveFound ? getHive(hiveInfo) : default;
      return hiveFound;
    }



    public static bool TryGetNormalized(string registryKeyName, out RegistryHive hive, out string subKeyName)
      => DoTryGetNormalized(registryKeyName, out hive, out subKeyName, hiveInfo => hiveInfo.Hive);



    public static bool TryGetNormalized(string registryKeyName, out RegistryKey hiveKey, out string subKeyName)
      => DoTryGetNormalized(registryKeyName, out hiveKey, out subKeyName, hiveInfo => hiveInfo.Key);



    public static bool TryGetNormalized(string registryKeyName, out string hiveName, out string subKeyName)
      => DoTryGetNormalized(registryKeyName, out hiveName, out subKeyName, hiveInfo => hiveInfo.Name);


    public static RegistryKey OpenRegistryKey(string registryName)
      => !TryGetNormalized(registryName, out RegistryKey hive, out var subKey)
           ? default
           : subKey != default
             ? hive.OpenSubKey(subKey)
             : hive;



    public static bool TryOpenRegistryKey(string registryName, out RegistryKey registryKey) {
      registryKey = OpenRegistryKey(registryName);
      return registryKey != default;
    }
    #endregion


    #region extensions

    [Obsolete("Use RegistryKeyX.GetShortenedName instead")]
    public static string GetShortName(this RegistryKey key) => key.GetShortenedName();

    #endregion



    internal struct RegistryHiveInfo {
      public RegistryKey Key;
      public RegistryHive Hive;
      public string Name;
      public string ShortName;



      public static implicit operator RegistryHiveInfo((RegistryKey key,
                                                         RegistryHive hive,
                                                         string name,
                                                         string shortName) infos)
        => new RegistryHiveInfo() {Key = infos.key, Hive = infos.hive, Name = infos.name, ShortName = infos.shortName};
    }
  }
}
