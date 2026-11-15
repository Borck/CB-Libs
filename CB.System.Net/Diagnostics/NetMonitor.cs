namespace CB.System.Net.Diagnostics;

using global::System;
using global::System.Diagnostics;
using global::System.Threading;



/// <summary>
///   Measures the network traffic the machine receives between <see cref="Start" /> and <see cref="Stop" />.
/// </summary>
/// <remarks>
///   <para>
///     On net10.0 and later it is native: the byte counters of the network adapters (loopback excluded), the same source as
///     <see cref="NetworkThroughputMeter" />. That counts all traffic of the machine and cannot be filtered, so a filter is
///     refused with <see cref="PlatformNotSupportedException" /> (<see cref="SupportsFilter" /> says so up front).
///   </para>
///   <para>
///     On net48 and netstandard2.0 packet capture through SharpPcap is the fallback, for those targets have no equivalent that
///     can filter: it needs Npcap (Windows) or libpcap (Linux, macOS) and the right to open a capture device, and it accepts a pcap
///     filter (see <see cref="PcapFilterBuilder" />).
///   </para>
/// </remarks>
public partial class NetMonitor : IDisposable {
  private readonly Func<long> _timestamp;
  private readonly long _frequency;

  private long _startTimestamp;

  /// <summary>Whether <see cref="Start" /> has been called and <see cref="Stop" /> has not.</summary>
  public bool Started { get; private set; }

  /// <summary>Bytes received between the last <see cref="Start" /> and <see cref="Stop" />; 0 until the first <see cref="Stop" />.</summary>
  public long ReceivedBytes { get; private set; }

  /// <summary>Bytes per second over that interval; -1 if it was too short to tell, 0 until the first <see cref="Stop" />.</summary>
  public long BytesPerSecond { get; private set; }

  /// <summary>Whether this target can restrict the measurement with a filter (packet capture): true on net48 and netstandard2.0, false from net10.0 on.</summary>
  public static bool SupportsFilter =>
#if NET6_0_OR_GREATER
    false;
#else
    true;
#endif



  /// <summary>Starts measuring the traffic received from now until <see cref="Stop" />.</summary>
  /// <exception cref="InvalidOperationException">On net48 and netstandard2.0: the capture library is not installed.</exception>
  public void Start() {
    PrepareStart();
    _startTimestamp = _timestamp();
    Started = true;
  }



  /// <summary>Ends the measurement and sets <see cref="ReceivedBytes" /> and <see cref="BytesPerSecond" />.</summary>
  /// <exception cref="InvalidOperationException">It was not started.</exception>
  public void Stop() {
    if (!Started) {
      throw new InvalidOperationException(nameof(NetMonitor) + " is not started.");
    }

    var elapsedMilliseconds = (_timestamp() - _startTimestamp) * 1000.0 / _frequency;
    ReceivedBytes = CollectReceivedBytes();
    BytesPerSecond = elapsedMilliseconds >= 1
                       ? (long)(ReceivedBytes * 1000.0 / elapsedMilliseconds)
                       : -1;
    Started = false;
  }



  /// <summary>Releases what the measurement holds (capture devices on net48 and netstandard2.0).</summary>
  public void Dispose() {
    ReleaseResources();
    GC.SuppressFinalize(this);
  }



  private static long Now() => Stopwatch.GetTimestamp();
}
