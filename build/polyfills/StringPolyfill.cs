namespace System;

/// <summary><c>string.Contains(string, StringComparison)</c> (.NET Core 2.1+) for the targets that predate it. See <see cref="ArgumentNullExceptionPolyfill" />.</summary>
internal static class StringPolyfill {
  public static bool Contains(this string text, string value, StringComparison comparisonType) => text.IndexOf(value, comparisonType) >= 0;
}
