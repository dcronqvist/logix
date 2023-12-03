using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LogiX.Content.Loaders;
using LogiX.UserInterfaceContext;
using Symphony;

namespace LogiX.Content;

public class ContentLoader : IContentLoader
{
    private readonly IAsyncGLContextProvider _gLContextProvider;

    public ContentLoader(IAsyncGLContextProvider gLContextProvider)
    {
        _gLContextProvider = gLContextProvider;
    }

    public string GetIdentifierForSource(IContentSource source)
    {
        return GetContentMeta(source).Identifier;
    }

    public IEnumerable<IContentLoadingStage> GetLoadingStages()
    {
        yield return new LoadingStage("Core",
            new ShaderLoader(_gLContextProvider));
        yield return new LoadingStage("Main",
            new TextureLoader(_gLContextProvider),
            new FontLoader(_gLContextProvider),
            new LuaScriptLoader());
    }

    public IEnumerable<IContentSource> GetSourceLoadOrder(IEnumerable<IContentSource> sources)
    {
        // Topologically order sources by dependencies
        var sourceList = sources.ToList();
        var sourceDictionary = sourceList.ToDictionary(source => $"{GetContentMeta(source).Identifier}:{GetContentMeta(source).Version}");
        var sourceDependencies = sourceList.ToDictionary(source => $"{GetContentMeta(source).Identifier}:{GetContentMeta(source).Version}", source => GetContentMeta(source).Dependencies);

        var orderedSources = new List<IContentSource>();
        var visitedSources = new HashSet<string>();

        void visit(IContentSource source)
        {
            string identifier = GetContentMeta(source).Identifier;
            string version = GetContentMeta(source).Version;
            string key = $"{identifier}:{version}";
            if (visitedSources.Contains(key))
            {
                return;
            }

            visitedSources.Add(key);

            foreach (var dependency in sourceDependencies[key])
            {
                if (!sourceDictionary.ContainsKey($"{dependency.Identifier}:{dependency.Version}"))
                {
                    throw new System.Exception($"Missing dependency {dependency.Identifier}:{dependency.Version}");
                }

                visit(sourceDictionary[$"{dependency.Identifier}:{dependency.Version}"]);
            }

            orderedSources.Add(source);
        }

        foreach (var source in sourceList)
        {
            visit(source);
        }

        return orderedSources;
    }

    private ContentMeta GetContentMeta(IContentSource source)
    {
        var structure = source.GetStructure();
        var metadata = structure.GetEntryStream("meta.json");
        var deserialized = DeserializeJson<ContentMeta>(metadata);
        return deserialized;
    }

    private T DeserializeJson<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);
        string jsonText = reader.ReadToEnd();

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return JsonSerializer.Deserialize<T>(jsonText, options);
    }
}
