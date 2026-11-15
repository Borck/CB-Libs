namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Collections.Generic;
using global::System.IO;
using global::System.Net.Http;
using global::System.Net.Sockets;
using global::System.Security.Authentication;



/// <summary>
///   Why a connection attempt failed, in the terms an operator acts on.
/// </summary>
public enum NetworkFailureKind {
  /// <summary>Not a recognised network failure.</summary>
  Other,

  /// <summary>The host name could not be resolved.</summary>
  HostNotFound,

  /// <summary>The host answered, but nothing accepts connections on that port.</summary>
  Refused,

  /// <summary>There is no route to the host (network down, wrong subnet, firewall rejecting).</summary>
  Unreachable,

  /// <summary>No answer within the deadline (host off, or packets silently dropped).</summary>
  TimedOut,

  /// <summary>The TLS handshake was refused by either side.</summary>
  TlsRejected,

  /// <summary>An established connection was closed or reset.</summary>
  ConnectionLost,

  /// <summary>The configured endpoint itself is not valid; retrying will not help until it changes.</summary>
  InvalidEndpoint
}



/// <summary>
///   Classifies connection exceptions into a <see cref="NetworkFailureKind" /> and describes each kind in a way
///   an operator can act on, so a log line says "host not found — check the name or DNS" instead of carrying
///   only <c>SocketException (11001)</c> and a stack trace.
/// </summary>
public static class NetworkFailure {
  #region methods and others

  /// <summary>
  ///   Classifies <paramref name="exception" />. The exception chain (inner exceptions, and every inner exception
  ///   of an <see cref="AggregateException" />) is searched outermost first for a type that says precisely what
  ///   happened; wrapper types only decide when nothing more specific is found.
  /// </summary>
  public static NetworkFailureKind Classify(Exception? exception) {
    if (exception == null) {
      return NetworkFailureKind.Other;
    }

    var chain = Flatten(exception);

    foreach (var ex in chain) {
      if (ClassifyPrecise(ex) is { } kind) {
        return kind;
      }
    }

    foreach (var ex in chain) {
      switch (ex) {
        case EndOfStreamException:
        case IOException:
        case ObjectDisposedException:
          return NetworkFailureKind.ConnectionLost;
        case OperationCanceledException:
          return NetworkFailureKind.TimedOut;
      }
    }

    return NetworkFailureKind.Other;
  }



  /// <summary>
  ///   Whether retrying the same endpoint can succeed without anything being reconfigured.
  /// </summary>
  public static bool IsTransient(NetworkFailureKind kind) => kind != NetworkFailureKind.InvalidEndpoint;



  /// <summary>A short, operator-facing reason and what to check.</summary>
  public static string Describe(NetworkFailureKind kind) =>
    kind switch {
      NetworkFailureKind.HostNotFound =>
        "host not found: the name could not be resolved - check the host name and DNS, or use an IP address",
      NetworkFailureKind.Refused =>
        "connection refused: the host is reachable but nothing accepts connections on that port - is the " +
        "service running, is the port right?",
      NetworkFailureKind.Unreachable =>
        "host unreachable: no route to the host - check the cable, network, subnet or firewall",
      NetworkFailureKind.TimedOut =>
        "timed out: no answer within the deadline - the host may be off, or a firewall drops the packets",
      NetworkFailureKind.TlsRejected =>
        "TLS rejected: the handshake was refused - check the certificates on both sides",
      NetworkFailureKind.ConnectionLost => "connection lost: closed or reset by the remote side or the network",
      NetworkFailureKind.InvalidEndpoint => "invalid endpoint: the configured address is not valid",
      _ => "unexpected error"
    };



  private static NetworkFailureKind? ClassifyPrecise(Exception ex) =>
    ex switch {
      SocketException socket => ClassifySocketError(socket.SocketErrorCode),
      AuthenticationException => NetworkFailureKind.TlsRejected,
      TimeoutException => NetworkFailureKind.TimedOut,
#if NET8_0_OR_GREATER
      // Older targets have no HttpRequestError; the SocketException inside the HttpRequestException is classified instead.
      HttpRequestException { HttpRequestError: HttpRequestError.NameResolutionError } =>
        NetworkFailureKind.HostNotFound,
      HttpRequestException { HttpRequestError: HttpRequestError.SecureConnectionError } =>
        NetworkFailureKind.TlsRejected,
#endif
      UriFormatException or FormatException or ArgumentException => NetworkFailureKind.InvalidEndpoint,
      _ => null
    };



  private static NetworkFailureKind? ClassifySocketError(SocketError error) =>
    error switch {
      SocketError.HostNotFound or SocketError.TryAgain or SocketError.NoData or SocketError.NoRecovery =>
        NetworkFailureKind.HostNotFound,
      SocketError.ConnectionRefused => NetworkFailureKind.Refused,
      SocketError.HostUnreachable or SocketError.NetworkUnreachable or SocketError.NetworkDown or
        SocketError.HostDown or SocketError.AddressNotAvailable => NetworkFailureKind.Unreachable,
      SocketError.TimedOut => NetworkFailureKind.TimedOut,
      SocketError.ConnectionReset or SocketError.ConnectionAborted or SocketError.Shutdown or
        SocketError.NotConnected or SocketError.Disconnecting or SocketError.NetworkReset or
        SocketError.OperationAborted => NetworkFailureKind.ConnectionLost,
      _ => null
    };



  private static List<Exception> Flatten(Exception root) {
    var result = new List<Exception>();
    var pending = new Queue<Exception>();
    pending.Enqueue(root);

    // Bounded: a pathological self-referencing chain must not hang the logger that called us.
    while (pending.Count > 0 && result.Count < 32) {
      var ex = pending.Dequeue();
      result.Add(ex);

      if (ex is AggregateException aggregate) {
        foreach (var inner in aggregate.InnerExceptions) {
          pending.Enqueue(inner);
        }
      } else if (ex.InnerException is { } inner) {
        pending.Enqueue(inner);
      }
    }

    return result;
  }

  #endregion
}
