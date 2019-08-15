﻿using JetBrains.Annotations;



namespace CB.System {
  public static class StringX {
    public static (string left, string right) Separate([NotNull] this string @string, params char[] separators) {
      return @string.Separate( @string.IndexOfAny( separators ) );
    }



    public static (string left, string right) Separate([NotNull] this string @string, char separator) {
      return @string.Separate( @string.IndexOf( separator ) );
    }



    public static (string left, string right) SeparateLast([NotNull] this string @string, params char[] separators) {
      return @string.Separate( @string.LastIndexOfAny( separators ) );
    }



    public static (string left, string right) SeparateLast([NotNull] this string @string, char separator) {
      return @string.Separate( @string.LastIndexOf( separator ) );
    }



    private static (string left, string right) Separate([NotNull] this string @string, int indexOfSeparator) {
      switch (indexOfSeparator) {
        case -1:
          return ( @string, default(string) );
        case 0:
          return ( string.Empty, @string );
        default:
          return (
                   @string.Substring( 0, indexOfSeparator ),
                   @string.Length != indexOfSeparator + 1
                     ? @string.Substring( indexOfSeparator + 1 )
                     : string.Empty );
      }
    }



    public static bool TryHead(this string @string, char[] separators, out string head, out string tail) {
      var strings = @string.Split( separators, 2 );
      if (strings.Length == 2) {
        head = strings[0];
        tail = strings[1];
        return true;
      }

      head = @string;
      tail = default(string);
      return false;
    }



    public static bool TryTail([NotNull] this string @string, char separator, out string head, out string tail) {
      var idx = @string.LastIndexOf( separator );
      switch (idx) {
        case -1:
          head = @string;
          tail = default(string);
          return false;
        case 0:
          head = "";
          tail = idx + 1 < @string.Length
                   ? @string.Substring( idx + 1 )
                   : "";
          return true;
        default:
          head = @string.Substring( 0, idx );
          tail = idx + 1 < @string.Length
                   ? @string.Substring( idx + 1 )
                   : "";
          return true;
      }
    }
  }
}
