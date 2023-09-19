using ProtoBuf;



namespace CB.System.Net.Helper {
  [ProtoContract, ProtoInclude(100, typeof(RecursiveProtoContract))]
  public interface IRecursiveProtoContract {
    int IntValue { get; }


    IRecursiveProtoContract Child { get; }
  }
}
