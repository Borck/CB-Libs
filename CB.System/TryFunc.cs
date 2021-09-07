namespace CB.System {
  /// <summary>
  ///   A function delegate applies the try method pattern.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="TOut"></typeparam>
  /// <param name="input"></param>
  /// <param name="value"></param>
  /// <returns></returns>
  public delegate bool TryFunc<T, TOut>(T input, out TOut value);
}
