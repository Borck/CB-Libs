using System.IO;
using CB.System.IO;
using ProtoBuf;
using ProtoBuf.Meta;



namespace CB.System.Runtime.Serialization {
  public class ProtoBufSerializer<TData> : IFormatter<TData> {
    private readonly PrefixStyle _prefixStyle;
    private readonly RuntimeTypeModel _model;

    #region constructors

    public ProtoBufSerializer(PrefixStyle prefixStyle, RuntimeTypeModel model) {
      _prefixStyle = prefixStyle;
      _model = model;
    }
    
    public ProtoBufSerializer(RuntimeTypeModel model)
      : this(PrefixStyle.Fixed32, model) { }



    public ProtoBufSerializer(PrefixStyle prefixStyle)
      : this(prefixStyle, RuntimeTypeModel.Default) { }



    public ProtoBufSerializer()
      : this(PrefixStyle.Fixed32) { }

    #endregion

    #region methods and others

    public void Prepare()
      => _model[typeof(TData)].CompileInPlace();



    public void Serialize(Stream stream, TData data) =>
      _model.SerializeWithLengthPrefix(stream, data, typeof(TData), _prefixStyle, 0);



    public TData Deserialize(Stream stream)
      => (TData)RuntimeTypeModel.Default.DeserializeWithLengthPrefix(stream, null, typeof(TData), _prefixStyle, 0);

    #endregion
  }
}
