namespace CB.System.Net;

using global::System.Net;
using Xunit;



public class IPAddressXTests {
  [Theory]
  [InlineData("127.0.0.1")]
  [InlineData("127.1.2.3")]
  [InlineData("::1")]
  [InlineData("::ffff:127.0.0.1")]
  public void IsLoopbackOrMapped_WithLoopbackAddress_ExpectTrue(string address) =>
    Assert.True(IPAddress.Parse(address).IsLoopbackOrMapped());



  [Theory]
  [InlineData("10.0.0.1")]
  [InlineData("192.168.1.20")]
  [InlineData("0.0.0.0")]
  [InlineData("2001:db8::1")]
  [InlineData("::ffff:10.0.0.1")]
  [InlineData("::")]
  public void IsLoopbackOrMapped_WithOtherAddress_ExpectFalse(string address) =>
    Assert.False(IPAddress.Parse(address).IsLoopbackOrMapped());



  [Fact]
  public void IsLoopbackOrMapped_WithNull_ExpectFalse() => Assert.False(((IPAddress?)null).IsLoopbackOrMapped());



  [Theory]
  [InlineData("localhost")]
  [InlineData("LocalHost")]
  [InlineData(" localhost ")]
  [InlineData("127.0.0.1")]
  [InlineData("[::1]")]
  [InlineData("::1")]
  [InlineData("[::ffff:127.0.0.1]")]
  public void IsLoopbackHost_WithLoopbackHost_ExpectTrue(string host) =>
    Assert.True(IPAddressX.IsLoopbackHost(host));



  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("example.com")]
  [InlineData("localhost.example.com")]
  [InlineData("10.0.0.1")]
  [InlineData("[2001:db8::1]")]
  [InlineData("host.docker.internal")]
  public void IsLoopbackHost_WithOtherHost_ExpectFalse(string host) =>
    Assert.False(IPAddressX.IsLoopbackHost(host));



  [Fact]
  public void IsLoopbackHost_WithNull_ExpectFalse() => Assert.False(IPAddressX.IsLoopbackHost(null));
}
