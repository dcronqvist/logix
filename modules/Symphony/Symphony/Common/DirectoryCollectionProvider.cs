using System;
using System.Collections.Generic;
using System.IO;

namespace Symphony.Common;

public class DirectoryCollectionProvider : IContentCollectionProvider
{
    private string _root;
    private Func<string, IContentSource?> _sourceFactory;

    public DirectoryCollectionProvider(string root, Func<string, IContentSource> fileToSourceFactory)
    {
        _root = root;
        _sourceFactory = fileToSourceFactory;
    }

    public IEnumerable<IContentSource> GetModSources()
    {
        var directories = Directory.EnumerateDirectories(_root);
        var files = Directory.EnumerateFiles(_root);

        foreach (var directory in directories)
        {
            yield return new DirectoryContentSource(directory);
        }

        foreach (var file in files)
        {
            var source = _sourceFactory(file);
            if (source is not null)
            {
                yield return source;
            }
        }
    }
}