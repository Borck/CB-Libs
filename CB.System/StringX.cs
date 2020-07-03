using System;
using JetBrains.Annotations;



namespace CB.System {
  public static class StringX {
    public static (string left, string right) Separate([NotNull] this string @string, char separator) {
      return @string.Separate(@string.IndexOf(separator), 1);
    }



    public static (T left, string right) Separate<T>([NotNull] this string @string,
                                                     char separator,
                                                     Func<string, T> parseLeft) {
      var (left, right) = Separate(@string, separator);
      return (parseLeft(left), right);
    }



    public static (string left, string right) Separate([NotNull] this string @string, params char[] separators) {
      return @string.Separate(@string.IndexOfAny(separators), 1);
    }



    public static (T left, string right) Separate<T>([NotNull] this string @string,
                                                     char[] separators,
                                                     Func<string, T> parseLeft) {
      var (left, right) = Separate(@string, separators);
      return (parseLeft(left), right);
    }



    public static (string left, string right) Separate([NotNull] this string @string, string separator) {
      return @string.Separate(@string.IndexOf(separator), separator.Length);
    }



    public static (T left, string right) Separate<T>([NotNull] this string @string,
                                                     string separator,
                                                     Func<string, T> parseLeft) {
      var (left, right) = Separate(@string, separator);
      return (parseLeft(left), right);
    }



    public static (string left, string right) Separate([NotNull] this string @string,
                                                       string separator,
                                                       StringComparison comparisonType) {
      return @string.Separate(@string.IndexOf(separator, comparisonType), separator.Length);
    }



    public static (T left, string right) Separate<T>([NotNull] this string @string,
                                                     string separator,
                                                     StringComparison comparisonType,
                                                     Func<string, T> parseLeft) {
      var (left, right) = Separate(@string, separator, comparisonType);
      return (parseLeft(left), right);
    }



    public static (string left, string right) SeparateLast([NotNull] this string @string, char separator) {
      return @string.Separate(@string.LastIndexOf(separator), 1);
    }



    public static (string left, T right) SeparateLast<T>([NotNull] this string @string,
                                                         char separator,
                                                         Func<string, T> parseRight)
      => DoParseRightIfNotDefault(@string.SeparateLast(separator), parseRight);



    public static (string left, string right) SeparateLast([NotNull] this string @string, params char[] separators) {
      return @string.Separate(@string.LastIndexOfAny(separators), 1);
    }



    public static (string left, T right) SeparateLast<T>([NotNull] this string @string,
                                                         char[] separators,
                                                         Func<string, T> parseRight)
      => DoParseRightIfNotDefault(@string.SeparateLast(separators), parseRight);



    public static (string left, string right) SeparateLast([NotNull] this string @string, string separator)
      => @string.Separate(@string.LastIndexOf(separator), separator.Length);



    public static (string left, T right) SeparateLast<T>([NotNull] this string @string,
                                                         string separator,
                                                         Func<string, T> parseRight)
      => DoParseRightIfNotDefault(@string.SeparateLast(separator), parseRight);



    public static (string left, string right) SeparateLast([NotNull] this string @string,
                                                           string separator,
                                                           StringComparison comparisonType)
      => @string.Separate(@string.LastIndexOf(separator, comparisonType), separator.Length);



    public static (string left, T right) SeparateLast<T>([NotNull] this string @string,
                                                         string separator,
                                                         Func<string, T> parseRight,
                                                         StringComparison comparisonType)
      => DoParseRightIfNotDefault(@string.SeparateLast(separator, comparisonType), parseRight);



    private static (string left, T right) DoParseRightIfNotDefault<T>((string left, string right) separatedString,
                                                                      Func<string, T> parseRight)
      => (separatedString.left, separatedString.right != default ? parseRight(separatedString.right) : default);



    private static (string left, string right) Separate([NotNull] this string @string,
                                                        int indexOfSeparator,
                                                        int lengthOfSeparator) {
      switch (indexOfSeparator) {
        case -1:
          return (@string, default);
        case 0:
          return (string.Empty,
                   @string.RightSubstring(indexOfSeparator, lengthOfSeparator));
        default:
          return (
                   @string.Substring(0, indexOfSeparator),
                   @string.RightSubstring(indexOfSeparator, lengthOfSeparator)
                 );
      }
    }



    private static string RightSubstring([NotNull] this string @string,
                                         int indexOfSeparator,
                                         int lengthOfSeparator) {
      var rightStartIndex = indexOfSeparator + lengthOfSeparator;
      return @string.Length != rightStartIndex
               ? @string.Substring(rightStartIndex)
               : string.Empty;
    }



    private static bool TrySeparate([NotNull] this string @string,
                                    int indexOfSeparator,
                                    int lengthOfSeparator,
                                    out string left,
                                    out string right) {
      switch (indexOfSeparator) {
        case -1:
          (left, right) = (@string, default);
          return false;
        case 0:
          (left, right) = (string.Empty,
                            @string.RightSubstring(indexOfSeparator, lengthOfSeparator));
          return true;
        default:
          var endIndexOfSepExcl = indexOfSeparator + lengthOfSeparator;
          left = @string.Substring(0, indexOfSeparator);
          right = @string.RightSubstring(indexOfSeparator, lengthOfSeparator);
          return true;
      }
    }



