namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Net.NetworkInformation;
using global::System.Threading;
using global::System.Threading.Tasks;



/// <summary>
///   Lets a reconnect wait end early when the machine's network changes — a cable re-plugged, a VPN or Wi-Fi
///   coming up, an address assigned — instead of sleeping out a back-off of up to half a minute while the remote
///   has been reachable again for most of it.
/// </summary>
/// <remarks>
///   Built on <see cref="NetworkChange" />. Where that is not available (some containers, restricted platforms)
///   waits simply run their full length. See <c>docs/engine/Resilience-Rules.md</c>, R3.
/// </remarks>
public static class NetworkChangeSignal {
  #region static fields and constants

  /// <summary>
  ///   How long to let the network settle after a change before the wait returns: an address change is usually
  ///   followed by DHCP and DNS catching up.
  /// </summary>
  public static readonly TimeSpan SettleTime = TimeSpan.FromSeconds(1);

  private static TaskCompletionSource _next = NewSignal();
  private static int _hooked;

  #endregion

  #region methods and others

  /// <summary>
  ///   Waits for <paramref name="delay" />, or less when the network changes in the meantime.
  /// </summary>
  /// <returns><c>true</c> when the wait was cut short by a network change.</returns>
  /// <exception cref="OperationCanceledException"><paramref name="cancellationToken" /> was cancelled.</exception>
  public static async Task<bool> DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default) {
    cancellationToken.ThrowIfCancellationRequested();
    if (delay <= TimeSpan.Zero) {
      return false;
    }

    var change = NextChangeAsync();
    using var delayCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var wait = Task.Delay(delay, delayCts.Token);

    if (await Task.WhenAny(wait, change).ConfigureAwait(false) != change) {
      await wait.ConfigureAwait(false); // propagates cancellation
      return false;
    }

    await delayCts.CancelAsync().ConfigureAwait(false);
    await Task.Delay(delay < SettleTime ? delay : SettleTime, cancellationToken).ConfigureAwait(false);
    return true;
  }



  /// <summary>A task that completes on the next network change.</summary>
  public static Task NextChangeAsync() {
    EnsureHooked();
    return Volatile.Read(ref _next).Task;
  }



  /// <summary>Signals a network change, as <see cref="NetworkChange" /> would. For tests.</summary>
  internal static void Raise() => Interlocked.Exchange(ref _next, NewSignal()).TrySetResult();



  private static void EnsureHooked() {
    if (Interlocked.Exchange(ref _hooked, 1) != 0) {
      return;
    }

    try {
      NetworkChange.NetworkAddressChanged += (_, _) => Raise();
      NetworkChange.NetworkAvailabilityChanged += (_, _) => Raise();
    } catch (Exception ex) when (ex is PlatformNotSupportedException or NetworkInformationException
                                   or InvalidOperationException) {
      // No change notifications here; every wait runs its full length, which is the behaviour without this type.
    }
  }



  private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

  #endregion
}
