namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Net;
using global::System.Net.Sockets;
using global::System.Threading;
using global::System.Threading.Tasks;
using Xunit;



public class HostResolverTests {
  private static readonly IPAddress _address = IPAddress.Parse("10.1.2.3");



  [Fact]
  public async Task AnIpLiteralNeverTouchesDns() {
    var resolver = new HostResolver((_, _) => throw new InvalidOperationException("DNS must not be asked"));

    var resolved = await resolver.ResolveAsync("192.168.0.10", cancellationToken: TestContext.Current.CancellationToken);

    Assert.Equal([IPAddress.Parse("192.168.0.10")], resolved.Addresses);
    Assert.False(resolved.FromCache);
  }



  [Fact]
  public async Task ATransientDnsFailureFallsBackToTheLastAddressThatWorked() {
    var dnsUp = true;
    var resolver = new HostResolver((_, _) =>
      dnsUp
        ? Task.FromResult(new[] { _address })
        : Task.FromException<IPAddress[]>(new SocketException((int)SocketError.HostNotFound))
    );
    var ct = TestContext.Current.CancellationToken;

    await resolver.ResolveAsync("controller.plant.local", cancellationToken: ct);
    dnsUp = false;
    var resolved = await resolver.ResolveAsync("controller.plant.local", cancellationToken: ct);

    Assert.Equal([_address], resolved.Addresses);
    Assert.True(resolved.FromCache);
    Assert.IsType<SocketException>(resolved.ResolutionError);
  }



  [Fact]
  public async Task WithNothingRememberedTheFailureIsReported() {
    var resolver = new HostResolver((_, _) =>
      Task.FromException<IPAddress[]>(new SocketException((int)SocketError.HostNotFound))
    );

    var ex = await Assert.ThrowsAsync<SocketException>(() =>
      resolver.ResolveAsync("nowhere.invalid", cancellationToken: TestContext.Current.CancellationToken)
    );
    Assert.Equal(SocketError.HostNotFound, ex.SocketErrorCode);
  }



  [Fact]
  public async Task AStaleAnswerIsNotUsedForever() {
    var time = new ManualTimeProvider();
    var dnsUp = true;
    var resolver = new HostResolver(
      (_, _) => dnsUp
                  ? Task.FromResult(new[] { _address })
                  : Task.FromException<IPAddress[]>(new SocketException((int)SocketError.HostNotFound)),
      time
    ) { MaxStaleness = TimeSpan.FromHours(1) };
    var ct = TestContext.Current.CancellationToken;

    await resolver.ResolveAsync("controller", cancellationToken: ct);
    dnsUp = false;
    time.Advance(TimeSpan.FromHours(2));

    await Assert.ThrowsAsync<SocketException>(() => resolver.ResolveAsync("controller", cancellationToken: ct));
  }



  [Fact(Timeout = 20_000)]
  public async Task AHangingLookupIsBoundedByTheDeadline() {
    var resolver = new HostResolver((_, _) => new TaskCompletionSource<IPAddress[]>().Task);

    await Assert.ThrowsAsync<TimeoutException>(() =>
      resolver.ResolveAsync(
        "silent-dns",
        TimeSpan.FromMilliseconds(100),
        TestContext.Current.CancellationToken
      )
    );
  }



  [Fact]
  public async Task AUrlInAHostFieldIsAnInvalidEndpointNotADnsFailure() {
    var resolver = new HostResolver((_, _) => throw new InvalidOperationException("DNS must not be asked"));

    var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
      resolver.ResolveAsync("http://10.0.0.1:8001", cancellationToken: TestContext.Current.CancellationToken)
    );
    Assert.Equal(NetworkFailureKind.InvalidEndpoint, NetworkFailure.Classify(ex));
  }



  [Theory]
  [InlineData("controller_01")]       // NetBIOS/LLMNR names may carry underscores
  [InlineData("PLC-LINE-3")]
  [InlineData("host.plant.local")]
  public async Task NamesTheResolverMayKnowAreLeftToTheResolver(string host) {
    var asked = false;
    var resolver = new HostResolver((_, _) => {
      asked = true;
      return Task.FromResult(new[] { _address });
    });

    await resolver.ResolveAsync(host, cancellationToken: TestContext.Current.CancellationToken);

    Assert.True(asked);
  }



  [Theory]
  [InlineData("tcp://10.0.0.1")]
  [InlineData("10.0.0.1/24")]
  [InlineData("my host")]
  public void WhatCanNeverBeAHostNameIsRejectedUpFront(string host) =>
    Assert.False(HostResolver.LooksLikeAHostName(host));



  [Fact]
  public async Task CallerCancellationIsNotMistakenForADnsFailure() {
    using var cts = new CancellationTokenSource();
    var resolver = new HostResolver(async (_, ct) => {
      await cts.CancelAsync();
      ct.ThrowIfCancellationRequested();
      return [_address];
    });

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
      resolver.ResolveAsync("controller", cancellationToken: cts.Token)
    );
  }
}
