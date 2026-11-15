namespace CB.System.Net;

using global::System;
using global::System.Net;



/// <summary>Extensions for <see cref="IPAddress" />.</summary>
public static class IPAddressX {
  /// <summary>
  ///   Whether the address is this machine: IPv4 loopback (127.0.0.0/8), IPv6 loopback (::1), or either of them written as an
  ///   IPv4-mapped IPv6 address (::ffff:127.0.0.1) - which is how a dual-stack server sees an IPv4 caller on loopback.
  /// </summary>
  /// <remarks>
  ///   <c>null</c> is not loopback. A caller for which "no address" means "in-process" (a test host) decides that itself.
  ///   <see cref="IPAddress.IsLoopback" /> alone does not treat the mapped form as loopback on every target.
  /// </remarks>
  public static bool IsLoopbackOrMapped(this IPAddress? address) =>
    address is not null &&
    (IPAddress.IsLoopback(address) || address.IsIPv4MappedToIPv6 && IPAddress.IsLoopback(address.MapToIPv4()));



  /// <summary>
  ///   Whether a host as written in a URL or a configuration points at this machine: <c>localhost</c> (any case) or an address
  ///   literal that <see cref="IsLoopbackOrMapped" />, with or without the brackets of an IPv6 literal (<c>[::1]</c>).
  /// </summary>
  /// <remarks>It does not resolve names: a host name other than <c>localhost</c> is never loopback here, whatever DNS says.</remarks>
  public static bool IsLoopbackHost(string? host) {
    if (host is null) {
      return false;
    }

    var trimmed = host.Trim().Trim('[', ']');
    return trimmed.Length > 0 &&
           (trimmed.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            IPAddress.TryParse(trimmed, out var address) && address.IsLoopbackOrMapped());
  }
}
