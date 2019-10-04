using System.Net;
using System.Net.NetworkInformation;
using NUnit.Framework;



namespace CB.System.Net.Diagnostics {
  [TestFixture]
  public class PcapFilterBuilderTests {
    private static string TrimIpV6(string str) {
      return str.Substring( 0, str.LastIndexOf( '%' ) );
    }



    [Test]
    public void AppendEthAddrTest() {
      var filter = new PcapFilterBuilder()
                   .AppendEthAddr( new PhysicalAddress( new byte[] {0, 1, 2, 3, 4, byte.MaxValue} ) )
                   .ToString();

      Assert.That( filter, Is.EqualTo( "ether host 00:01:02:03:04:FF" ) );
    }



    [Test]
    public void AppendIpSrcTest() {
      var filter = new PcapFilterBuilder()
                   .AppendIpSrc( new IPAddress( new byte[] {127, 0, 0, 1} ) )
                   .ToString();

      Assert.That( filter, Is.EqualTo( "src host 127.0.0.1" ) );
    }



    [Test]
    public void AppendIpSrcTest_OneIpV4AndOneIpV6() {
      var ipv4 = "127.0.0.1";
      var ipv6A = "fe80::d5bd:f894:fff8:93ce%5";
      var ipv6B = "fe80::d5bd:f894:fff8:93ce%15";

      var filter = new PcapFilterBuilder()
                   .AppendIpSrc(
                     IPAddress.Parse( ipv4 ),
                     IPAddress.Parse( ipv6A ),
                     IPAddress.Parse( ipv6B )
                   )
                   .ToString();

      Assert.That(
        filter,
        Is.EqualTo(
          $"( src host {ipv4} || src host {TrimIpV6( ipv6A )} || src host {TrimIpV6( ipv6B )} )"
        )
      );
    }



    [Test]
    public void AppendIpSrcTest_TwoIpAddresses() {
      var filter = new PcapFilterBuilder()
                   .AppendIpSrc(
                     new IPAddress( new byte[] {127, 0, 0, 1} ),
                     new IPAddress( new byte[] {127, 0, 0, 2} )
                   )
                   .ToString();

      Assert.That( filter, Is.EqualTo( "( src host 127.0.0.1 || src host 127.0.0.2 )" ) );
    }



    [Test]
    public void AppendSrcPortTest() {
      var filter = new PcapFilterBuilder()
                   .AppendPortSrc( 80 )
                   .ToString();

      Assert.That( filter, Is.EqualTo( "src port 80" ) );
    }
  }
}
