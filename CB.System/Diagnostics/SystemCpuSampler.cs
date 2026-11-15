namespace CB.System.Diagnostics;

using global::System;
using global::System.Diagnostics;
using global::System.Globalization;
using global::System.IO;
using global::System.Runtime.InteropServices;
using global::System.Threading;



/// <summary>The kernel's cumulative CPU time counters: the machine's idle time, and all of its time (busy plus idle).</summary>
public readonly record struct SystemCpuTimes(ulong Idle, ulong Total);



/// <summary>
///   Machine-wide CPU load, 0..1, from the kernel's counters (<c>GetSystemTimes</c> on Windows, <c>/proc/stat</c> on Linux), smoothed
///   with an exponential moving average so that a single spike does not move it much and a sustained change does.
/// </summary>
/// <remarks>
///   Not the load of this process. The first reading only takes a baseline, so the value is 0 until the second sample at least
///   250 ms later. Thread-safe.
/// </remarks>
public sealed class SystemCpuSampler {
  /// <summary>The time constant when none is given: 15 s.</summary>
  public static readonly TimeSpan DefaultSmoothing = TimeSpan.FromSeconds(15);

  private static readonly TimeSpan _minSampleInterval = TimeSpan.FromMilliseconds(250);
  private static readonly char[] _space = [' '];

  private readonly Func<long> _clock;
  private readonly Lock _gate = new();
  private readonly Func<SystemCpuTimes?>? _read;
  private readonly TimeSpan _smoothing;

  private float _smoothed;
  private bool _seeded;
  private SystemCpuTimes _previous;
  private long _previousTimestamp;
  private bool _hasPrevious;

  /// <summary>Whether this operating system offers the counters. When it does not, <see cref="NextValue" /> is always 0.</summary>
  public bool IsSupported => _read is not null;



  /// <param name="smoothing">The time constant of the average; <see cref="DefaultSmoothing" /> if null, no smoothing at zero.</param>
  public SystemCpuSampler(TimeSpan? smoothing = null)
    : this(ReadCurrentCpuTimes, Stopwatch.GetTimestamp, smoothing ?? DefaultSmoothing) { }



  /// <summary>For tests and for a custom source: the counters, the clock (a <see cref="Stopwatch" /> timestamp) and the time constant.</summary>
  public SystemCpuSampler(Func<SystemCpuTimes?>? read, Func<long> clock, TimeSpan smoothing) {
    _read = read;
    _clock = clock;
    _smoothing = smoothing < TimeSpan.Zero ? TimeSpan.Zero : smoothing;
  }



  /// <summary>Takes a sample and returns the smoothed busy share since the previous one; the last value when it is too soon or unreadable.</summary>
  public float NextValue() {
    if (_read is null) {
      return 0f;
    }

    lock (_gate) {
      var now = _read();
      var timestamp = _clock();
      if (now is not { } current) {
        return _smoothed;
      }

      if (!_hasPrevious) {
        _previous = current;
        _previousTimestamp = timestamp;
        _hasPrevious = true;
        return _smoothed;
      }

      var elapsed = Stopwatch.GetElapsedTime(_previousTimestamp, timestamp);
      if (elapsed < _minSampleInterval || current.Total <= _previous.Total) {
        return _smoothed;
      }

      var raw = Fraction(_previous, current);
      _previous = current;
      _previousTimestamp = timestamp;

      _smoothed = _seeded ? Smooth(_smoothed, raw, elapsed, _smoothing) : raw;
      _seeded = true;
      return _smoothed;
    }
  }



  /// <summary>The busy share, 0..1, between two readings; 0 when the counters did not advance or went backwards.</summary>
  public static float Fraction(SystemCpuTimes previous, SystemCpuTimes current) {
    var total = current.Total - previous.Total;
    if (current.Total <= previous.Total || total == 0) {
      return 0f;
    }

    var idle = current.Idle >= previous.Idle ? current.Idle - previous.Idle : 0UL;
    return (float)Math.Clamp(1.0 - (double)idle / total, 0.0, 1.0);
  }



  /// <summary>One step of an exponential moving average: <paramref name="previous" /> moves towards <paramref name="sample" /> by <c>1 - e^(-elapsed / timeConstant)</c>.</summary>
  public static float Smooth(float previous, float sample, TimeSpan elapsed, TimeSpan timeConstant) {
    if (timeConstant <= TimeSpan.Zero) {
      return sample;
    }

    var alpha = 1.0 - Math.Exp(-elapsed.TotalSeconds / timeConstant.TotalSeconds);
    return (float)(previous + alpha * (sample - previous));
  }



  /// <summary>
  ///   Parses the aggregate <c>cpu </c> line of <c>/proc/stat</c> (user nice system idle iowait irq softirq steal ...): the sum of the first
  ///   eight fields is the total, idle plus iowait is idle (a CPU waiting on a disk is not busy). Guest time is already in user time and
  ///   is not added. Null for anything but that line, or a field that is not a number.
  /// </summary>
  public static SystemCpuTimes? ParseProcStat(string? line) {
    if (line is null || !line.StartsWith("cpu ", StringComparison.Ordinal)) {
      return null;
    }

    var fields = line.Split(_space, StringSplitOptions.RemoveEmptyEntries);
    if (fields.Length < 5) {
      return null;
    }

    ulong total = 0;
    ulong idle = 0;
    for (var i = 1; i < fields.Length && i <= 8; i++) {
      if (!ulong.TryParse(fields[i], NumberStyles.None, CultureInfo.InvariantCulture, out var value)) {
        return null;
      }

      total += value;
      if (i is 4 or 5) {
        idle += value;
      }
    }

    return new(idle, total);
  }



  private static Func<SystemCpuTimes?>? ReadCurrentCpuTimes =>
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ReadWindows :
    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ReadLinux : null;



  private static SystemCpuTimes? ReadLinux() {
    try {
      using var reader = new StreamReader("/proc/stat");
      return ParseProcStat(reader.ReadLine());
    } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
      return null;
    }
  }



  private static SystemCpuTimes? ReadWindows() {
    // Kernel time includes the idle time.
    if (!NativeSystem.GetSystemTimes(out var idle, out var kernel, out var user)) {
      return null;
    }

    return new(idle, kernel + user);
  }
}
