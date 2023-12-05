namespace LogiX.Debug.Logging;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public interface ILog
{
    void Start();
    void Stop();
    void SetLogLevel(LogLevel level);
    void LogMessage(LogLevel level, string message);
}
