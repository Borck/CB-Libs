namespace CB.System.Net.Connectivity;

using global::System;
using global::System.Linq;
using global::System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Xunit;



public class ConnectionFailureLogTests {
  private static SocketException HostNotFound() => new((int)SocketError.HostNotFound);
  private static SocketException Refused() => new((int)SocketError.ConnectionRefused);



  [Fact]
  public void TheSameCauseIsWarnedOnceNotOncePerAttempt() {
    var logger = new RecordingLogger();
    var time = new ManualTimeProvider();
    var log = new ConnectionFailureLog(logger, "plc:502", timeProvider: time);

    for (var i = 0; i < 50; i++) {
      log.Failed(HostNotFound(), TimeSpan.FromSeconds(5));
      time.Advance(TimeSpan.FromSeconds(5));
    }

    var warning = Assert.Single(logger.Entries, e => e.Level == LogLevel.Warning);
    Assert.NotNull(warning.Exception);
    Assert.Contains("host not found", warning.Message, StringComparison.Ordinal);
    Assert.Equal(49, logger.Count(LogLevel.Debug));
  }



  [Fact]
  public void AChangeOfCauseIsWarnedAgain() {
    var logger = new RecordingLogger();
    var log = new ConnectionFailureLog(logger, "plc:502", timeProvider: new ManualTimeProvider());

    log.Failed(HostNotFound());
    log.Failed(HostNotFound());
    log.Failed(Refused());

    Assert.Equal(2, logger.Count(LogLevel.Warning));
  }



  [Fact]
  public void ALongOutageIsRemindedWithoutAStackTrace() {
    var logger = new RecordingLogger();
    var time = new ManualTimeProvider();
    var log = new ConnectionFailureLog(logger, "plc:502", timeProvider: time) {
      ReminderInterval = TimeSpan.FromMinutes(5)
    };

    for (var i = 0; i < 12; i++) {
      log.Failed(Refused());
      time.Advance(TimeSpan.FromMinutes(1));
    }

    var warnings = logger.Entries.Where(e => e.Level == LogLevel.Warning).ToArray();
    Assert.Equal(3, warnings.Length); // minute 0, 5, 10
    Assert.All(warnings.Skip(1), w => Assert.Null(w.Exception));
    Assert.Contains("still failing", warnings[1].Message, StringComparison.Ordinal);
  }



  [Fact]
  public void RecoveryIsLoggedWithTheOutageDuration() {
    var logger = new RecordingLogger();
    var time = new ManualTimeProvider();
    var log = new ConnectionFailureLog(logger, "plc:502", timeProvider: time);

    log.Failed(Refused());
    time.Advance(TimeSpan.FromMinutes(7));
    log.Failed(Refused());
    log.Connected();

    var info = Assert.Single(logger.Entries, e => e.Level == LogLevel.Information);
    Assert.Contains("7 min", info.Message, StringComparison.Ordinal);
    Assert.Contains("2 failed attempts", info.Message, StringComparison.Ordinal);
  }



  [Fact]
  public void AConnectWithoutAPriorFailureLogsNothing() {
    var logger = new RecordingLogger();
    var log = new ConnectionFailureLog(logger, "plc:502", timeProvider: new ManualTimeProvider());

    log.Connected();

    Assert.Empty(logger.Entries);
  }



  [Fact]
  public void AFlappingLinkIsOneOutageWithOneRecoveryLine() {
    var logger = new RecordingLogger();
    var time = new ManualTimeProvider();
    var log = new ConnectionFailureLog(logger, "peer:5000", timeProvider: time);

    for (var i = 0; i < 10; i++) {
      log.Failed(new ConnectionClosedException("closed at once"));
      time.Advance(TimeSpan.FromSeconds(2));
      log.Connected();
      time.Advance(TimeSpan.FromSeconds(1));
    }

    Assert.Equal(1, logger.Count(LogLevel.Warning));
    Assert.Equal(1, logger.Count(LogLevel.Information));
    Assert.True(log.InOutage);
  }



  [Fact]
  public void AFailureAfterAStableConnectionStartsANewOutage() {
    var logger = new RecordingLogger();
    var time = new ManualTimeProvider();
    var log = new ConnectionFailureLog(logger, "peer:5000", timeProvider: time);

    log.Failed(Refused());
    log.Connected();
    time.Advance(TimeSpan.FromHours(3));
    log.Failed(Refused());

    // Same cause as last time, but after three healthy hours it is news again.
    Assert.Equal(2, logger.Count(LogLevel.Warning));
    Assert.Equal(1, log.Failures);
  }
}
