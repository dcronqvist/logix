using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;

namespace Symphony;

public interface IContentStructure : IDisposable
{
    bool HasEntry(string entryPath);
    ContentEntry GetEntry(string entryPath);
    IEnumerable<ContentEntry> GetEntries(Predicate<ContentEntry> filter);
    Stream GetEntryStream(string entryPath);

    DateTime GetLastWriteTimeForEntry(string entryPath);
}
