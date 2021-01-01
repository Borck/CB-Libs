using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;



namespace CB.System.Net {
  public static class NetInfo {
    /// <summary>
    ///   Get all ip addresses of this machine
    /// </summary>
    /// <returns></returns>
    public static IPAddress[] GetLocalIpAddresses() {
      var strHostName = Dns.GetHostName();
      return Dns.GetHostAddresses(strHostName);
    }



    /// <summary>
    ///   Get all ip addresses of this machine asynchronous
    /// </summary>
    /// <returns></returns>
    public static async Task<IPAddress[]> GetLocalIpAddressesAsync() {
      var strHostName = Dns.GetHostName();
      return await Dns.GetHostAddressesAsync(strHostName);
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



    public static PhysicalAddress? GetPhysicalAddress(IPAddress ipAddress)
      => TryGetPhysicalAddress(ipAddress, out var macAddress)
           ? macAddress!
           : default;



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



    public static async Task<PhysicalAddress?> GetPhysicalAddressAsync(IPAddress ipAddress) {
      var arpStream = Execute("arp", "-a " + ipAddress);

      // Consume first three lines
      await arpStream.ReadLineAsync();
      await arpStream.ReadLineAsync();
      await arpStream.ReadLineAsync();

      // Read entries
      while (!arpStream.EndOfStream) {
        var line = await arpStream.ReadLineAsync();
        var tokens = line?.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

        if (tokens?.Length != 3) {
          continue;
        }

        var mac = tokens[1];
        return PhysicalAddress.Parse(mac);
      }

      return default;
    }



    /// <summary>
    ///   Get the host name of a ip address
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    public static string GetHostName(IPAddress ipAddress)
      => Dns
         .GetHostEntry(ipAddress)
         .HostName;



    /// <summary>
    ///   Get the host name of a ip address asynchronous
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    public static async Task<string> GetHostNameAsync(IPAddress ipAddress)
      => (await Dns
            .GetHostEntryAsync(ipAddress))
        .HostName;
  }
}
