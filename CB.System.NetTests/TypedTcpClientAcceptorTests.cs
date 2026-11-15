namespace CB.System.Net;

using global::System;
using global::System.IO;
using global::System.Net;
using global::System.Net.Sockets;
using global::System.Threading.Tasks;
using CB.System.IO;
using Xunit;

// The type under test is obsolete (use TypedTcpClient); it is still shipped, so it is still tested.
#pragma warning disable CS0618

public class TypedTcpClientAcceptorTests {
  [Fact(Timeout = 10000)]
  public void Dispose_WithAStartedAcceptor_ExpectNoCrash() {
    using var tcpAcceptor = new TypedTcpClientAcceptor<object>(IPAddress.Loopback, 0, new Formatter());
    tcpAcceptor.Start();
  }



  /// <summary>
  ///   Start-then-dispose races the accept loop against the disposal of its cancellation source. Once that raced to an
  ///   unhandled exception on a thread-pool task, which ends the whole process (intermittently: the run aborted with exit code 134).
  /// </summary>
  [Fact(Timeout = 30000)]
  public void Dispose_WithManyStartedAcceptors_ExpectNoCrash() {
    for (var i = 0; i < 200; i++) {
      using var tcpAcceptor = new TypedTcpClientAcceptor<object>(IPAddress.Loopback, 0, new Formatter());
      tcpAcceptor.Start();
    }
  }



  [Fact(Timeout = 10000)]
  public async Task ClientAccepted_WithAConnectingClient_ExpectTheEventRaised() {
    using var tcpAcceptor = new TypedTcpClientAcceptor<object>(IPAddress.Loopback, 0, new Formatter());
    var accepted = new TaskCompletionSource<TypedTcpClient<object>>(TaskCreationOptions.RunContinuationsAsynchronously);
    tcpAcceptor.ClientAccepted += (_, client) => accepted.TrySetResult(client);
    tcpAcceptor.Start();

    using var connecting = new TcpClient();
    await connecting.ConnectAsync(IPAddress.Loopback, tcpAcceptor.LocalEndPoint.Port);

    Assert.NotNull(await accepted.Task);
  }



  [Fact(Timeout = 10000)]
  public void Start_AfterDispose_ExpectObjectDisposedException() {
    var tcpAcceptor = new TypedTcpClientAcceptor<object>(IPAddress.Loopback, 0, new Formatter());
    tcpAcceptor.Dispose();

    Assert.Throws<ObjectDisposedException>(tcpAcceptor.Start);
  }



  private sealed class Formatter : IFormatter<object> {
    public void Serialize(Stream stream, object data) => throw new NotImplementedException();



    public object Deserialize(Stream stream) => throw new NotImplementedException();
  }
}
