using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;

namespace Symphony;

public interface IContentStructure : IDisposable
{
    bool HasEntry(string entryPath);
    bool TryGetEntry(string entryPath, [NotNullWhen(true)] out ContentEntry? entry);
    ContentEntry GetEntry(string entryPath);
    IEnumerable<ContentEntry> GetEntries(Predicate<ContentEntry>? filter = null);
    bool TryGetEntryStream(string entryPath, [NotNullWhen(true)] out ContentEntry? entry, [NotNullWhen(true)] out Stream? stream);
    Stream GetEntryStream(string entryPath, out ContentEntry entry);
    DateTime GetLastWriteTimeForEntry(string entryPath);
}