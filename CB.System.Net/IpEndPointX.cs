using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;



namespace CB.System.Net {
  public static class IpEndPointX {
    private const char PORT_SEPARATOR = ':';



    /// <summary>
    ///   Parses IPv4 and IPv6 notation.
    /// </summary>
    /// <param name="string"></param>
    /// <returns></returns>
    public static IPEndPoint ParseIpEndPoint(string @string) {
      return !@string.TrySeparateLast(PORT_SEPARATOR, out var addressStr, out var portStr)
        ? throw new FormatException("Invalid IP endpoint format")
        : !int.TryParse(portStr, out var port)
        ? throw new FormatException("Invalid IP port format")
        : !IPAddress.TryParse(addressStr, out var address)
        ? throw new FormatException("Invalid IP address format")
        : new IPEndPoint(address, port);
    }



    /// <summary>
    ///   Tries to parse IPv4 and IPv6 notation.
    /// </summary>
    /// <param name="string"></param>
    /// <param name="endPoint">the output IPEndPoint</param>
    /// <returns>true if parseable, otherwise false</returns>
    public static bool TryParse(string @string, out IPEndPoint? endPoint) {
      if (@string.TrySeparateLast(PORT_SEPARATOR, out var addressStr, out var portStr) &&
        int.TryParse(portStr, out var port) &&
        port >= 0 && port <= 65535 &&
        IPAddress.TryParse(addressStr, out var address)) {
        endPoint = new IPEndPoint(address, port);
        return true;
      }

      endPoint = default;
      return false;
    }



    private static int ParsePort(IReadOnlyList<string> endPointTokens) {
      return int.TryParse(
        endPointTokens[endPointTokens.Count - 1],
        NumberStyles.None,
        NumberFormatInfo.CurrentInfo,
        out var port
      )
        ? port
        :
      throw new FormatException("Invalid port");
    }
  }
}
