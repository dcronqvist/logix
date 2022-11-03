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

    public bool TryGetEntry(string entryPath, [NotNullWhen(true)] out ContentEntry? entry)
    {
        if (HasEntry(entryPath))
        {
            entry = new ContentEntry(entryPath);
            return true;
        }
        else
        {
            entry = null;
            return false;
        }
    }

    public ContentEntry GetEntry(string entryPath)
    {
        return new ContentEntry(entryPath);
    }

    public IEnumerable<ContentEntry> GetEntries(Predicate<ContentEntry>? filter = null)
    {
        var allFiles = Directory.GetFiles(_contentRoot, "*", SearchOption.AllDirectories);

        if (filter is null)
        {
            return allFiles.Select(f => new ContentEntry(Path.GetRelativePath(_contentRoot, f)));
        }
        else
        {
            return allFiles.Select(f => new ContentEntry(Path.GetRelativePath(_contentRoot, f))).Where(x => filter(x));
        }
    }

    public bool TryGetEntryStream(string entryPath, [NotNullWhen(true)] out ContentEntry? entry, [NotNullWhen(true)] out Stream? stream)
    {
        if (HasEntry(entryPath))
        {
            entry = new ContentEntry(entryPath);
            stream = File.OpenRead(Path.Combine(_contentRoot, entryPath));
            return true;
        }
        else
        {
            entry = null;
            stream = null;
            return false;
        }
    }

    public Stream GetEntryStream(string entryPath, out ContentEntry entry)
    {
        entry = new ContentEntry(entryPath);
        return File.OpenRead(Path.Combine(_contentRoot, entryPath));
    }

    public DateTime GetLastWriteTimeForEntry(string entryPath)
    {
        return File.GetLastWriteTime(Path.Combine(_contentRoot, entryPath));
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

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~DirectoryContentStructure()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}