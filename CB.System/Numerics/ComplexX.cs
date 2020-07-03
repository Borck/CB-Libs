using System.Numerics;



namespace CB.System.Numerics {
  public static class ComplexX {
    /// <summary>
    ///   Swaps the real and the imaginary part
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static Complex Transpose(this Complex c) {
      return new Complex(c.Imaginary, c.Real);
    }
  }
}
