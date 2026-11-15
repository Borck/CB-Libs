namespace CB.System.Net;

using global::System;
using global::System.IO;
using global::System.Net;
using CB.System.IO;
using Xunit;

public class TypedTcpClientAcceptorTests {
  [Fact]
  public void TestDispose() {
    using (var tcpAcceptor = new TypedTcpClientAcceptor<object>(new IPAddress(0), 0, new Formatter())) {
      tcpAcceptor.Start();
    }
  }



  private class Formatter : IFormatter<object> {
    public void Serialize(Stream stream, object data) => throw new NotImplementedException();



    public object Deserialize(Stream stream) => throw new NotImplementedException();
  }
}
