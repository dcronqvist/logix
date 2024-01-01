using System;

namespace Symphony;

public sealed class ContentEntry
{
    private string _entryPath = "";
    public string EntryPath { get => _entryPath; private set => _entryPath = value.Replace('\\', '/'); }
    public DateTime LastWriteTime { get; set; }

    public ContentEntry(string entryPath)
    {
        EntryPath = entryPath;
    }

    public override string ToString()
    {
        return $"ContentEntry: {EntryPath} @ {LastWriteTime}";
    }
}
