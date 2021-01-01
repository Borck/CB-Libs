using System;
using System.Runtime.Serialization;
using System.Security.Permissions;



namespace CB.System {
  /// <summary>
  ///   Represents a exception which brings up the solution to solve the source of this problem.
  /// </summary>
  [Obsolete("Their is no replacement, because this kind is not a real exception and should be prevented.")]
  [Serializable]
  public class SolvableException : Exception {
    private readonly Func<bool> _solution;



    public SolvableException(Func<bool> solution) {
      _solution = solution;
    }



    public SolvableException(string message, Func<bool> solution)
      : base(message) {
      _solution = solution;
    }



    public SolvableException(string message, Exception inner, Func<bool> solution)
      : base(message, inner) {
      _solution = solution;
    }



    protected SolvableException(
      SerializationInfo info,
      StreamingContext context,
      Func<bool> solution)
      : base(info, context) {
      _solution = solution;
    }



    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //
    public bool Solved { get; private set; }



    public bool Solve() {
      return Solved || (Solved = _solution());
    }



    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context) {
      base.GetObjectData(info, context);
      if (info == null) {
        throw new ArgumentNullException(nameof(info));
      }

      info.AddValue(nameof(Solved), Solved);
      info.AddValue("Solution", _solution.ToString());
    }
  }
}
