﻿using System;
using Microsoft.Win32;



namespace CB.Win32.Registry {
  public static class RegistryFactory {
    private const string SUBKEY_SEP = @"\";
    private const string SUBKEY_DEFAULTICON = @"DefaultIcon";



    public static class Drive {
      public static string GetClassRoot(RegistryHive hive) {
        switch (hive) {
          case RegistryHive.CurrentUser:
            return @"Software\Classes\Applications\Explorer.exe\Drives";
          case RegistryHive.LocalMachine:
            return @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\DriveIcons";
          default:
            throw new ArgumentOutOfRangeException();
        }
      }



      public static string GetClassPath(string drivePath, RegistryHive hive)
        => GetClassRoot(hive) + SUBKEY_SEP + drivePath[0];



      public static RegistryKey OpenDefaultIconKey(string driveName, RegistryHive hive) {
        var keyName = GetClassPath(driveName, hive) + SUBKEY_SEP + SUBKEY_DEFAULTICON;
        return RegistryKey.OpenBaseKey(hive, RegistryView.Default).OpenSubKey(keyName);
      }



      public static RegistryKey CreateDefaultIconKey(string driveName, RegistryHive hive) {
        var keyName = GetClassPath(driveName, hive) + SUBKEY_SEP + SUBKEY_DEFAULTICON;
        return RegistryKey.OpenBaseKey(hive, RegistryView.Default).CreateSubKey(keyName);
      }



      public static RegistryKey OpenDefaultIconKey() {
        return Microsoft.Win32.Registry.ClassesRoot
                        .OpenSubKey(@"Drive" + SUBKEY_SEP + SUBKEY_DEFAULTICON);
      }



      public static RegistryKey OpenDefaultIconKey(string driveName) {
        return OpenDefaultIconKey(driveName, RegistryHive.CurrentUser) ??
               OpenDefaultIconKey(driveName, RegistryHive.LocalMachine) ??
               OpenDefaultIconKey();
      }



      // public void WriteDriveIconName(string drivePath, string iconPath) {
      //   var regKeyPath = GetDriveClassPath(drivePath) + SUBKEY_SEP + SUBKEY_DEFAULTICON;
      //   Key.CreateSubKey(regKeyPath)
      //      ?.SetValue(null, iconPath);
      // }
      //
      //
      //
      // public void DeleteDriveIcon(string drivePath) {
      //   var regKeyPath = GetDriveClassPath(drivePath);
      //   Key.OpenSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree)
      //      ?.DeleteSubKey(SUBKEY_DEFAULTICON, false);
      // }
    }
  }
}
