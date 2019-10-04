﻿using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using JetBrains.Annotations;



namespace CB.System.Net {
  public static class NetInfo {
    [NotNull]
    public static IPAddress[] GetLocalIpAddresses() {
      var strHostName = Dns.GetHostName();
      return Dns.GetHostAddresses( strHostName );
    }



    [CanBeNull]
    public static PhysicalAddress GetPhysicalAddress([NotNull] IPAddress ipAddress) {
      try {
        var arpStream = new CommandLine().Execute( "arp", "-a " + ipAddress );

        // Consume first three lines
        arpStream.ReadLine();
        arpStream.ReadLine();
        arpStream.ReadLine();

        // Read entries
        while (!arpStream.EndOfStream) {
          var line = arpStream.ReadLine();
          var tokens = line?.Split( new[] {' '}, StringSplitOptions.RemoveEmptyEntries );

          if (tokens?.Length != 3)
            continue;

          var mac = tokens[1];
          return PhysicalAddress.Parse( mac );
        }
      } catch (Exception e) {
        Console.Error.WriteLine( e );
      }

      return null;
    }



    [CanBeNull]
    public static string GetHostName([NotNull] IPAddress ipAddress) {
      try {
        return Dns
               .GetHostEntry( ipAddress )
               .HostName;
      } catch (SocketException e) {
        Console.Error.WriteLine( e );
      }

      return null;
    }
  }
}
