# CB.System.Net

*This package is only for testing purpose*.

CB.System.Net is a collection of networking classes to solve frequently occurring problems: typed TCP clients and listeners,
local address and host information, IP end point parsing.

## Connectivity (`CB.System.Net.Connectivity`)

The building blocks of a connection that has to survive: a network that comes and goes, a host that restarts, a DNS that is down.

- `ReconnectLoop.RunAsync(...)` - connect, use, reconnect; `ReconnectLoop.WithDeadlineAsync(...)` puts a deadline on any step.
- `ReconnectBackoff` / `ReconnectOptions` - exponential, capped, jittered delays, reset only once a connection has been stable.
- `ReconnectPacer` + `ConnectionFailureLog` - pace the retries and log each cause once, then the recovery with the outage's duration.
- `HostResolver` - DNS with a deadline and a last-known-good fallback.
- `TcpConnector.ConnectAsync(...)` - resolve and connect within one deadline.
- `NetworkFailure.Classify(...)` - refused, unreachable, host not found, timed out, TLS rejected, lost: for logs and for the decision to retry.
- `InFlightLimiter`, `NetworkChangeSignal` - bound outstanding work; wake a waiting retry when the network changes.

Logging goes through `Microsoft.Extensions.Logging.Abstractions`; the clocks are `TimeProvider`s, so every delay is testable.

## Adapters and traffic

- `NetworkLinkSpeed.GetMBps()` - the nominal speed of the card the machine really talks through (virtual switches, VPNs and container bridges
  that claim 10 Gbit/s are skipped unless the OS routes through them); `SelectMBps(cards, name)` is the choice over adapters you describe.
- `NetworkThroughputMeter(incoming)` - measured MB/s over all non-loopback adapters between two calls.

## Addresses

`IPAddressX.IsLoopbackOrMapped()` and `IPAddressX.IsLoopbackHost(host)` - loopback including the IPv4-mapped IPv6 form a dual-stack server
sees (`IPAddress.IsLoopback` alone does not cover it on every target).

## Traffic monitoring (`NetMonitor`)

`NetMonitor.Start()` / `Stop()` measure the traffic the machine received in between: `ReceivedBytes` and `BytesPerSecond`.

- **net10.0 and later:** native, from the byte counters of the network adapters (loopback excluded). Nothing to install. They count all traffic of
  the machine and cannot be filtered: a constructor with a filter throws `PlatformNotSupportedException` (`NetMonitor.SupportsFilter` tells).
- **net48 and netstandard2.0:** packet capture through [SharpPcap](https://github.com/dotpcap/sharppcap) is the fallback, since those targets have
  no native counter that can filter. It needs Npcap (Windows) or libpcap (Linux, macOS) and the right to open a capture device, and it
  accepts a pcap filter - build one with `PcapFilterBuilder`. SharpPcap is referenced for those two targets only.

## Upgrading from 0.4.x

`NetMonitor` and `PcapFilterBuilder` stay in this package and namespace (`CB.System.Net.Diagnostics`). On net10.0 and later `NetMonitor` is native
and refuses a filter (see above); on net48 and netstandard2.0 it works as before, through SharpPcap. SharpPcap is no longer a dependency of
the net10.0 and net11.0 builds. Also, `NetMonitor.BytesPerSecond` was computed from `TimeSpan.Milliseconds` (the 0-999 part) instead of the elapsed
milliseconds, so it was wrong for any interval over a second; that is fixed.
