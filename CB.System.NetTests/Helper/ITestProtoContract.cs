using ProtoBuf;



namespace CB.System.Net.Helper {
  [ProtoContract]
  [ProtoInclude( 1, typeof(TestProtoContract) )]
  public interface ITestProtoContract {
    int IntValue { get; }


    ITestProtoContract Child { get; }
  }
}
