using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace Symphony.Common;

public class EmbeddedStructure : IContentStructure
{
    private Assembly _assembly;
    private string _pathToEmbeddedZip;

    private bool disposedValue;

    public EmbeddedStructure(Assembly assembly, string pathToEmbeddedZip)
    {
        _assembly = assembly;
        _pathToEmbeddedZip = pathToEmbeddedZip;
    }

    private ZipArchive GetEmbeddedZip()
    {
        var assembly = _assembly;
        Stream embeddedResourceStream = assembly.GetManifestResourceStream(_pathToEmbeddedZip)!;
        return new ZipArchive(embeddedResourceStream);
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
        var zipArchive = GetEmbeddedZip();

        var forwardSlashed = entryPath.Replace('\\', '/');
        var backSlashed = entryPath.Replace('/', '\\');

        return zipArchive.Entries.Any(x => x.FullName == forwardSlashed || x.FullName == backSlashed);
    }

    public ContentEntry GetEntry(string entryPath)
    {
        return new ContentEntry(entryPath);
    }

    public IEnumerable<ContentEntry> GetEntries(Predicate<ContentEntry>? filter = null)
    {
        var zipArchive = GetEmbeddedZip();

        var entries = zipArchive.Entries
            .Select(x => new ContentEntry(x.FullName))
            .Where(x => filter == null || filter(x));

        return entries;
    }

    public DateTime GetLastWriteTimeForEntry(string entryPath)
    {
        return DateTime.Now;
    }

    public Stream GetEntryStream(string entryPath)
    {
        var zipArchive = GetEmbeddedZip();

        var forwardSlashed = entryPath.Replace('\\', '/');
        var backSlashed = entryPath.Replace('/', '\\');

        if (zipArchive.GetEntry(forwardSlashed) != null)
        {
            return zipArchive.GetEntry(forwardSlashed)!.Open();
        }

        if (zipArchive.GetEntry(backSlashed) != null)
        {
            return zipArchive.GetEntry(backSlashed)!.Open();
        }

        throw new Exception($"Entry not found: {entryPath}");
    }
}
