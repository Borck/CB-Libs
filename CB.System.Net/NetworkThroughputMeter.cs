namespace CB.System.Net;

using global::System;
using global::System.Diagnostics;
using global::System.Linq;
using global::System.Net.NetworkInformation;



/// <summary>
///   Measured network traffic of the whole machine in one direction, in decimal megabytes per second: the bytes the adapters (loopback
///   excluded) have moved since the previous <see cref="NextMBps" />, over the time between the two calls.
/// </summary>
/// <remarks>The first call only takes a baseline and returns 0. A counter that went backwards (an adapter was removed or reset) reads as 0 for that interval. Not thread-safe.</remarks>
public sealed class NetworkThroughputMeter {
  private readonly Func<long> _readBytes;
  private readonly Func<long> _timestamp;
  private readonly long _frequency;

  private long _previousBytes = -1;
  private long _previousTimestamp;



  /// <param name="incoming">Received bytes if true, sent bytes if false.</param>
  public NetworkThroughputMeter(bool incoming)
    : this(() => ReadAdapterBytes(incoming), Stopwatch.GetTimestamp, Stopwatch.Frequency) { }



  internal NetworkThroughputMeter(Func<long> readBytes, Func<long> timestamp, long frequency) {
    _readBytes = readBytes;
    _timestamp = timestamp;
    _frequency = frequency;
  }



  /// <summary>The average rate since the previous call, MB/s; 0 for the first call and for a counter that did not advance.</summary>
  public float NextMBps() {
    var currentBytes = _readBytes();
    var timestamp = _timestamp();

    if (_previousBytes == -1) {
      _previousBytes = currentBytes;
      _previousTimestamp = timestamp;
      return 0;
    }

    var seconds = (timestamp - _previousTimestamp) / (double)_frequency;
    var diff = currentBytes - _previousBytes;
    _previousBytes = currentBytes;
    _previousTimestamp = timestamp;

    return seconds > 0 && diff > 0 ? (float)(diff / 1_000_000.0 / seconds) : 0f;
  }



  /// <summary>The bytes received or sent so far over all adapters except loopback (the operating system's counters).</summary>
  internal static long ReadAdapterBytes(bool incoming) {
    var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                                     .Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                     .ToList();
    return incoming
             ? interfaces.Sum(i => i.GetIPStatistics().BytesReceived)
             : interfaces.Sum(i => i.GetIPStatistics().BytesSent);
  }
}
