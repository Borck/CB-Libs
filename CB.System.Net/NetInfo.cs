using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;



namespace CB.System.Net {
  public static class NetInfo {
    public static IPAddress[] GetLocalIpAddresses() {
      var strHostName = Dns.GetHostName();
      return Dns.GetHostAddresses(strHostName);
    }



    private static StreamReader Execute(string filename, string arguments) {
      var process = new Process {
        StartInfo = {
          FileName = filename,
          Arguments = arguments,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          CreateNoWindow = true
        }
      };
      process.Start();
      return process.StandardOutput;
    }



    [Obsolete("Use TryGetPhysicalAddress")]
    public static PhysicalAddress GetPhysicalAddress(IPAddress ipAddress) {
      return TryGetPhysicalAddress(ipAddress, out var macAddress)
               ? macAddress!
               : default!;
    }



    public static bool TryGetPhysicalAddress(IPAddress ipAddress, out PhysicalAddress? address) {
      var arpStream = Execute("arp", "-a " + ipAddress);

      // Consume first three lines
      arpStream.ReadLine();
      arpStream.ReadLine();
      arpStream.ReadLine();

      // Read entries
      while (!arpStream.EndOfStream) {
        var line = arpStream.ReadLine();
        var tokens = line?.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

        if (tokens?.Length != 3) {
          continue;
        }

        var mac = tokens[1];
        address = PhysicalAddress.Parse(mac);
        return true;
      }

      address = default;
      return false;
    }



    public static string GetHostName(IPAddress ipAddress)
      => Dns
         .GetHostEntry(ipAddress)
         .HostName;
  }
}
