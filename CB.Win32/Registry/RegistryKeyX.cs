using Microsoft.Win32;



namespace CB.Win32.Registry {
  public static class RegistryKeyX {
    public static T GetValue<T>(this RegistryKey registryKey, string name)
      => (T)registryKey.GetValue(name);



    public static T GetValue<T>(this RegistryKey registryKey, string name, T defaultValue)
      => (T)registryKey.GetValue(name, defaultValue);



    public static string GetString(this RegistryKey registryKey, string name)
      => registryKey.GetValue<string>(name);
  }
}
