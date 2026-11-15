namespace CB.System.Net;

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Net;
using global::System.Net.NetworkInformation;
using global::System.Net.Sockets;



/// <summary>A network adapter as far as choosing the one that bounds this machine's traffic is concerned.</summary>
/// <param name="Name">The adapter's name (<c>eth0</c>, <c>Ethernet</c>).</param>
/// <param name="Description">The adapter's description, which is where a virtual one gives itself away.</param>
/// <param name="BitsPerSecond">The speed the adapter reports.</param>
/// <param name="Status">Whether it is up.</param>
/// <param name="Type">Ethernet, wireless, loopback, tunnel...</param>
/// <param name="HasGateway">Whether it has a default gateway: it leads somewhere beyond its own network.</param>
/// <param name="IsRouteInterface">Whether the operating system routes outgoing traffic through it.</param>
public readonly record struct NetworkCard(
  string Name,
  string Description,
  long BitsPerSecond,
  OperationalStatus Status,
  NetworkInterfaceType Type,
  bool HasGateway,
  bool IsRouteInterface = false);



/// <summary>
///   The nominal speed of the network card this machine really talks through, in decimal megabytes per second (1 MB/s = 8 Mbit/s).
///   Nominal, not measured: what the link negotiated, not what the network behind it delivers.
/// </summary>
/// <remarks>
///   The adapters are not equal. A virtual switch, a VPN, a container bridge and Bluetooth report speeds (often 10 Gbit/s) that
///   have nothing to do with the wire, so they are skipped unless the operating system routes through them. The order is: an adapter
///   named by the caller, then the one the OS routes outgoing traffic through, then the fastest connected physical adapter with a
///   gateway, then the fastest connected physical adapter.
/// </remarks>
public static class NetworkLinkSpeed {
  private static readonly string[] _virtualMarkers = [
    "vethernet", "hyper-v", "virtual", "vmware", "vmnet", "virtualbox", "vbox", "wsl", "docker", "veth", "virbr",
    "br-", "tap", "tun", "zerotier", "tailscale", "wireguard", "loopback", "bluetooth"
  ];

  private static readonly string[] _routeProbes = ["192.0.2.1", "2001:db8::1"];



  /// <summary>Reads this machine's adapters and returns the chosen one's speed, 0 when there is none or the system will not say.</summary>
  /// <param name="interfaceName">An adapter to use, by name or part of its description; ignored if there is no such connected adapter.</param>
  public static float GetMBps(string? interfaceName = null) {
    try {
      var routeAddress = RouteAddress();
      return SelectMBps(NetworkInterface.GetAllNetworkInterfaces().Select(nic => Describe(nic, routeAddress)), interfaceName);
    } catch (Exception ex) when (ex is NetworkInformationException or PlatformNotSupportedException) {
      return 0f;
    }
  }



  /// <summary>The choice itself, over adapters the caller describes: see the class remarks for the order. 0 when none qualifies.</summary>
  public static float SelectMBps(IEnumerable<NetworkCard> cards, string? interfaceName = null) {
    ArgumentNullException.ThrowIfNull(cards);
    var all = cards.ToList();

    if (interfaceName is not null && !string.IsNullOrWhiteSpace(interfaceName)) {
      var named = all.Where(c => IsConnected(c) &&
                                 (string.Equals(c.Name, interfaceName, StringComparison.OrdinalIgnoreCase) ||
                                  c.Description.Contains(interfaceName, StringComparison.OrdinalIgnoreCase)))
                     .ToList();
      if (named.Count > 0) {
        return ToMBps(named.Max(c => c.BitsPerSecond));
      }
    }

    // What the operating system itself sends traffic through is the card that bounds it, virtual or not.
    var routed = all.Where(c => c.IsRouteInterface && IsConnected(c)).ToList();
    if (routed.Count > 0) {
      return ToMBps(routed.Max(c => c.BitsPerSecond));
    }

    var usable = all.Where(c => IsConnected(c) && !IsVirtual(c)).ToList();
    var candidates = usable.Where(c => c.HasGateway).ToList();
    if (candidates.Count == 0) {
      candidates = usable;
    }

    return candidates.Count == 0 ? 0f : ToMBps(candidates.Max(c => c.BitsPerSecond));
  }



  private static float ToMBps(long bitsPerSecond) => bitsPerSecond / 8f / 1_000_000f;



  private static bool IsConnected(NetworkCard card) =>
    card.Status == OperationalStatus.Up &&
    card.BitsPerSecond > 0 &&
    card.Type is not (NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel);



  private static bool IsVirtual(NetworkCard card) =>
    _virtualMarkers.Any(marker => card.Name.Contains(marker, StringComparison.OrdinalIgnoreCase) ||
                                card.Description.Contains(marker, StringComparison.OrdinalIgnoreCase));



  /// <summary>
  ///   The local address the OS picks to reach an outside address. A UDP "connect" sends nothing: it only asks the routing table. Documentation
  ///   addresses (RFC 5737 / 3849) are used so that no real host is ever involved.
  /// </summary>
  private static IPAddress? RouteAddress() {
    foreach (var probe in _routeProbes) {
      try {
        var address = IPAddress.Parse(probe);
        using var socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(new IPEndPoint(address, 9));
        if (socket.LocalEndPoint is IPEndPoint { Address: { } local } && !IPAddress.IsLoopback(local)) {
          return local;
        }
      } catch (Exception ex) when (ex is SocketException or PlatformNotSupportedException) {
        // No route for this address family: try the next, or fall back to the heuristic.
      }
    }

    return null;
  }



  private static NetworkCard Describe(NetworkInterface nic, IPAddress? routeAddress) {
    var hasGateway = false;
    var isRoute = false;
    try {
      var properties = nic.GetIPProperties();
      hasGateway = properties.GatewayAddresses.Any(g => !IPAddress.Any.Equals(g.Address) && !IPAddress.IPv6Any.Equals(g.Address));
      isRoute = routeAddress is not null && properties.UnicastAddresses.Any(a => a.Address.Equals(routeAddress));
    } catch (Exception ex) when (ex is NetworkInformationException or PlatformNotSupportedException) {
      // An adapter whose IP properties cannot be read counts as neither.
    }

    return new(nic.Name, nic.Description, nic.Speed, nic.OperationalStatus, nic.NetworkInterfaceType, hasGateway, isRoute);
  }
}
