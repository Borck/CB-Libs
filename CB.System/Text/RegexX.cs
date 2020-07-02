using System;
using System.Text.RegularExpressions;



namespace CB.System.Text {
  public static class RegexX {
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
