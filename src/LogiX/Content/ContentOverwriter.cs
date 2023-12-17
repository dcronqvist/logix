using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Symphony;

namespace LogiX.Content;

public class ContentOverwriter : IContentOverwriter
{
    public IEnumerable<(IContentSource, IContentSource)> SelectContentSourcesForEntry(string entryPath, IEnumerable<IContentSource> sources)
    {
        if (entryPath == "meta.json")
        {
            foreach (var source in sources)
            {
                yield return (source, source);
            }
        }

        var sourceList = sources.ToList();
        var firstSource = sourceList.First();
        var currentSource = firstSource;

        for (int i = 1; i < sourceList.Count; i++)
        {
            var nextSource = sourceList[i];
            var nextMeta = GetContentMetaForSource(nextSource);

            var currentMeta = GetContentMetaForSource(currentSource);

            if (nextMeta.Overwrites.Contains($"{currentMeta.Identifier}:{entryPath}"))
            {
                currentSource = nextSource;
            }
            else
            {
                yield return (firstSource, currentSource);
                firstSource = nextSource;
                currentSource = nextSource;
            }
        }

        yield return (firstSource, currentSource);
    }

    private ContentMeta GetContentMetaForSource(IContentSource source)
    {
        using var structure = source.GetStructure();
        var metaEntry = structure.GetEntry("meta.json");
        using var sr = new StreamReader(structure.GetEntryStream(metaEntry.EntryPath));
        var json = sr.ReadToEnd();
        return DeserializeContentMeta(json);
    }

    private ContentMeta DeserializeContentMeta(string json)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        return JsonSerializer.Deserialize<ContentMeta>(json, jsonOptions);
    }
}
