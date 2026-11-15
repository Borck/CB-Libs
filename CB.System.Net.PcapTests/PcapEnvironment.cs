using System;
using SharpPcap;



namespace CB.System.Net.Diagnostics {
  /// <summary>Whether this machine can capture packets: the native library is there and a device opens.</summary>
  internal static class PcapEnvironment {
    public const string SKIP_REASON =
      "Needs a packet capture library (Npcap or libpcap), the right to open a capture device and internet access.";

    public static bool IsCaptureAvailable { get; } = Probe();



    private static bool Probe() {
      try {
        var devices = CaptureDeviceList.Instance;
        if (devices.Count == 0) {
          return false;
        }

        devices[0].Open();
        devices[0].Close();
        return true;
      } catch (Exception) {
        // DllNotFoundException without the library, PcapException without the right to capture.
        return false;
      }
    }
  }
}
