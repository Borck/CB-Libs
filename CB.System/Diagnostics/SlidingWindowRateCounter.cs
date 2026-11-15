namespace CB.System.Diagnostics;

using global::System;
using global::System.Collections.Generic;
using global::System.Diagnostics;



/// <summary>
///   Counts events and reports their rate over the last <see cref="Window" />: events per second, e.g. frames per second.
/// </summary>
/// <remarks>Not thread-safe: call <see cref="Tick" /> from one thread, or lock around it.</remarks>
public sealed class SlidingWindowRateCounter {
  private readonly Queue<long> _timestamps = new();
  private readonly long _windowTicks;
  private readonly Func<long> _timestamp;

  /// <summary>Length of the sliding window.</summary>
  public TimeSpan Window { get; }



  /// <exception cref="ArgumentOutOfRangeException"><paramref name="window" /> is not positive.</exception>
  public SlidingWindowRateCounter(TimeSpan window)
    : this(window, Stopwatch.GetTimestamp, Stopwatch.Frequency) { }



  internal SlidingWindowRateCounter(TimeSpan window, Func<long> timestamp, long frequency) {
    if (window <= TimeSpan.Zero) {
      throw new ArgumentOutOfRangeException(nameof(window), window, "The window must be longer than zero.");
    }

    Window = window;
    _windowTicks = (long)(window.TotalSeconds * frequency);
    _timestamp = timestamp;
  }



  /// <summary>Records one event now and returns how many happened in the last <see cref="Window" />, per second.</summary>
  public double Tick() {
    var now = _timestamp();
    _timestamps.Enqueue(now);

    var cutoff = now - _windowTicks;
    while (_timestamps.Count > 0 && _timestamps.Peek() < cutoff) {
      _timestamps.Dequeue();
    }

    return _timestamps.Count / Window.TotalSeconds;
  }
}
