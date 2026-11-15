namespace CB.System;

using global::System;

public static class EnumX {
  /// <summary>
  /// Sets the flag at the enum to true.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="value"></param>
  /// <param name="flagToSet"></param>
  /// <returns></returns>
  public static T Add<T>(this Enum value, T flagToSet) => (T)(object)((int)(object)value | (int)(object)flagToSet);



  /// <summary>
  /// Sets the flag at the enum to false.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="value"></param>
  /// <param name="flagToUnset"></param>
  /// <returns></returns>
  public static T Substract<T>(this Enum value, T flagToUnset) => (T)(object)((int)(object)value & ~(int)(object)flagToUnset);
}
