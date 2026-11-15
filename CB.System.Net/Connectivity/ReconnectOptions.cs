namespace CB.System.Net.Connectivity;

using global::System;



/// <summary>
///   Timing of one connection's connect/reconnect cycle: the deadlines of a single attempt and the back-off
///   between attempts. Shared by every connection in the framework so a peer link, a controller adapter and an
///   inference client all back off, time out and recover the same way — see <c>docs/engine/Resilience-Rules.md</c>
///   (R2, R3).
/// </summary>
/// <remarks>
///   Everything here is a plain value, so tests shrink the delays instead of waiting them out (R15).
/// </remarks>
public sealed record ReconnectOptions {
  #region static fields and constants

  /// <summary>
  ///   Defaults that suit a LAN peer or a controller: first retry after about a second, never slower than every
  ///   30 s, an attempt that has not connected within 10 s counts as failed.
  /// </summary>
  public static ReconnectOptions Default { get; } = new();

  #endregion

  #region fields and properties

  /// <summary>
  ///   Upper bound of the first retry delay. Later delays grow by <see cref="Multiplier" /> up to
  ///   <see cref="MaxDelay" />.
  /// </summary>
  public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(1);

  /// <summary>
  ///   Upper bound of any retry delay. A remote that has been gone for an hour is retried this often.
  /// </summary>
  public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(30);

  /// <summary>Growth factor per consecutive failed attempt.</summary>
  public double Multiplier { get; init; } = 2.0;

  /// <summary>
  ///   Fraction of each delay that is randomized, <c>0..1</c>. With the default <c>0.5</c> a delay of <c>d</c> is
  ///   drawn from <c>[d/2, d]</c>: never zero, and clients that lost the same server at the same instant do not
  ///   all come back at the same instants.
  /// </summary>
  public double Jitter { get; init; } = 0.5;

  /// <summary>
  ///   How long a connection must stay up before the back-off starts over. A link that is accepted and dropped at
  ///   once — a refused certificate, a licence refusal, a duplicate client id — keeps escalating instead of
  ///   reconnecting in a tight loop.
  /// </summary>
  public TimeSpan StableAfter { get; init; } = TimeSpan.FromSeconds(30);

  /// <summary>
  ///   Deadline of one connect attempt, name resolution included. Never the OS connect timeout (about 21 s on
  ///   Windows, about two minutes on Linux).
  /// </summary>
  public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromSeconds(10);

  /// <summary>
  ///   Deadline of a protocol handshake that follows the connect (TLS, a session start message).
  /// </summary>
  public TimeSpan HandshakeTimeout { get; init; } = TimeSpan.FromSeconds(15);

  #endregion
}
