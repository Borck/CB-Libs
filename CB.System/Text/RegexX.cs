using System;
using System.Text.RegularExpressions;



namespace CB.System.Text {
  public static class RegexX {
    /// <summary>
    ///   Tries to match a regular expression with a string and returns true of the match was successful, otherwise false.
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="input"></param>
    /// <param name="match">the precise result as a Match object</param>
    /// <returns></returns>
    public static bool TryMatch(this Regex regex, string input, out Match match) {
      match = regex.Match(input);
      return match.Success;
    }



    /// <summary>
    ///   Tries to match a regular expression with a string and returns true of the match was successful, otherwise false.
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="input"></param>
    /// <param name="startAt"></param>
    /// <param name="match">the precise result as a Match object</param>
    /// <returns></returns>
    public static bool TryMatch(this Regex regex, string input, int startAt, out Match match) {
      match = regex.Match(input, startAt);
      return match.Success;
    }



    /// <summary>
    ///   Tries to match a regular expression with a string and returns true of the match was successful, otherwise false.
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="input"></param>
    /// <param name="beginning"></param>
    /// <param name="length"></param>
    /// <param name="match">the precise result as a Match object</param>
    /// <returns></returns>
    public static bool TryMatch(this Regex regex, string input, int beginning, int length, out Match match) {
      match = regex.Match(input, beginning, length);
      return match.Success;
    }
    
    
    
    /// <summary>
    ///   Converts a wildcard patterns to a regex pattern
    /// </summary>
    /// <param name="wildcardPattern"></param>
    /// <returns></returns>
    public static string ConvertWildcards(string wildcardPattern) {
      return Regex.Escape(wildcardPattern)
                  .Replace(@"\*", ".*")
                  .Replace(@"\?", ".");

      //TODO not working correctly if input string is "\*\?"
    }



    /// <summary>
    ///   Converts a wildcard patterns to a regex pattern
    /// </summary>
    /// <param name="wildcardPattern"></param>
    /// <returns></returns>
    public static Regex CreateRegexFromWildcards(string wildcardPattern) {
      return new Regex(ConvertWildcards(wildcardPattern));
    }



    /// <summary>
    ///   Converts a wildcard patterns to a regex pattern
    /// </summary>
    /// <param name="wildcardPattern"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static Regex CreateRegexFromWildcards(string wildcardPattern, RegexOptions options) {
      return new Regex(ConvertWildcards(wildcardPattern), options);
    }



    /// <summary>
    ///   Converts a wildcard patterns to a regex pattern
    /// </summary>
    /// <param name="wildcardPattern"></param>
    /// <param name="options"></param>
    /// <param name="matchTimeout"></param>
    /// <returns></returns>
    public static Regex CreateRegexFromWildcards(string wildcardPattern, RegexOptions options, TimeSpan matchTimeout) {
      return new Regex(ConvertWildcards(wildcardPattern), options, matchTimeout);
    }
  }
}
