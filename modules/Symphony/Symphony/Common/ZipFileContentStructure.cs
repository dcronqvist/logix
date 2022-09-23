using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Symphony.Common;

public class ZipFileContentStructure : IContentStructure
{
    private string _pathToZip;
    private ZipArchive _archive;
    private Stream _streamForArchive;
    private bool disposedValue;

    public ZipFileContentStructure(string pathToZip)
    {
        _pathToZip = pathToZip;
        _streamForArchive = File.OpenRead(_pathToZip);
        _archive = new ZipArchive(_streamForArchive);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                this._streamForArchive.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ZipFileContentStructure()
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

    public bool HasEntry(string entryPath)
    {
        return _archive.Entries.Any(x => x.FullName == entryPath);
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
        return _archive.Entries.Select(x => new ContentEntry(x.FullName)).Where(x => filter == null || filter(x));
    }

    public bool TryGetEntryStream(string entryPath, [NotNullWhen(true)] out ContentEntry? entry, [NotNullWhen(true)] out Stream? stream)
    {
        if (HasEntry(entryPath))
        {
            entry = new ContentEntry(entryPath);
            stream = _archive.GetEntry(entryPath)!.Open();
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
        return _archive.GetEntry(entryPath)!.Open();
    }

    public DateTime GetLastWriteTimeForEntry(string entryPath)
    {
        return _archive.GetEntry(entryPath)!.LastWriteTime.DateTime;
    }
}