namespace System;

using global::System.Runtime.CompilerServices;



/// <summary>
///   <c>ArgumentOutOfRangeException.ThrowIfLessThan</c> for the targets that predate it (net48, netstandard2.0); see
///   <see cref="ArgumentNullExceptionPolyfill" />.
/// </summary>
internal static class ArgumentOutOfRangeExceptionPolyfill {
  extension(ArgumentOutOfRangeException) {
    public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
      where T : IComparable<T> {
      if (value.CompareTo(other) < 0) {
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be greater than or equal to '{other}'.");
      }
    }
  }
}
