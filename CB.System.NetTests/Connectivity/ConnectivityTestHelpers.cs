namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Collections.Concurrent;
using global::System.Collections.Generic;
using global::System.Linq;
using Microsoft.Extensions.Logging;



/// <summary>A clock the test moves by hand.</summary>
internal sealed class ManualTimeProvider(DateTimeOffset start) : TimeProvider {
  public DateTimeOffset Now { get; private set; } = start;

  public ManualTimeProvider() : this(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)) { }

  private long _ticks;

  public override DateTimeOffset GetUtcNow() => Now;

  // The monotonic clock moves with the wall clock here: elapsed time is measured with GetTimestamp, not GetUtcNow.
  public override long TimestampFrequency => TimeSpan.TicksPerSecond;

  public override long GetTimestamp() => _ticks;

  public void Advance(TimeSpan by) {
    Now += by;
    _ticks += by.Ticks;
  }
}



/// <summary>Records every log entry so a test can assert on levels and counts.</summary>
internal sealed class RecordingLogger : ILogger {
  private readonly ConcurrentQueue<(LogLevel Level, string Message, Exception? Exception)> _entries = new();

  public IReadOnlyList<(LogLevel Level, string Message, Exception? Exception)> Entries => _entries.ToList();

  public IDisposable? BeginScope<TState>(TState state)
    where TState : notnull =>
    null;

  public bool IsEnabled(LogLevel logLevel) => true;

  public void Log<TState>(LogLevel logLevel,
                          EventId eventId,
                          TState state,
                          Exception? exception,
                          Func<TState, Exception?, string> formatter) =>
    _entries.Enqueue((logLevel, formatter(state, exception), exception));

  public int Count(LogLevel level) => Entries.Count(e => e.Level == level);
}