    private static bool TrySeparateParseLeft<T>([NotNull] this string @string,
                                                int indexOfSeparator,
                                                int lengthOfSeparator,
                                                out T left,
                                                out string right,
                                                Func<string, T> parseLeft) {
      var success = @string.TrySeparate(
        indexOfSeparator,
        lengthOfSeparator,
        out var leftString,
        out right
      );
      left = parseLeft(leftString);
      return success;
    }



    private static bool TrySeparateParseRight<T>([NotNull] this string @string,
                                                 int indexOfSeparator,
                                                 int lengthOfSeparator,
                                                 out string left,
                                                 out T right,
                                                 Func<string, T> parseRight) {
      var success = @string.TrySeparate(
        indexOfSeparator,
        lengthOfSeparator,
        out left,
        out var rightString
      );
      right = parseRight(rightString);
      return success;
    }



    public static bool TrySeparate(
      this string @string,
      char separator,
      out string left,
      out string right)
      => @string.TrySeparate(@string.IndexOf(separator), 1, out left, out right);



    public static bool TrySeparate<T>(
      this string @string,
      char separator,
      out T left,
      out string right,
      Func<string, T> parseLeft
    )
      => @string.TrySeparateParseLeft(@string.IndexOf(separator), 1, out left, out right, parseLeft);



    public static bool TrySeparate(
      this string @string,
      char[] separators,
      out string left,
      out string right)
      => @string.TrySeparate(@string.IndexOfAny(separators), 1, out left, out right);



    public static bool TrySeparate<T>(
      this string @string,
      char[] separators,
      out T left,
      out string right,
      Func<string, T> parseLeft
    )
      => @string.TrySeparateParseLeft(@string.IndexOfAny(separators), 1, out left, out right, parseLeft);



    public static bool TrySeparate(
      this string @string,
      string separator,
      out string left,
      out string right)
      => @string.TrySeparate(@string.IndexOf(separator), separator.Length, out left, out right);



    public static bool TrySeparate<T>(
      this string @string,
      string separator,
      out T left,
      out string right,
      Func<string, T> parseLeft
    )
      => @string.TrySeparateParseLeft(@string.IndexOf(separator), separator.Length, out left, out right, parseLeft);



    public static bool TrySeparate(this string @string,
                                   string separator,
                                   StringComparison comparisonType,
                                   out string left,
                                   out string right)
      => @string.TrySeparate(@string.IndexOf(separator, comparisonType), separator.Length, out left, out right);



    public static bool TrySeparate<T>(
      this string @string,
      string separator,
      StringComparison comparisonType,
      out T left,
      out string right,
      Func<string, T> parseLeft
    )
      => @string.TrySeparateParseLeft(
        @string.IndexOf(separator, comparisonType),
        separator.Length,
        out left,
        out right,
        parseLeft
      );



    public static bool TrySeparateLast([NotNull] this string @string,
                                       char separator,
                                       out string left,
                                       out string right)
      => @string.TrySeparate(@string.IndexOf(separator), 1, out left, out right);



    public static bool TrySeparateLast<T>([NotNull] this string @string,
                                          char separator,
                                          out string left,
                                          out T right,
                                          Func<string, T> parseRight)
      => @string.TrySeparateParseRight(@string.IndexOf(separator), 1, out left, out right, parseRight);



    public static bool TrySeparateLast([NotNull] this string @string,
                                       char[] separators,
                                       out string left,
                                       out string right)
      => @string.TrySeparate(@string.IndexOfAny(separators), 1, out left, out right);



    public static bool TrySeparateLast<T>([NotNull] this string @string,
                                          char[] separators,
                                          out string left,
                                          out T right,
                                          Func<string, T> parseRight)
      => @string.TrySeparateParseRight(@string.IndexOfAny(separators), 1, out left, out right, parseRight);



    public static bool TrySeparateLast([NotNull] this string @string,
                                       string separator,
                                       out string left,
                                       out string right)
      => @string.TrySeparate(@string.IndexOf(separator), 1, out left, out right);



    public static bool TrySeparateLast<T>([NotNull] this string @string,
                                          string separator,
                                          out string left,
                                          out T right,
                                          Func<string, T> parseRight)
      => @string.TrySeparateParseRight(@string.IndexOf(separator), 1, out left, out right, parseRight);



    public static bool TrySeparateLast([NotNull] this string @string,
                                       string separator,
                                       StringComparison comparisonType,
                                       out string left,
                                       out string right)
      => @string.TrySeparate(@string.IndexOf(separator, comparisonType), 1, out left, out right);



    public static bool TrySeparateLast<T>([NotNull] this string @string,
                                          string separator,
                                          StringComparison comparisonType,
                                          out string left,
                                          out T right,
                                          Func<string, T> parseRight)
      => @string.TrySeparateParseRight(@string.IndexOf(separator, comparisonType), 1, out left, out right, parseRight);



    [Obsolete("Use TrySeparate instead")]
    public static bool TryHead(this string @string, char[] separators, out string head, out string tail)
      => TrySeparate(@string, separators, out head, out tail);



    [Obsolete("Use TrySeparateLast instead")]
    public static bool TryTail([NotNull] this string @string, char separator, out string head, out string tail)
      => TrySeparateLast(@string, separator, out head, out tail);
  }
}
