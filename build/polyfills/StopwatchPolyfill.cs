namespace System;

using global::System.Diagnostics;



/// <summary><c>Stopwatch.GetElapsedTime</c> (.NET 7+) for the targets that predate it. See <see cref="ArgumentNullExceptionPolyfill" />.</summary>
internal static class StopwatchPolyfill {
  extension(Stopwatch) {
    public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) =>
      new((long)((endingTimestamp - startingTimestamp) * ((double)TimeSpan.TicksPerSecond / Stopwatch.Frequency)));



    public static TimeSpan GetElapsedTime(long startingTimestamp) => Stopwatch.GetElapsedTime(startingTimestamp, Stopwatch.GetTimestamp());
  }
}
