# CB.System.Net

*This package is only for testing purpose*.

CB.System.Net is a collection of networking classes to solve frequently occurring problems: typed TCP clients and listeners,
local address and host information, IP end point parsing.

## Upgrading from 0.4.x

`NetMonitor` and `PcapFilterBuilder` (namespace `CB.System.Net.Diagnostics`) moved to the package
[CB.System.Net.Pcap](../CB.System.Net.Pcap/README.md), so that this package no longer depends on SharpPcap. The namespace did not
change: reference `CB.System.Net.Pcap` as well and the code compiles as before.

## Contributors/Contributing

Maintained by [Christian Borck](https://github.com/Borck/)
