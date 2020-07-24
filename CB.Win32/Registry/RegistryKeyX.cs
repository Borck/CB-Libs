using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using CB.System.Collections;
using Microsoft.Win32;



namespace CB.Win32.Registry {
  public static class RegistryKeyX {
    public static string GetShortenedName(this RegistryKey key) {
      var regPath = key.Name;
      return Registry.DoCutHive(ref regPath).ShortName + Registry.PathSeparator + regPath;
    }



    public static T GetValue<T>(this RegistryKey registryKey, string name)
      => (T)registryKey.GetValue(name);



    public static T GetValue<T>(this RegistryKey registryKey, string name, T defaultValue)
      => (T)registryKey.GetValue(name, defaultValue);



    public static string GetString(this RegistryKey registryKey, string name)
      => registryKey.GetValue<string>(name);



    public static IEnumerable<RegistryKey> OpenSubKeys(this RegistryKey key)
      => key.GetSubKeyNames()
            .Select(key.OpenSubKey);



    public static IEnumerable<RegistryKey> OpenSubKeys(this RegistryKey key, RegistryRights rights)
      => key.GetSubKeyNames()
            .Select(subKeyName => key.OpenSubKey(subKeyName, rights));



    public static IEnumerable<RegistryKey> OpenSubKeys(this RegistryKey key, RegistryKeyPermissionCheck permissionCheck)
      => key.GetSubKeyNames()
            .Select(subKeyName => key.OpenSubKey(subKeyName, permissionCheck));



    public static IEnumerable<RegistryKey> OpenSubKeys(this RegistryKey key, bool writable)
      => key.GetSubKeyNames()
            .Select(subKeyName => key.OpenSubKey(subKeyName, writable));



    public static void DeleteSubKeys(this RegistryKey key)
      => key.GetSubKeyNames()
            .ForEach(key.DeleteSubKey);



    public static void DeleteSubKeys(this RegistryKey key, bool throwOnMissingSubKey)
      => key.GetSubKeyNames()
            .ForEach(subKeyName => key.DeleteSubKey(subKeyName, throwOnMissingSubKey));
  }
}
