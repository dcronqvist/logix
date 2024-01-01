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
                _streamForArchive.Dispose();
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
        var forwardSlashed = entryPath.Replace('\\', '/');
        var backSlashed = entryPath.Replace('/', '\\');

        return _archive.Entries.Any(x => x.FullName == forwardSlashed || x.FullName == backSlashed);
    }

    public ContentEntry GetEntry(string entryPath)
    {
        return new ContentEntry(entryPath);
    }

    public IEnumerable<ContentEntry> GetEntries(Predicate<ContentEntry>? filter = null)
    {
        return _archive.Entries.Select(x => new ContentEntry(x.FullName)).Where(x => filter == null || filter(x));
    }

    public DateTime GetLastWriteTimeForEntry(string entryPath)
    {
        return File.GetLastWriteTime(_pathToZip);
    }

    public Stream GetEntryStream(string entryPath)
    {
        var forwardSlashed = entryPath.Replace('\\', '/');
        var backSlashed = entryPath.Replace('/', '\\');

        if (_archive.GetEntry(forwardSlashed) != null)
        {
            return _archive.GetEntry(forwardSlashed)!.Open();
        }

        if (_archive.GetEntry(backSlashed) != null)
        {
            return _archive.GetEntry(backSlashed)!.Open();
        }

        throw new FileNotFoundException("Could not find entry in zip file", entryPath);
    }
}
