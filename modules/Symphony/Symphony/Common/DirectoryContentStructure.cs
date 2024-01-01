using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Symphony.Common;

public class DirectoryContentStructure : IContentStructure
{
    private string _contentRoot;
    private bool disposedValue;

    public DirectoryContentStructure(string contentRoot)
    {
        _contentRoot = contentRoot;
    }

    public bool HasEntry(string entryPath)
    {
        return File.Exists(Path.Combine(_contentRoot, entryPath));
    }

    public ContentEntry GetEntry(string entryPath)
    {
        return new ContentEntry(entryPath);
    }

    public IEnumerable<ContentEntry> GetEntries(Predicate<ContentEntry> filter)
    {
        if (!Directory.Exists(_contentRoot))
        {
            return Enumerable.Empty<ContentEntry>();
        }

        var allFiles = Directory.GetFiles(_contentRoot, "*", SearchOption.AllDirectories);

        return allFiles.Select(f => new ContentEntry(Path.GetRelativePath(_contentRoot, f))).Where(x => filter(x));
    }

    public DateTime GetLastWriteTimeForEntry(string entryPath)
    {
        return File.GetLastWriteTime(Path.Combine(_contentRoot, entryPath));
    }

    public Stream GetEntryStream(string entryPath)
    {
        return File.OpenRead(Path.Combine(_contentRoot, entryPath));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
