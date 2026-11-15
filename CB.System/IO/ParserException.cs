namespace CB.System.IO;

using global::System;

public class ParserException : Exception {
  public ParserException(
    string filename,
    int lineIndex,
    string message,
    Exception innerException)
    : base(CreateMessage(filename, lineIndex, message), innerException) { }



  public ParserException(
    string filename,
    int lineIndex,
    string message)
    : base(CreateMessage(filename, lineIndex, message)) { }



  public ParserException(
    string filename,
    int lineIndex,
    Exception innerException)
    : base(CreateMessage(filename, lineIndex), innerException) { }



  private static string CreateMessage(
    string filename,
    int lineIndex,
    string message) => $"{CreateMessage(filename, lineIndex)}: {message}";



  private static string CreateMessage(
    string filename,
    int lineIndex) => $"Failed reading {filename}:{lineIndex}";
}
