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

## Addresses

`IPAddressX.IsLoopbackOrMapped()` and `IPAddressX.IsLoopbackHost(host)` - loopback including the IPv4-mapped IPv6 form a dual-stack server
sees (`IPAddress.IsLoopback` alone does not cover it on every target).

## Upgrading from 0.4.x

`NetMonitor` and `PcapFilterBuilder` (namespace `CB.System.Net.Diagnostics`) moved to the package
[CB.System.Net.Pcap](../CB.System.Net.Pcap/README.md), so that this package no longer depends on SharpPcap. The namespace did not
change: reference `CB.System.Net.Pcap` as well and the code compiles as before.

## Contributors/Contributing

Maintained by [Christian Borck](https://github.com/Borck/)
