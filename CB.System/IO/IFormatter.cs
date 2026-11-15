namespace CB.System.IO;

using global::System.IO;

public interface IFormatter<TProtoContract> {
  public void Serialize(Stream stream, TProtoContract data);
  public TProtoContract Deserialize(Stream stream);
}
