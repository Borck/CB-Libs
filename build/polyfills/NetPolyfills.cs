namespace System;

using global::System.Net;
using global::System.Net.Sockets;
using global::System.Threading;
using global::System.Threading.Tasks;



/// <summary>
///   Small BCL members that net48 and netstandard2.0 lack, with the semantics the newer targets have: a connect that honours a
///   token, <c>CancelAsync</c>, <c>Random.Shared</c> and <c>Math.Clamp</c>. See <see cref="ArgumentNullExceptionPolyfill" />.
/// </summary>
internal static class NetPolyfills {
  /// <summary><c>TcpClient.ConnectAsync(IPAddress[], int, CancellationToken)</c> (.NET 5+). The connect itself is not aborted by the token; the caller disposes the client.</summary>
  public static Task ConnectAsync(this TcpClient client, IPAddress[] addresses, int port, CancellationToken cancellationToken) =>
    client.ConnectAsync(addresses, port).WaitAsync(cancellationToken);



  /// <summary><c>CancellationTokenSource.CancelAsync()</c> (.NET 8+).</summary>
  public static Task CancelAsync(this CancellationTokenSource source) {
    source.Cancel();
    return Task.CompletedTask;
  }



  extension(Random) {
    /// <summary>A thread-safe shared instance (.NET 6+).</summary>
    public static Random Shared => SharedRandom.Instance;
  }



  extension(Math) {
    public static double Clamp(double value, double min, double max) => value < min ? min : value > max ? max : value;



    public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;



    public static long Clamp(long value, long min, long max) => value < min ? min : value > max ? max : value;
  }



  private sealed class SharedRandom : Random {
    private readonly object _gate = new();

    public static SharedRandom Instance { get; } = new();



    public override int Next() {
      lock (_gate) {
        return base.Next();
      }
    }



    public override int Next(int maxValue) {
      lock (_gate) {
        return base.Next(maxValue);
      }
    }



    public override int Next(int minValue, int maxValue) {
      lock (_gate) {
        return base.Next(minValue, maxValue);
      }
    }



    public override double NextDouble() {
      lock (_gate) {
        return base.NextDouble();
      }
    }



    public override void NextBytes(byte[] buffer) {
      lock (_gate) {
        base.NextBytes(buffer);
      }
    }
  }
}
