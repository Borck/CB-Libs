#if NET6_0_OR_GREATER
namespace CB.System.Net.Diagnostics;

using global::System;
using global::System.Diagnostics;



// The native implementation: the adapters' own byte counters.
public partial class NetMonitor {
  private readonly Func<long> _readReceivedBytes;

  private long _baseline;



  /// <summary>Creates a monitor of the machine's received traffic.</summary>
  /// <param name="pcapFilter">Must be empty: the native counters cannot be filtered (<see cref="PlatformNotSupportedException" /> otherwise).</param>
  /// <exception cref="PlatformNotSupportedException">A filter was given.</exception>
  public NetMonitor(string pcapFilter = "")
    : this(pcapFilter, () => NetworkThroughputMeter.ReadAdapterBytes(true), Stopwatch.GetTimestamp, Stopwatch.Frequency) { }



  /// <summary>Creates a monitor filtered by <paramref name="filterBuilder" />; refused here, see <see cref="SupportsFilter" />.</summary>
  /// <exception cref="PlatformNotSupportedException">Always on this target.</exception>
  public NetMonitor(PcapFilterBuilder filterBuilder)
    : this((filterBuilder ?? throw new ArgumentNullException(nameof(filterBuilder))).ToString()) { }



  internal NetMonitor(string pcapFilter, Func<long> readReceivedBytes, Func<long> timestamp, long frequency) {
    if (!string.IsNullOrWhiteSpace(pcapFilter)) {
      throw new PlatformNotSupportedException(
        "A traffic filter needs packet capture, which this target does not use: it measures the adapters' byte counters, " +
        "which cannot be filtered. Packet capture (SharpPcap) is the fallback of net48 and netstandard2.0."
      );
    }

    _readReceivedBytes = readReceivedBytes;
    _timestamp = timestamp;
    _frequency = frequency;
  }



  private void PrepareStart() => _baseline = _readReceivedBytes();



  // A counter that went backwards (an adapter was removed or reset) is not negative traffic.
  private long CollectReceivedBytes() => Math.Max(0, _readReceivedBytes() - _baseline);



  private static void ReleaseResources() { }
}
#endif
