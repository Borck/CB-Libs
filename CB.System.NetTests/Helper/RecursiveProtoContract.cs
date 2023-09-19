using ProtoBuf;
using System;



namespace CB.System.Net.Helper {
  [Serializable, ProtoContract]
  class RecursiveProtoContract : IRecursiveProtoContract, IEquatable<RecursiveProtoContract> {
    [ProtoMember(1)]
    public int IntValue { get; }

    [ProtoMember(2)]
    public IRecursiveProtoContract? Child { get; }

    private RecursiveProtoContract() { }



    public RecursiveProtoContract(
      int intValue,
      IRecursiveProtoContract? child) {
      IntValue = intValue;
      Child = child;
    }



    public bool Equals(RecursiveProtoContract? other) {
      if (ReferenceEquals(null, other)) {
        return false;
      }

      if (ReferenceEquals(this, other)) {
        return true;
      }

      return IntValue == other.IntValue && Equals(Child, other.Child);
    }



    public override bool Equals(object? obj) {
      if (ReferenceEquals(this, obj)) {
        return true;
      }

      if (ReferenceEquals(null, obj)) {
        return false;
      }

      return obj.GetType() == GetType() &&
             Equals((RecursiveProtoContract)obj);
    }



    public override int GetHashCode() {
      unchecked {
        return (IntValue * 397) ^ (Child != null ? Child.GetHashCode() : 0);
      }
    }



    public static class Factory {
      public static RecursiveProtoContract CreateRecursiveSample(int deep = 2) {
        if (deep <= 0) {
          throw new ArgumentException($"'{nameof(deep)}' must be greater zero");
        }

        var contract = new RecursiveProtoContract(deep, default);
        for (var i = deep - 1; i > 0; i--) {
          contract = new RecursiveProtoContract(i, contract);
        }

        return contract;
      }
    }
  }
}
