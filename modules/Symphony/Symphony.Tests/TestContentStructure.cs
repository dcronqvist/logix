using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Symphony.Tests;

internal class TestContentEntry
{
    public string Name { get; set; }
    public byte[] Data { get; set; }

    internal TestContentEntry(string name, byte[] data)
    {
        Name = name;
        Data = data;
    }
}

internal class TestContentStructure : IContentStructure
{
    private TestContentEntry[] _entries;
    private bool disposedValue;

    public TestContentStructure(params TestContentEntry[] entries)
    {
        _entries = entries;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                // Nothing to do.
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TestContentStructure()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }

    public bool HasEntry(string entryPath)
    {
        return _entries.Any(e => e.Name == entryPath);
    }

    public bool TryGetEntry(string entryPath, [NotNullWhen(true)] out ContentEntry? entry)
    {
        if (this.HasEntry(entryPath))
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
        if (this.HasEntry(entryPath))
        {
            return new ContentEntry(entryPath);
        }
        else
        {
            throw new KeyNotFoundException($"Entry {entryPath} not found");
        }
    }

    public IEnumerable<ContentEntry> GetEntries(Predicate<ContentEntry>? filter = null)
    {
        return _entries.Select(e => new ContentEntry(e.Name)).Where(e => filter == null || filter(e));
    }

    public bool TryGetEntryStream(string entryPath, [NotNullWhen(true)] out ContentEntry? entry, [NotNullWhen(true)] out Stream? stream)
    {
        if (this.HasEntry(entryPath))
        {
            entry = new ContentEntry(entryPath);
            stream = new MemoryStream(_entries.First(e => e.Name == entryPath).Data);
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
        if (this.HasEntry(entryPath))
        {
            entry = new ContentEntry(entryPath);
            return new MemoryStream(_entries.First(e => e.Name == entryPath).Data);
        }
        else
        {
            throw new KeyNotFoundException($"Entry {entryPath} not found");
        }
    }

    public DateTime GetLastWriteTimeForEntry(string entryPath)
    {
        if (this.HasEntry(entryPath))
        {
            return DateTime.Now;
        }
        else
        {
            throw new KeyNotFoundException($"Entry {entryPath} not found");
        }
    }
}