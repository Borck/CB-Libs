#if !NET6_0_OR_GREATER
namespace CB.System.Net.Diagnostics;

using global::System;
using global::System.Threading;
using global::System.Threading.Tasks;
using SharpPcap;



// The fallback for targets without a native, filterable counter: packet capture through SharpPcap.
public partial class NetMonitor {
  private readonly Task<CaptureDeviceList> _captureDevices;

  private long _receivedBytes;



  /// <summary>Creates a monitor that captures packets on every device, restricted by a pcap filter.</summary>
  /// <param name="pcapFilter">A pcap filter expression (see <see cref="PcapFilterBuilder" />); empty captures everything.</param>
  public NetMonitor(string pcapFilter = "") {
    _timestamp = Now;
    _frequency = global::System.Diagnostics.Stopwatch.Frequency;
    _captureDevices = Task.Factory.StartNew(
      () => {
        try {
          var captureDevices = CaptureDeviceList.Instance;
          foreach (var captureDevice in captureDevices) {
            captureDevice.Open();
            captureDevice.OnPacketArrival += OnPacketReceived;
            captureDevice.StartCapture();
            captureDevice.Filter = pcapFilter;
          }

          return captureDevices;
        } catch (DllNotFoundException e) {
          throw new InvalidOperationException(
            "Could not load dll for traffic monitoring (PCAP), maybe Npcap or libpcap is not installed.",
            e
          );
        }
      }
    );
  }



  /// <summary>Creates a monitor restricted by a filter built with <paramref name="filterBuilder" />.</summary>
  public NetMonitor(PcapFilterBuilder filterBuilder)
    : this((filterBuilder ?? throw new ArgumentNullException(nameof(filterBuilder))).ToString()) { }



  private void PrepareStart() {
    _captureDevices.ConfigureAwait(false)
                   .GetAwaiter()
                   .GetResult();
    Interlocked.Exchange(ref _receivedBytes, 0);
  }



  private long CollectReceivedBytes() => Interlocked.Read(ref _receivedBytes);



  private void OnPacketReceived(object sender, PacketCapture packetCapture) {
    if (Started) {
      Interlocked.Add(ref _receivedBytes, packetCapture.Data.Length);
    }
  }



  private void ReleaseResources() {
    foreach (var captureDevice in _captureDevices.Result) {
      captureDevice.StopCapture();
      captureDevice.OnPacketArrival -= OnPacketReceived;
      captureDevice.Close();
    }

    _captureDevices.Dispose();
  }
}
#endif
