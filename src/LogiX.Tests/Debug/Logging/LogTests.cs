using LogiX.App.Debug.Logging;
using NSubstitute;

namespace LogiX.App.Tests.Debug.Logging;

public class LogTests
{
    [Fact]
    public void LogMessage_LogNotStarted_ThrowsException()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var log = new Log(logger);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            log.LogMessage(LogLevel.Info, "Test");
        });
    }

    [Fact]
    public void LogMessage_LogStarted_LogsMessage()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var log = new Log(logger);
        log.Start();

        // Act
        log.LogMessage(LogLevel.Info, "Test");

        // Assert
        logger.Received(1).Log(Arg.Is<string>(x => x.Contains("Test")));
    }

    [Fact]
    public void LogMessage_LogStartedWithLogLevelBelowMessageLogLevel_DoesNotLogMessage()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var log = new Log(logger);
        log.SetLogLevel(LogLevel.Warning);
        log.Start();

        // Act
        log.LogMessage(LogLevel.Info, "Test");
        Task.Delay(100).Wait();

        // Assert
        logger.DidNotReceive().Log(Arg.Any<string>());
    }

    [Fact]
    public void LogMessage_LogStartedWithLogLevelAboveMessageLogLevel_LogsMessage()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var log = new Log(logger);
        log.SetLogLevel(LogLevel.Info);
        log.Start();

        // Act
        log.LogMessage(LogLevel.Warning, "Test");
        Task.Delay(100).Wait();

        // Assert
        logger.Received(1).Log(Arg.Is<string>(x => x.Contains("Test")));
    }

    [Fact]
    public void LogMessage_LogStopped_ThrowsException()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var log = new Log(logger);
        log.Start();
        log.Stop();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            log.LogMessage(LogLevel.Info, "Test");
        });
    }

    [Fact]
    public void Stop_LogNotStarted_ThrowsException()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var log = new Log(logger);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            log.Stop();
        });
    }
}
