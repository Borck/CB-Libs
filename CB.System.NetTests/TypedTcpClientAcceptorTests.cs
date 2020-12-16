using System;
using System.IO;
using System.Net;
using CB.System.IO;
using Xunit;



namespace CB.System.Net {
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
}
