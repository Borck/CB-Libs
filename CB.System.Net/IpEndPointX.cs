using System;
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
      if (!@string.TryTail( PORT_SEPARATOR, out var addressStr, out var portStr )) {
        throw new FormatException( "Invalid IP endpoint format" );
      }

      if (!int.TryParse( portStr, out var port )) {
        throw new FormatException( "Invalid IP port format" );
      }

      if (!IPAddress.TryParse( addressStr, out var address )) {
        throw new FormatException( "Invalid IP address format" );
      }

      return new IPEndPoint( address, port );
    }



    /// <summary>
    ///   Tries to parses IPv4 and IPv6 notation.
    /// </summary>
    /// <param name="string"></param>
    /// <param name="endPoint">the output IPEndPoint</param>
    /// <returns>true if parseable, otherwise false</returns>
    public static bool TryParse(string @string, out IPEndPoint endPoint) {
      if (@string.TryTail( PORT_SEPARATOR, out var addressStr, out var portStr ) &&
          int.TryParse( portStr, out var port ) &&
          IPAddress.TryParse( addressStr, out var address )) {
        endPoint = new IPEndPoint( address, port );
        return true;
      }

      endPoint = default(IPEndPoint);
      return false;
    }



    private static int ParsePort(string[] endPointTokens) {
      if (int.TryParse(
        endPointTokens[endPointTokens.Length - 1],
        NumberStyles.None,
        NumberFormatInfo.CurrentInfo,
        out var port
      ))
        return port;
      throw new FormatException( "Invalid port" );
    }
  }
}
