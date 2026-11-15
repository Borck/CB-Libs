namespace CB.System.Diagnostics;

using global::System;
using global::System.Runtime.InteropServices;
using Xunit;



/// <summary>The machine-wide memory reading: the parser of <c>/proc/meminfo</c> and the status maths, without a real machine.</summary>
public class SystemMemoryTests {
  [Fact]
  public void Meminfo_UsesMemAvailable_NotMemFree() {
    const string meminfo = """
      MemTotal:       16000000 kB
      MemFree:          500000 kB
      MemAvailable:    8000000 kB
      Buffers:          100000 kB
      """;

    var status = SystemMemory.ParseMeminfo(meminfo);

    Assert.NotNull(status);
    Assert.Equal(16_000_000 * 1024.0, status.Value.TotalBytes);
    Assert.Equal(0.5f, status.Value.UsedFraction, 3);
  }



  [Fact]
  public void Meminfo_FallsBackToMemFree_WhenTheKernelHasNoMemAvailable() {
    var status = SystemMemory.ParseMeminfo("MemTotal: 1000 kB\nMemFree: 250 kB\n");

    Assert.Equal(0.75f, status!.Value.UsedFraction, 3);
  }



  [Fact]
  public void Meminfo_WithoutATotal_IsNotAReading() =>
    Assert.Null(SystemMemory.ParseMeminfo("MemFree: 250 kB\n"));



  [Fact]
  public void UsedFraction_OfAnEmptyOrOverfullStatus_StaysInRange() {
    Assert.Equal(0f, new SystemMemoryStatus(0, 0).UsedFraction);
    Assert.Equal(0f, new SystemMemoryStatus(100, 200).UsedFraction);
    Assert.Equal(1f, new SystemMemoryStatus(100, -5).UsedFraction);
  }



  [Fact]
  public void ParseMeminfo_WithNull_ExpectArgumentNullException() =>
    Assert.Throws<ArgumentNullException>(() => SystemMemory.ParseMeminfo(null!));



  [Fact]
  public void ParseMeminfo_WithLinesThatAreNotNumbers_ExpectThemSkipped() {
    var status = SystemMemory.ParseMeminfo("MemTotal: 1000 kB\nHugePages_Total: many\nnot a line\nMemAvailable: 400 kB\n");

    Assert.Equal(0.6f, status!.Value.UsedFraction, 3);
  }



  [Fact]
  public void Read_OnThisMachine_ExpectATotalWhereTheOperatingSystemReportsOne() {
    var status = SystemMemory.Read();

    Assert.InRange(status.UsedFraction, 0f, 1f);
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
      Assert.True(status.TotalBytes > 0);
      Assert.True(status.AvailableBytes <= status.TotalBytes);
    }
  }
}
