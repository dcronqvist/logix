using System;

namespace Symphony;

public sealed class ContentEntry
{
    public string EntryPath { get; }
    public DateTime LastWriteTime { get; private set; }

    public ContentEntry(string entryPath)
    {
        EntryPath = entryPath;
    }

    internal void SetLastWriteTime(DateTime lastWriteTime)
    {
        LastWriteTime = lastWriteTime;
    }
}