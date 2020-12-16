using System.IO;



namespace CB.System.IO {
  public interface IFormatter<TProtoContract> {
    void Serialize(Stream stream, TProtoContract data);
    TProtoContract Deserialize(Stream stream);
  }
}
