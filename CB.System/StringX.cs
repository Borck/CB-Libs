using System;
using JetBrains.Annotations;



namespace CB.System {
  public static class StringX {
    public static (string left, string right) Separate([NotNull] this string @string, params char[] separators) {
      return @string.Separate(@string.IndexOfAny(separators));
    }



    public static (string left, string right) Separate([NotNull] this string @string, char separator) {
      return @string.Separate(@string.IndexOf(separator));
    }



    public static (string left, string right) SeparateLast([NotNull] this string @string, params char[] separators) {
      return @string.Separate(@string.LastIndexOfAny(separators));
    }



    public static (string left, string right) SeparateLast([NotNull] this string @string, char separator) {
      return @string.Separate(@string.LastIndexOf(separator));
    }



    private static (string left, string right) Separate([NotNull] this string @string, int indexOfSeparator) {
      switch (indexOfSeparator) {
        case -1:
          return (@string, default);
        case 0:
          return (string.Empty, @string);
        default:
          return (
                   @string.Substring(0, indexOfSeparator),
                   @string.Length != indexOfSeparator + 1
                     ? @string.Substring(indexOfSeparator + 1)
                     : string.Empty);
      }
    }



    public static bool TrySeparate(
      this string @string,
      char[] separators,
      out string first,
      out string rest) {
      var strings = @string.Split(separators, 2);
      if (strings.Length == 2) {
        first = strings[0];
        rest = strings[1];
        return true;
      }

      first = @string;
      rest = default;
      return false;
    }



    public static bool TrySeparate<T>(this string @string,
                                      char[] separators,
                                      out T first,
                                      out string rest,
                                      Func<string, T> parseFirst) {
      var success = TrySeparate(@string, separators, out var firstString, out rest);
      first = success ? parseFirst(firstString) : default;
      return success;
    }



    public static bool TrySeparateLast([NotNull] this string @string,
                                       char separator,
                                       out string rest,
                                       out string last) {
      var idx = @string.LastIndexOf(separator);
      switch (idx) {
        case -1:
          rest = @string;
          last = default;
          return false;
        case 0:
          rest = "";
          last = idx + 1 < @string.Length
                   ? @string.Substring(idx + 1)
                   : "";
          return true;
        default:
          rest = @string.Substring(0, idx);
          last = idx + 1 < @string.Length
                   ? @string.Substring(idx + 1)
                   : "";
          return true;
      }
    }



    public static bool TrySeparateLast<T>([NotNull] this string @string,
                                          char separator,
                                          out string rest,
                                          out T last,
                                          Func<string, T> parseLast) {
      var success = TrySeparateLast(@string, separator, out var lastString, out rest);
      last = success ? parseLast(lastString) : default;
      return success;
    }



    [Obsolete("Use TrySeparate instead")]
    public static bool TryHead(this string @string, char[] separators, out string head, out string tail)
      => TrySeparate(@string, separators, out head, out tail);



    [Obsolete("Use TrySeparateLast instead")]
    public static bool TryTail([NotNull] this string @string, char separator, out string head, out string tail)
      => TrySeparateLast(@string, separator, out head, out tail);
  }
}
