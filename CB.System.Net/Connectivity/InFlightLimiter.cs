namespace CB.System.Net.Connectivity;

using global::System.Threading;



/// <summary>
///   Bounds how many requests to a remote are in flight at once, dropping — not queueing — what does not fit:
///   for real-time sensor data the freshest frame is what matters, the same policy as the latest-frame slot of a
///   peer link's frame stream. Thread-safe.
/// </summary>
/// <remarks>
///   The shape it replaces is <c>frames.Subscribe(async frame => await Remote(frame))</c>: one request per frame,
///   none of them bounded, so a remote that slowed down or stopped answering accumulated one pending request per
///   frame — each holding its input — and memory grew at frame rate until it answered. See
///   <c>docs/engine/Resilience-Rules.md</c>, R12.
/// </remarks>
public sealed class InFlightLimiter(int maxInFlight) {
  #region fields and properties

  private int _dropping;
  private long _dropped;
  private int _inFlight;

  /// <summary>The configured bound.</summary>
  public int MaxInFlight { get; } = maxInFlight > 0 ? maxInFlight : 1;

  /// <summary>Requests currently in flight.</summary>
  public int InFlight => Volatile.Read(ref _inFlight);

  /// <summary>Requests dropped since this limiter was created.</summary>
  public long Dropped => Interlocked.Read(ref _dropped);

  #endregion

  #region methods and others

  /// <summary>
  ///   Claims a slot. Every <c>true</c> must be followed by exactly one <see cref="Exit" />.
  /// </summary>
  /// <param name="startedDropping">
  ///   <c>true</c> on the first refusal after a period without any — the moment worth logging once.
  /// </param>
  public bool TryEnter(out bool startedDropping) {
    // Compare-and-swap rather than increment-then-undo: a refused caller must never raise the count, or
    // InFlight briefly reads above MaxInFlight while it backs out.
    var current = Volatile.Read(ref _inFlight);
    while (current < MaxInFlight) {
      var seen = Interlocked.CompareExchange(ref _inFlight, current + 1, current);
      if (seen == current) {
        startedDropping = false;
        return true;
      }

      current = seen;
    }

    Interlocked.Increment(ref _dropped);
    startedDropping = Interlocked.Exchange(ref _dropping, 1) == 0;
    return false;
  }



  /// <summary>Releases a slot claimed by <see cref="TryEnter" />.</summary>
  /// <returns><c>true</c> when requests had been dropped since the last recovery — the remote caught up.</returns>
  public bool Exit() {
    Interlocked.Decrement(ref _inFlight);
    return Interlocked.Exchange(ref _dropping, 0) == 1;
  }

  #endregion
}
