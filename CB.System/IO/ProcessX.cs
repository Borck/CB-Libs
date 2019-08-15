using System.Diagnostics;
using System.IO;



namespace CB.System.IO {
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
}
