namespace CB.System.Numerics;

using global::System.Numerics;

public static class ComplexX {
  /// <summary>
  ///   Swaps the real and the imaginary part
  /// </summary>
  /// <param name="c"></param>
  /// <returns></returns>
  public static Complex Transpose(this Complex c) => new(c.Imaginary, c.Real);
}
