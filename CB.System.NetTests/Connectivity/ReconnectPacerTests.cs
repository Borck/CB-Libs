namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Net.Sockets;
using global::System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;



public class ReconnectPacerTests {
  private static readonly ReconnectOptions _options = new() {
    InitialDelay = TimeSpan.FromMilliseconds(40),
    MaxDelay = TimeSpan.FromMilliseconds(200),
    StableAfter = TimeSpan.FromSeconds(30)
  };



  [Fact(Timeout = 20_000)]
  public async Task ALinkThatDidNotStayUpIsNotRedialledAtOnce() {
    var time = new ManualTimeProvider();
    var pacer = new ReconnectPacer(new RecordingLogger(), "broker", _options, time);

    pacer.Connected();
    time.Advance(TimeSpan.FromSeconds(1)); // kicked off right away, e.g. a duplicate MQTT client id

    var waited = await pacer.DisconnectedAsync(TestContext.Current.CancellationToken);

    Assert.True(waited > TimeSpan.Zero);
  }



  [Fact(Timeout = 20_000)]
  public async Task ALinkThatStayedUpIsRedialledAtOnce() {
    var time = new ManualTimeProvider();
    var pacer = new ReconnectPacer(new RecordingLogger(), "broker", _options, time);

    pacer.Connected();
    time.Advance(TimeSpan.FromMinutes(5));

    Assert.Equal(TimeSpan.Zero, await pacer.DisconnectedAsync(TestContext.Current.CancellationToken));
  }



  [Fact(Timeout = 20_000)]
  public async Task FailuresBackOffAndAreLoggedOncePerCause() {
    var logger = new RecordingLogger();
    var pacer = new ReconnectPacer(logger, "broker", _options with { Jitter = 0 });
    var ct = TestContext.Current.CancellationToken;

    var first = await pacer.FailedAsync(new SocketException((int)SocketError.ConnectionRefused), ct);
    var second = await pacer.FailedAsync(new SocketException((int)SocketError.ConnectionRefused), ct);

    Assert.True(second > first);
    Assert.Equal(1, logger.Count(LogLevel.Warning));
  }
}
