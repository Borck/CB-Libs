namespace CB.System.Diagnostics;

using global::System;
using global::System.Globalization;
using global::System.IO;
using global::System.Runtime.InteropServices;



/// <summary>Physical memory of the machine: how much there is and how much is available.</summary>
/// <param name="TotalBytes">Installed memory the operating system can use.</param>
/// <param name="AvailableBytes">Memory a program could take without the machine swapping or evicting something it needs.</param>
public readonly record struct SystemMemoryStatus(double TotalBytes, double AvailableBytes) {
  /// <summary>The used share, 0..1: <c>1 - available / total</c>, clamped; 0 when the total is unknown.</summary>
  public float UsedFraction =>
    TotalBytes > 0 ? (float)Math.Clamp(1.0 - AvailableBytes / TotalBytes, 0.0, 1.0) : 0f;
}



/// <summary>The machine-wide memory reading (not the process's own): <c>GlobalMemoryStatusEx</c> on Windows, <c>/proc/meminfo</c> on Linux.</summary>
public static class SystemMemory {
  /// <summary>
  ///   Reads the memory of the machine now. Where the operating system offers no reading, or it cannot be read (a sandbox without
  ///   <c>/proc</c>), the runtime's own view is used on .NET Core and later, and an unknown (0 total) on net48 and netstandard2.0 off Windows.
  /// </summary>
  public static SystemMemoryStatus Read() {
    SystemMemoryStatus? status = null;

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      status = ReadWindows();
    } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
      status = ReadLinux();
    }

    return status ?? ReadFromRuntime();
  }



  /// <summary>
  ///   Parses the text of <c>/proc/meminfo</c>. <c>MemAvailable</c> is the kernel's own estimate of what can be reclaimed
  ///   (caches included) and is what counts; <c>MemFree</c> is only the fallback of a kernel older than 3.14, because free memory
  ///   alone calls a healthy machine full. Null without a <c>MemTotal</c>.
  /// </summary>
  public static SystemMemoryStatus? ParseMeminfo(string text) {
    ArgumentNullException.ThrowIfNull(text);
    double? total = null;
    double? available = null;
    double? free = null;

    foreach (var line in text.Split('\n')) {
      var colon = line.IndexOf(':');
      if (colon < 0) {
        continue;
      }

      var key = line[..colon];
      var rest = line[(colon + 1)..].Trim();
      var space = rest.IndexOf(' ');
      var number = space < 0 ? rest : rest[..space];

      if (!ulong.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out var kibibytes)) {
        continue;
      }

      var bytes = kibibytes * 1024.0;
      if (key == "MemTotal") {
        total = bytes;
      } else if (key == "MemAvailable") {
        available = bytes;
      } else if (key == "MemFree") {
        free = bytes;
      }
    }

    return total is > 0 ? new(total.Value, available ?? free ?? 0) : null;
  }



  private static SystemMemoryStatus? ReadLinux() {
    try {
      return ParseMeminfo(File.ReadAllText("/proc/meminfo"));
    } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
      return null;
    }
  }



  private static SystemMemoryStatus? ReadWindows() {
    var status = new NativeSystem.MemoryStatusEx { Length = (uint)Marshal.SizeOf<NativeSystem.MemoryStatusEx>() };
    return NativeSystem.GlobalMemoryStatusEx(ref status)
             ? new(status.TotalPhys, status.AvailPhys)
             : null;
  }



  private static SystemMemoryStatus ReadFromRuntime() {
#if NETCOREAPP
    var info = GC.GetGCMemoryInfo();
    var total = (double)info.TotalAvailableMemoryBytes;
    return new(total, Math.Max(0, total - info.MemoryLoadBytes));
#else
    return new(0, 0);
#endif
  }
}
