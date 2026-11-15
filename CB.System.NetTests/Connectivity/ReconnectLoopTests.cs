namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Diagnostics;
using global::System.Net;
using global::System.Net.Sockets;
using global::System.Threading;
using global::System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;



public class ReconnectLoopTests {
  private static readonly ReconnectOptions _fast = new() {
    InitialDelay = TimeSpan.FromMilliseconds(10),
    MaxDelay = TimeSpan.FromMilliseconds(50),
    StableAfter = TimeSpan.FromMinutes(1),
    ConnectTimeout = TimeSpan.FromMilliseconds(200)
  };



  [Fact(Timeout = 20_000)]
  public async Task ReconnectsAfterFailuresAndAfterTheConnectionEnds() {
    var ct = TestContext.Current.CancellationToken;
    var logger = new RecordingLogger();
    var attempts = 0;
    var runs = 0;
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

    var loop = ReconnectLoop.RunAsync(
      _ => ++attempts <= 3
             ? Task.FromException<string>(new SocketException((int)SocketError.ConnectionRefused))
             : Task.FromResult("connection"),
      async (_, _) => {
        if (++runs == 2) {
          await cts.CancelAsync();
        }
      },
      new ConnectionFailureLog(logger, "target"),
      _fast,
      cancellationToken: cts.Token
    );

    await loop;

    Assert.Equal(2, runs);
    Assert.Equal(5, attempts);

    // One warning for the refused attempts (three of them, same cause), one for the first connection closing
    // long before StableAfter - a different cause. The recovery is logged once for the whole outage.
    Assert.Equal(2, logger.Count(LogLevel.Warning));
    Assert.Equal(1, logger.Count(LogLevel.Information));
  }



  [Fact(Timeout = 20_000)]
  public async Task AConnectAttemptThatHangsIsReportedAsATimeout() {
    var ct = TestContext.Current.CancellationToken;
    var logger = new RecordingLogger();
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    var attempts = 0;

    var loop = ReconnectLoop.RunAsync(
      async token => {
        if (++attempts == 2) {
          await cts.CancelAsync();
        }

        await Task.Delay(Timeout.Infinite, token);
        return "never";
      },
      (_, _) => Task.CompletedTask,
      new ConnectionFailureLog(logger, "silent"),
      _fast,
      cancellationToken: cts.Token
    );

    await loop;

    var warning = Assert.Single(logger.Entries, e => e.Level == LogLevel.Warning);
    Assert.IsType<TimeoutException>(warning.Exception);
  }



  [Fact(Timeout = 20_000)]
  public async Task TheConnectionIsDisposedAfterItEnds() {
    var ct = TestContext.Current.CancellationToken;
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    var connection = new DisposalProbe();

    await ReconnectLoop.RunAsync(
      _ => Task.FromResult(connection),
      async (_, _) => await cts.CancelAsync(),
      new ConnectionFailureLog(new RecordingLogger(), "target"),
      _fast,
      cancellationToken: cts.Token
    );

    Assert.True(connection.Disposed);
  }



  [Fact(Timeout = 20_000)]
  public async Task TcpConnectorConnectsWithinTheDeadline() {
    var ct = TestContext.Current.CancellationToken;
    var listener = new TcpListener(IPAddress.Loopback, 0);
    using var stopListener = new StopOnDispose(listener);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;

    var configured = false;
    using var client = await TcpConnector.ConnectAsync(
      "127.0.0.1",
      port,
      TimeSpan.FromSeconds(10),
      c => configured = !c.Connected,
      cancellationToken: ct
    );

    Assert.True(client.Connected);
    Assert.True(configured, "configure must run before the connect");
  }



  [Fact(Timeout = 20_000)]
  public async Task TcpConnectorReportsARefusedPortPromptly() {
    var ct = TestContext.Current.CancellationToken;
    int port;
    var probe = new TcpListener(IPAddress.Loopback, 0);
    using (new StopOnDispose(probe)) {
      probe.Start();
      port = ((IPEndPoint)probe.LocalEndpoint).Port;
    }

    var stopwatch = Stopwatch.StartNew();
    var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
      TcpConnector.ConnectAsync("127.0.0.1", port, TimeSpan.FromSeconds(10), cancellationToken: ct)
    );

    Assert.Equal(NetworkFailureKind.Refused, NetworkFailure.Classify(ex));
    Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10));
  }



  private sealed class DisposalProbe : IDisposable {
    public bool Disposed { get; private set; }

    public void Dispose() => Disposed = true;
  }



  /// <summary>TcpListener is only IDisposable from .NET 5 on; this stops it on every target.</summary>
  private sealed class StopOnDispose(TcpListener listener) : IDisposable {
    public void Dispose() => listener.Stop();
  }
}
