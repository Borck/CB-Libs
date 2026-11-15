namespace CB.System.Net.Helper;

using ProtoBuf;

[ProtoContract]
[ProtoInclude(1, typeof(TestProtoContract))]
public interface ITestProtoContract {
  public int IntValue { get; }


  public ITestProtoContract Child { get; }
}
