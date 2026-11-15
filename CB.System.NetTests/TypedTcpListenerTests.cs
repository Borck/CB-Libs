namespace CB.System.Net;

using global::System;
using global::System.IO;
using global::System.Net;
using global::System.Net.Sockets;
using global::System.Threading;
using global::System.Threading.Tasks;
using CB.System.IO;
using Xunit;



public class TypedTcpListenerTests {
  [Fact(Timeout = 10000)]
  public async Task AcceptTcpClientAsync_WithAConnectingClient_ExpectTheTypedClient() {
    var ct = TestContext.Current.CancellationToken;
    var listener = new TypedTcpListener<object>(IPAddress.Loopback, 0, new Formatter());
    listener.Start();
    using var stop = new StopOnDispose(listener.Listener);

    var accepting = listener.AcceptTcpClientAsync(ct);
    using var connecting = new TcpClient();
    await connecting.ConnectAsync(IPAddress.Loopback, listener.LocalEndPoint.Port, ct);

    var accepted = await accepting.WaitAsync(ct);

    Assert.True(accepted.Connected);
    accepted.Dispose();
  }



  [Fact(Timeout = 10000)]
  public async Task AcceptTcpClientAsync_WithATokenThatIsCancelled_ExpectOperationCanceledException() {
    var ct = TestContext.Current.CancellationToken;
    var listener = new TypedTcpListener<object>(IPAddress.Loopback, 0, new Formatter());
    listener.Start();
    using var stop = new StopOnDispose(listener.Listener);
    using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);

    var accepting = listener.AcceptTcpClientAsync(cancellation.Token);
    cancellation.Cancel();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => accepting);
  }



  [Fact(Timeout = 10000)]
  public async Task AcceptTcpClientAsync_WithAnAlreadyCancelledToken_ExpectOperationCanceledException() {
    var listener = new TypedTcpListener<object>(IPAddress.Loopback, 0, new Formatter());
    listener.Start();
    using var stop = new StopOnDispose(listener.Listener);
    using var cancellation = new CancellationTokenSource();
    cancellation.Cancel();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => listener.AcceptTcpClientAsync(cancellation.Token));
  }



  /// <summary>TcpListener is only IDisposable from .NET 5 on; this stops it on every target.</summary>
  private sealed class StopOnDispose(TcpListener listener) : IDisposable {
    public void Dispose() => listener.Stop();
  }



  private sealed class Formatter : IFormatter<object> {
    public void Serialize(Stream stream, object data) => throw new NotImplementedException();



    public object Deserialize(Stream stream) => throw new NotImplementedException();
  }
}
