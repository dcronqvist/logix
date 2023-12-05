using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace LogiX.Debug.Logging;

public class Log : ILog
{
    private Task _loggingTask;
    private CancellationTokenSource _cancellationTokenSource;
    private LogLevel _logLevel;
    private readonly ILogger _logger;
    private readonly BufferBlock<string> _logQueue = new();

    public Log(ILogger logger)
    {
        _logger = logger;
    }

    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        _loggingTask = new Task(async () =>
        {
            while (true)
            {
                try
                {
                    var message = await _logQueue.ReceiveAsync(_cancellationTokenSource.Token);
                    _logger.Log(message);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        });

        _loggingTask.Start();
    }

    public void Stop()
    {
        if (_loggingTask is null)
            throw new InvalidOperationException("Log has not been started.");

        _cancellationTokenSource.Cancel();
        _loggingTask.Wait();
        _loggingTask = null;
    }

    public void LogMessage(LogLevel level, string message)
    {
        if (_loggingTask is null)
            throw new InvalidOperationException("Log has not been started.");

        if (level < _logLevel)
            return;

        string logMessage = $"[{DateTime.Now} - {level.ToString().ToUpper(),-7}] {message}";
        _logQueue.Post(logMessage);
    }

    public void SetLogLevel(LogLevel level)
    {
        _logLevel = level;
    }
}
