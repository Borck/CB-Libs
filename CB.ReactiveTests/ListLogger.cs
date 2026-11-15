namespace CB.Reactive;

using global::System;
using global::System.Collections.Generic;
using global::System.Linq;
using Microsoft.Extensions.Logging;



/// <summary>A logger that keeps what it is given, for asserting on what an operator reports.</summary>
internal sealed class ListLogger : ILogger {
  private readonly List<Entry> _entries = [];

  public IReadOnlyList<Entry> Entries {
    get {
      lock (_entries) {
        return _entries.ToList();
      }
    }
  }



  public IDisposable? BeginScope<TState>(TState state)
    where TState : notnull => null;



  public bool IsEnabled(LogLevel logLevel) => true;



  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
    lock (_entries) {
      _entries.Add(new(logLevel, formatter(state, exception), exception));
    }
  }



  internal sealed record Entry(LogLevel Level, string Message, Exception? Exception);
}
