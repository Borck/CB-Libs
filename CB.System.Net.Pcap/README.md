# CB.System.Net.Pcap

Traffic monitoring (`NetMonitor`) and packet capture filter building (`PcapFilterBuilder`) on top of
[SharpPcap](https://github.com/dotpcap/sharppcap).

Until 0.4.x these types were part of `CB.System.Net`. They moved here so that `CB.System.Net` no longer drags SharpPcap into
every application that only needs its TCP and address helpers. The namespace (`CB.System.Net.Diagnostics`) did not change;
add a reference to this package.

Capturing needs Npcap (Windows) or libpcap (Linux, macOS) and the right to open a capture device.

## Contributors/Contributing

Maintained by [Christian Borck](https://github.com/Borck/)
