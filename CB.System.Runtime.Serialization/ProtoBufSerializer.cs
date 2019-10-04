using System.IO;
using CB.System.IO;
using ProtoBuf;



namespace CB.System.Runtime.Serialization {
  public class ProtoBufSerializer<TData> : IFormatter<TData> {
    private readonly PrefixStyle _prefixStyle;



    public ProtoBufSerializer(PrefixStyle prefixStyle) {
      _prefixStyle = prefixStyle;
    }



    public ProtoBufSerializer()
      : this( PrefixStyle.Fixed32 ) { }



    public void Prepare()
      => Serializer.PrepareSerializer<TData>();



    public void Serialize(Stream stream, TData data) {
      Serializer.SerializeWithLengthPrefix( stream, data, _prefixStyle );
    }



    public TData Deserialize(Stream stream)
      => Serializer.DeserializeWithLengthPrefix<TData>( stream, _prefixStyle );
  }
}
