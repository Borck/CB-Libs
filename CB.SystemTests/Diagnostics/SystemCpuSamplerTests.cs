namespace CB.System.Diagnostics;

using global::System;
using Xunit;



/// <summary>
///   The pure parts of the machine-wide CPU reading - parsing the kernel's counters, the busy fraction between two readings, the moving
///   average - tested without a clock or a real machine.
/// </summary>
public class SystemCpuSamplerTests {
  [Fact]
  public void ProcStat_SumsEightFields_AndCountsIowaitAsIdle() {
    // user nice system idle iowait irq softirq steal guest guest_nice
    var times = SystemCpuSampler.ParseProcStat("cpu  100 10 50 800 40 0 0 0 99 0");

    Assert.Equal(new SystemCpuTimes(Idle: 840, Total: 1000), times);
  }



  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("cpu0 1 2 3 4 5 6 7 8")]
  [InlineData("cpu  1 2 x 4 5")]
  [InlineData("cpu  1 2")]
  public void ProcStat_RejectsAnythingButTheAggregateLine(string? line) =>
    Assert.Null(SystemCpuSampler.ParseProcStat(line));



  [Fact]
  public void Fraction_IsTheBusyShareBetweenTwoReadings() =>
    Assert.Equal(0.75f, SystemCpuSampler.Fraction(new(1000, 2000), new(1250, 3000)), 3);



  [Fact]
  public void Fraction_OfASaturatedMachine_IsOne() =>
    Assert.Equal(1f, SystemCpuSampler.Fraction(new(500, 1000), new(500, 2000)));



  [Fact]
  public void Fraction_SurvivesCountersThatDidNotAdvanceOrWentBackwards() {
    Assert.Equal(0f, SystemCpuSampler.Fraction(new(10, 100), new(10, 100)));
    Assert.Equal(0f, SystemCpuSampler.Fraction(new(10, 100), new(5, 50)));
  }



  [Fact]
  public void Smooth_MovesTowardsTheSample_ByTheElapsedTimeOverTheConstant() {
    var oneTau = SystemCpuSampler.Smooth(0f, 1f, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));

    Assert.Equal(1 - 1 / Math.E, oneTau, 4); // 63.2% of the way after one time constant
  }



  [Fact]
  public void Smooth_WithZeroTimeConstant_IsTheSampleItself() =>
    Assert.Equal(0.9f, SystemCpuSampler.Smooth(0.1f, 0.9f, TimeSpan.FromSeconds(5), TimeSpan.Zero));



  /// <summary>
  ///   The reported symptom: readings jumping between ~0.006 and ~0.011 on a loaded host. A single spike or dip
  ///   must not move the reading by more than a fraction of its size.
  /// </summary>
  [Fact]
  public void NextValue_DampsASingleSpike_AndFollowsASustainedChange() {
    long now = 0;
    var samples = new[] { 0.10f, 0.10f, 1.00f, 0.10f, 0.10f };
    var index = 0;
    ulong total = 0;
    ulong idle = 0;

    var cpu = new SystemCpuSampler(
      () => {
        total += 1000;
        idle += (ulong)(1000 * (1 - samples[Math.Min(index, samples.Length - 1)]));
        index++;
        return new SystemCpuTimes(idle, total);
      },
      () => now += global::System.Diagnostics.Stopwatch.Frequency * 5, // every call is five seconds later
      TimeSpan.FromSeconds(15)
    );

    _ = cpu.NextValue(); // baseline
    var steady = cpu.NextValue();
    var afterSpike = cpu.NextValue();
    var recovered = cpu.NextValue();

    Assert.Equal(0.10f, steady, 3);
    Assert.InRange(afterSpike, 0.2f, 0.45f); // one spike to 100% moves the average, but nowhere near 1.0
    Assert.True(recovered < afterSpike);
  }



  [Fact]
  public void NextValue_FirstRealSampleSeedsTheAverage_InsteadOfRampingFromZero() {
    long now = 0;
    ulong total = 0;
    ulong idle = 0;

    var cpu = new SystemCpuSampler(
      () => {
        total += 1000;
        idle += 100; // 90% busy, every interval
        return new SystemCpuTimes(idle, total);
      },
      () => now += global::System.Diagnostics.Stopwatch.Frequency * 5,
      TimeSpan.FromSeconds(15)
    );

    _ = cpu.NextValue();

    Assert.Equal(0.9f, cpu.NextValue(), 3);
  }



  [Fact]
  public void NextValue_WithoutCounters_ExpectZeroAndNotSupported() {
    var cpu = new SystemCpuSampler(null, () => 0, TimeSpan.FromSeconds(15));

    Assert.False(cpu.IsSupported);
    Assert.Equal(0f, cpu.NextValue());
  }



  [Fact]
  public void NextValue_WithACounterThatCannotBeRead_ExpectTheLastValueKept() {
    long now = 0;
    var readings = 0;
    var cpu = new SystemCpuSampler(
      () => ++readings switch {
        1 => new SystemCpuTimes(0, 1000),
        2 => new SystemCpuTimes(500, 2000),
        _ => null
      },
      () => now += global::System.Diagnostics.Stopwatch.Frequency * 5,
      TimeSpan.Zero
    );

    _ = cpu.NextValue();
    var measured = cpu.NextValue();

    Assert.Equal(0.5f, measured, 3);
    Assert.Equal(measured, cpu.NextValue());
  }



  [Fact]
  public void NextValue_TwiceWithinTheMinimumInterval_ExpectNoNewSample() {
    long now = 0;
    ulong total = 0;
    var cpu = new SystemCpuSampler(
      () => new SystemCpuTimes(total / 2, total += 1000),
      () => now += global::System.Diagnostics.Stopwatch.Frequency / 10, // 100 ms per call, under the 250 ms minimum
      TimeSpan.Zero
    );

    _ = cpu.NextValue(); // baseline
    Assert.Equal(0f, cpu.NextValue());
    Assert.Equal(0f, cpu.NextValue());
    Assert.True(cpu.NextValue() > 0f); // 300 ms after the baseline
  }



  [Fact]
  public void NextValue_WithTheRealCounters_ExpectAValueInRange() {
    var cpu = new SystemCpuSampler(TimeSpan.Zero);

    var value = cpu.NextValue();

    Assert.InRange(value, 0f, 1f);
  }
}
