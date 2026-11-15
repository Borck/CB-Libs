using System.IO;
using ProtoBuf.Meta;
using Xunit;



namespace CB.System.Runtime.Serialization {
  public class ProtoBufSerializerTests {
    // Not attributed: only a model that declares it can (de)serialize it. The default model knows nothing about it.
    private sealed class Undeclared {
      public int Value { get; set; }
    }



    private static RuntimeTypeModel CreateModelDeclaring<T>(string member) {
      var model = RuntimeTypeModel.Create();
      model.Add(typeof(T), false).Add(1, member);
      return model;
    }



    [Fact]
    public void SerializeDeserialize_WithCustomModel_ExpectRoundTrip() {
      var serializer = new ProtoBufSerializer<Undeclared>(CreateModelDeclaring<Undeclared>(nameof(Undeclared.Value)));
      using var stream = new MemoryStream();

      serializer.Serialize(stream, new Undeclared { Value = 42 });
      stream.Position = 0;

      // Used to throw: Deserialize read from RuntimeTypeModel.Default instead of the model given to the constructor.
      Assert.Equal(42, serializer.Deserialize(stream).Value);
    }
  }
}
