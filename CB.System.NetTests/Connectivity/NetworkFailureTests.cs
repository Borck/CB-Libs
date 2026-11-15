namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Linq;
using global::System.IO;
using global::System.Net.Http;
using global::System.Net.Sockets;
using global::System.Security.Authentication;
using Xunit;



public class NetworkFailureTests {
  [Theory]
  [InlineData(SocketError.HostNotFound, NetworkFailureKind.HostNotFound)]
  [InlineData(SocketError.TryAgain, NetworkFailureKind.HostNotFound)]
  [InlineData(SocketError.ConnectionRefused, NetworkFailureKind.Refused)]
  [InlineData(SocketError.HostUnreachable, NetworkFailureKind.Unreachable)]
  [InlineData(SocketError.NetworkUnreachable, NetworkFailureKind.Unreachable)]
  [InlineData(SocketError.TimedOut, NetworkFailureKind.TimedOut)]
  [InlineData(SocketError.ConnectionReset, NetworkFailureKind.ConnectionLost)]
  [InlineData(SocketError.ConnectionAborted, NetworkFailureKind.ConnectionLost)]
  public void SocketErrorsAreClassified(SocketError error, NetworkFailureKind expected) =>
    Assert.Equal(expected, NetworkFailure.Classify(new SocketException((int)error)));



  [Fact]
  public void ADeadSocketReportedByNetworkStreamIsAConnectionLoss() {
    // NetworkStream wraps socket failures in IOException - which is why catching SocketException alone never
    // noticed a dead controller link.
    var ex = new IOException("Unable to write data", new SocketException((int)SocketError.ConnectionReset));

    Assert.Equal(NetworkFailureKind.ConnectionLost, NetworkFailure.Classify(ex));
  }



  [Fact]
  public void TheMostSpecificCauseInTheChainWins() {
#if NET8_0_OR_GREATER
    var dns = new HttpRequestException(
      HttpRequestError.NameResolutionError,
      "No such host is known",
      new SocketException((int)SocketError.HostNotFound)
    );
#else
    // No HttpRequestError before .NET 8: the SocketException inside is what gets classified.
    var dns = new HttpRequestException("No such host is known", new SocketException((int)SocketError.HostNotFound));
#endif

    Assert.Equal(NetworkFailureKind.HostNotFound, NetworkFailure.Classify(new AggregateException(dns)));
    Assert.Equal(NetworkFailureKind.TlsRejected, NetworkFailure.Classify(new AuthenticationException("refused")));
    Assert.Equal(NetworkFailureKind.TimedOut, NetworkFailure.Classify(new TimeoutException()));
  }



  [Fact]
  public void AnInvalidEndpointIsNotTransient() {
    var kind = NetworkFailure.Classify(new ArgumentException("'http://x' is not a host name"));

    Assert.Equal(NetworkFailureKind.InvalidEndpoint, kind);
    Assert.False(NetworkFailure.IsTransient(kind));
    Assert.True(NetworkFailure.IsTransient(NetworkFailureKind.HostNotFound));
  }



  [Fact]
  public void EveryKindHasAnOperatorFacingDescription() {
    foreach (var kind in Enum.GetValues(typeof(NetworkFailureKind)).Cast<NetworkFailureKind>()) {
      Assert.False(string.IsNullOrWhiteSpace(NetworkFailure.Describe(kind)));
    }
  }
}
