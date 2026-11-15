namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Collections.Concurrent;
using global::System.Net;
using global::System.Net.Sockets;
using global::System.Threading;
using global::System.Threading.Tasks;



/// <summary>
///   The outcome of <see cref="HostResolver.ResolveAsync" />.
/// </summary>
/// <param name="Addresses">The addresses to connect to, in resolver order. Never empty.</param>
/// <param name="FromCache">
///   <c>true</c> when name resolution failed and these are the last addresses that resolved successfully.
/// </param>
/// <param name="ResolutionError">Why name resolution failed, when <paramref name="FromCache" /> is set.</param>
public readonly record struct ResolvedHost(IPAddress[] Addresses, bool FromCache, Exception? ResolutionError = null);



/// <summary>
///   Resolves host names with a deadline and a last-known-good fallback, so that a transient name-resolution
///   failure does not fail a connect attempt whose target is perfectly reachable.
/// </summary>
/// <remarks>
///   <para>
///     Every connect used to re-resolve the name and fail on the spot with <c>SocketException (11001): No such
///     host is known</c> whenever the name server hiccupped, the network was still coming up at boot, or an
///     mDNS/NetBIOS name flapped on an industrial network — while the host itself answered on the address it had
///     a minute earlier. This remembers the last successful answer per name and falls back to it only when
///     resolution itself fails. The connect that follows stays the authority: if the host really moved, that
///     connect fails and the next attempt resolves again.
///   </para>
///   <para>
///     IP literals never touch DNS. A name that is not even syntactically a host name (a URL typed into a host
///     field) is rejected as <see cref="ArgumentException" />, which <see cref="NetworkFailure" /> classifies as
///     <see cref="NetworkFailureKind.InvalidEndpoint" />. See <c>docs/engine/Resilience-Rules.md</c>, R4.
///   </para>
/// </remarks>
public sealed class HostResolver {
  #region static fields and constants

  /// <summary>Deadline of one lookup when the caller does not give one.</summary>
  public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

  #endregion

  #region fields and properties

  private readonly ConcurrentDictionary<string, CacheEntry> _lastKnownGood = new(StringComparer.OrdinalIgnoreCase);
  private readonly Func<string, CancellationToken, Task<IPAddress[]>> _lookup;
  private readonly TimeProvider _time;

  /// <summary>The process-wide resolver every connection uses by default.</summary>
  public static HostResolver Default { get; } = new();

  /// <summary>
  ///   How old a remembered answer may be and still be used when resolution fails. Beyond this, the failure is
  ///   reported as it is.
  /// </summary>
  public TimeSpan MaxStaleness { get; init; } = TimeSpan.FromDays(1);

  #endregion

  #region constructors

  /// <param name="lookup">The actual name lookup; <see cref="Dns.GetHostAddressesAsync(string, CancellationToken)" /> by default.</param>
  /// <param name="timeProvider">Clock for <see cref="MaxStaleness" />.</param>
  public HostResolver(Func<string, CancellationToken, Task<IPAddress[]>>? lookup = null,
                      TimeProvider? timeProvider = null) {
    _lookup = lookup ?? DefaultLookup;
    _time = timeProvider ?? TimeProvider.System;
  }

  #endregion

  #region methods and others

  /// <summary>
  ///   Resolves <paramref name="host" />, falling back to its last successful resolution if the lookup fails.
  /// </summary>
  /// <exception cref="ArgumentException"><paramref name="host" /> is empty or not a host name.</exception>
  /// <exception cref="SocketException">Resolution failed and nothing usable is remembered.</exception>
  /// <exception cref="TimeoutException">The lookup did not finish within <paramref name="timeout" />.</exception>
  public async Task<ResolvedHost> ResolveAsync(string host,
                                               TimeSpan? timeout = null,
                                               CancellationToken cancellationToken = default) {
    if (string.IsNullOrWhiteSpace(host)) {
      throw new ArgumentException("The host name is empty.", nameof(host));
    }

    host = host.Trim();

    if (IPAddress.TryParse(host, out var literal)) {
      return new([literal], false);
    }

    if (!LooksLikeAHostName(host)) {
      throw new ArgumentException($"'{host}' is not a host name or IP address.", nameof(host));
    }

    var deadline = timeout ?? DefaultTimeout;
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    if (deadline != Timeout.InfiniteTimeSpan) {
      cts.CancelAfter(deadline);
    }

    try {
      // WaitAsync as well as the token: not every platform's resolver honours cancellation, and a lookup that
      // ignores it must not hold the connect attempt past its deadline.
      var addresses = await _lookup(host, cts.Token).WaitAsync(cts.Token).ConfigureAwait(false);
      if (addresses.Length == 0) {
        throw new SocketException((int)SocketError.HostNotFound);
      }

      _lastKnownGood[host] = new(addresses, _time.GetTimestamp());
      return new(addresses, false);
    } catch (Exception ex) when (!cancellationToken.IsCancellationRequested && IsResolutionFailure(ex)) {
      var failure = ex is OperationCanceledException
                      ? new TimeoutException($"Resolving '{host}' did not complete within {deadline}.", ex)
                      : ex;

      if (_lastKnownGood.TryGetValue(host, out var entry) && _time.GetElapsedTime(entry.ResolvedAt) <= MaxStaleness) {
        return new(entry.Addresses, true, failure);
      }

      if (ReferenceEquals(failure, ex)) {
        throw;
      }

      throw failure;
    }
  }



  /// <summary>Forgets every remembered answer.</summary>
  public void Clear() => _lastKnownGood.Clear();



  /// <summary>
  ///   Rejects only what can never be a host name — a URL, a path, an embedded space — and leaves every other
  ///   judgement to the resolver. Deliberately not <c>Uri.CheckHostName</c>: it refuses names that Windows
  ///   resolves perfectly well (NetBIOS/LLMNR names with underscores are common on plant networks), and turning
  ///   a working host name into "invalid endpoint" would be the opposite of what this type is for.
  /// </summary>
  internal static bool LooksLikeAHostName(string host) =>
    !host.Contains("://", StringComparison.Ordinal) &&
    host.IndexOfAny(['/', '\\', ' ', '\t', '?', '#', '@']) < 0;



  private static bool IsResolutionFailure(Exception ex) =>
    ex is SocketException or OperationCanceledException or TimeoutException;

  #endregion

  #region nested types

  private static Task<IPAddress[]> DefaultLookup(string host, CancellationToken cancellationToken) =>
#if NET6_0_OR_GREATER
    Dns.GetHostAddressesAsync(host, cancellationToken);
#else
    Dns.GetHostAddressesAsync(host); // no token overload; the caller's WaitAsync stops waiting for it
#endif



  private sealed record CacheEntry(IPAddress[] Addresses, long ResolvedAt);

  #endregion
}
