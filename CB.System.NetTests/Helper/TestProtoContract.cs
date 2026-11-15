using ProtoBuf;
using System;



namespace CB.System.Net.Helper {
  class TestProtoContract : ITestProtoContract, IEquatable<TestProtoContract> {
    [ProtoMember(1)]
    public int IntValue { get; }

    [ProtoMember(2)]
    public ITestProtoContract? Child { get; }



    public TestProtoContract(
      int intValue,
      ITestProtoContract? child) {
      IntValue = intValue;
      Child = child;
    }



    public bool Equals(TestProtoContract other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }

      if (ReferenceEquals(this, other)) {
        return true;
      }

      return IntValue == other.IntValue && Equals(Child, other.Child);
    }



    public override bool Equals(object obj) {
      if (ReferenceEquals(this, obj)) {
        return true;
      }

      if (ReferenceEquals(null, obj)) {
        return false;
      }

      if (obj.GetType() != GetType()) {
        return false;
      }

      return Equals((TestProtoContract)obj);
    }



    public override int GetHashCode() {
      unchecked {
        return (IntValue * 397) ^ (Child != null ? Child.GetHashCode() : 0);
      }
    }



    public static class Factory {
      public static TestProtoContract CreateSample(int deep = 2) {
        //TODO ProtoBufSerializer throws System.InvalidOperationException because of recursive object structure.
        if (deep <= 0) {
          throw new ArgumentException($"'{nameof(deep)}' must be greater zero");
        }

        TestProtoContract contract = null;
        for (var i = deep; i > 0; i--) {
          contract = new TestProtoContract(i, contract);
        }

        return contract;
      }
    }
  }
}
