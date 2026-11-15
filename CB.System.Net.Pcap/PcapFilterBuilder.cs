using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;



namespace CB.System.Net.Diagnostics {
  /// <summary>
  ///   Builder of filters for PCAP.
  ///   <see href="https://www.winpcap.org/docs/docs_40_2/html/group__language.html" />
  /// </summary>
  public class PcapFilterBuilder {
    private readonly IList<string> _args;



    public PcapFilterBuilder() {
      _args = new List<string>();
    }



    public PcapFilterBuilder(string pcapFilter) {
      _args = pcapFilter.Split( ' ' ).ToList();
    }



    /// <summary>
    ///   Joins multiple appends with OR and covers it with braces. For example: (a OR b)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="appendAction"></param>
    private void DoJoinWithOr<T>(IReadOnlyList<T> items, Func<T, PcapFilterBuilder> appendAction) {
      var itemsCount = items.Count;
      switch (itemsCount) {
        case 0:
          break;
        case 1:
          appendAction( items[0] );
          break;
        default:
          AppendOpenBrace();

          var appendOr = false;
          foreach (var item in items) {
            if (appendOr) {
              AppendOr();
            }

            appendOr = true;

            appendAction( item );
          }

          AppendCloseBrace();
          break;
      }
    }



    private PcapFilterBuilder DoAppend(string arg) {
      _args.Add( arg );
      return this;
    }



    private PcapFilterBuilder DoAppendRepeatedly(string arg, int times) {
      for (var i = 0; i < times; i++) {
        _args.Add( arg );
      }

      return this;
    }



    private PcapFilterBuilder DoAppendKeyValues(string key, object value)
      => DoAppend( $"{key} {value}" );



    private PcapFilterBuilder DoAppendKeyValues<T>(string key, IReadOnlyList<T> values) {
      DoJoinWithOr( values, value => DoAppendKeyValues( key, value ) );
      return this;
    }



    private PcapFilterBuilder DoAppendKeyValues(string key, IEnumerable<object> values)
      => DoAppendKeyValues( key, values.ToArray() );



    public PcapFilterBuilder AppendNot()
      => DoAppend( "NOT" );



    public PcapFilterBuilder AppendOr()
      => DoAppend( "||" );



    public PcapFilterBuilder AppendAnd()
      => DoAppend( "&&" );



    public PcapFilterBuilder AppendOpenBrace(int times = 1)
      => DoAppendRepeatedly( "(", times );



    public PcapFilterBuilder AppendCloseBrace(int times = 1)
      => DoAppendRepeatedly( ")", times );



    public PcapFilterBuilder AppendIpSrc(params IPAddress[] ipAddresses) {
      var ipStrings = ipAddresses.Select( ToString );
      return DoAppendKeyValues( "src host", ipStrings );
    }



    public PcapFilterBuilder AppendIpDst(params IPAddress[] ipAddresses) {
      var ipStrings = ipAddresses.Select( ToString );
      return DoAppendKeyValues( "dst host", ipStrings );
    }



    public PcapFilterBuilder AppendPortSrc(params int[] ports)
      => DoAppendKeyValues( "src port", ports );



    public PcapFilterBuilder AppendPortDst(params int[] ports)
      => DoAppendKeyValues( "dst port", ports );



    public PcapFilterBuilder AppendEthAddr(params PhysicalAddress[] macAddresses)
      => DoAppendKeyValues(
        "ether host",
        macAddresses.Select( ToString )
      );



    public PcapFilterBuilder AppendTcp()
      => DoAppend( "tcp" );



    public PcapFilterBuilder AppendUdp()
      => DoAppend( "udp" );



    public PcapFilterBuilder Icmp()
      => DoAppend( "icmp" );



    private static string ToString(PhysicalAddress address)
      => ToHexString( address.GetAddressBytes() );



    private static string ToString(IPAddress ipAddress) {
      switch (ipAddress.AddressFamily) {
        case AddressFamily.InterNetwork:
          return ipAddress.ToString();
        case AddressFamily.InterNetworkV6:
          var ipString = ipAddress.ToString();
          var iCutoff = ipString.LastIndexOf( '%' );
          return iCutoff == -1
                   ? ipString
                   : ipString.Substring( 0, iCutoff );
        default:
          throw new NotSupportedException(
            $"Address family '{ipAddress.AddressFamily}' is not supported: {ipAddress}"
          );
      }
    }



    private static string ToHexString(byte[] bytes)
      => string.Join(
        ":",
        ( from z
            in bytes
          select z.ToString( "X2" ) ).ToArray()
      );



    public override string ToString()
      => string.Join( " ", _args );



    public static class Factory {
      public static PcapFilterBuilder CreateForLocalTcpPort(int port) {
        var ipAddresses = NetInfo
          .GetLocalIpAddresses();

        return new PcapFilterBuilder()
               //source ip:port
               .AppendOpenBrace( 2 )
               .AppendIpSrc( ipAddresses )
               .AppendAnd()
               .AppendPortSrc( port )
               .AppendCloseBrace()
               .AppendOr()
               //destination ip:port
               .AppendOpenBrace()
               .AppendIpDst( ipAddresses )
               .AppendAnd()
               .AppendPortDst( port )
               .AppendCloseBrace( 2 )
               .AppendAnd()
               .AppendTcp();
      }
    }
  }
}
