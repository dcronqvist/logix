using System.Collections.Generic;
using System.IO;
using System.Linq;
using LogiX.Content.Loaders;
using Symphony;

namespace LogiX.Content;

public class LoadingStage : IContentLoadingStage
{
    private readonly ILoader[] _loaders;
    public string StageName { get; }

    public LoadingStage(string name, params ILoader[] loaders)
    {
        StageName = name;
        _loaders = loaders;
    }

    public bool IsEntryAffectedByStage(string entryPath)
    {
        return _loaders.Any(loader => loader.IsEntryAffectedByStage(entryPath));
    }

    public async IAsyncEnumerable<LoadEntryResult> LoadEntry(ContentEntry entry, Stream stream)
    {
        foreach (var loader in _loaders)
        {
            if (loader.IsEntryAffectedByStage(entry.EntryPath))
            {
                await foreach (var result in loader.LoadEntry(entry, stream))
                {
                    yield return result;
                }

                break;
            }
        }
    }
}
