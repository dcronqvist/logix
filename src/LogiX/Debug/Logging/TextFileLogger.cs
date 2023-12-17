using System.IO;

namespace LogiX.Debug.Logging;

public class TextFileLogger : ILogger
{
    private readonly string _filePath;

    public TextFileLogger(string filePath)
    {
        _filePath = filePath;
    }

    public void Log(string message)
    {
        using var writer = new StreamWriter(_filePath, true);
        writer.WriteLine(message);
    }
}
