namespace CB.System.IO;

using global::System.Diagnostics;
using global::System.IO;

public class ProcessX {
  public static StreamReader ExecuteCommandLine(string filename, string arguments) {
    var process = new Process {
      StartInfo = {
        FileName = filename,
        Arguments = arguments,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        CreateNoWindow = true
      }
    };
    process.Start();
    return process.StandardOutput;
  }
}
