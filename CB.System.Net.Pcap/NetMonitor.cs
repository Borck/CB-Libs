using SharpPcap;
using System;
using System.Threading.Tasks;



namespace CB.System.Net.Diagnostics {
  public class NetMonitor : IDisposable {
    private DateTime _lastStart;

    private readonly Task<CaptureDeviceList> _captureDevices;
    public bool Started { get; private set; }


    private long _receivedBytes;

    public long ReceivedBytes { get; private set; }

    public long BytesPerSecond { get; private set; }



    public NetMonitor(string pcapFilter = "") {
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
          }
          catch (DllNotFoundException e) {
            throw new InvalidOperationException(
              "Could not load dll for traffic monitoring (PCAP), maybe WinPCAP is not installed ",
              e
            );
          }
        }
      );
    }



    public NetMonitor(PcapFilterBuilder filterBuilder)
      : this(filterBuilder.ToString()) { }



    public void Start() {
      _captureDevices.ConfigureAwait(false)
                     .GetAwaiter()
                     .GetResult();

      Started = true;
      _receivedBytes = 0;
      _lastStart = DateTime.Now;
    }



    public void Stop() {
      if (!Started)
        throw new InvalidOperationException(nameof(NetMonitor) + " is not started.");

      var timeSpan = (DateTime.Now - _lastStart).Milliseconds;

      ReceivedBytes = _receivedBytes;
      BytesPerSecond = timeSpan > 0
                         ? ReceivedBytes * 1000 / timeSpan
                         : -1;

      Started = false;
    }



    private void OnPacketReceived(object sender, PacketCapture packetCapture) {
      if (Started)
        _receivedBytes += packetCapture.Data.Length;
    }



    public void Dispose() {
      foreach (var captureDevice in _captureDevices.Result) {
        captureDevice.StopCapture();
        captureDevice.OnPacketArrival -= OnPacketReceived;
        captureDevice.Close();
      }

      _captureDevices.Dispose();
    }
  }
}
