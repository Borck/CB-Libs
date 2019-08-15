using System.IO;
using JetBrains.Annotations;



namespace CB.System.IO {
  public interface IFormatter<TProtoContract> {
    void Serialize([NotNull] Stream stream, TProtoContract data);
    TProtoContract Deserialize([NotNull] Stream stream);
  }
}
