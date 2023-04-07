using System.Text.Json;
using LogiX.Content;
using Symphony;

namespace LogiX.Minimal;

public class MinimalContentLoader : IContentLoader<ContentMeta>
{
    private Dictionary<string, IContentItemLoader> _loaders = new Dictionary<string, IContentItemLoader>();

    public MinimalContentLoader()
    {
        _loaders.Add(".png", new TextureLoader());
        _loaders.Add(".dll", new AssemblyLoader());
        _loaders.Add(".fontzip", new FontLoader());
        _loaders.Add(".md", new MarkdownFileLoader());
    }

    public string GetIdentifierForSource(IContentSource source)
    {
        using var structure = source.GetStructure();

        using (var stream = structure.GetEntryStream("meta.json", out var entry))
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var metadata = JsonSerializer.Deserialize<ContentMeta>(stream, options);

            return metadata.Name;
        }
    }

    public IEnumerable<IContentLoadingStage> GetLoadingStages()
    {
        yield return new CoreLoadingStage(_loaders, false, ".fontzip");
        yield return new NormalLoadingStage(_loaders, false, ".dll", ".fontzip", ".md");
    }

    public IEnumerable<IContentSource> GetSourceLoadOrder(IEnumerable<IContentSource> sources)
    {
        return sources;
    }
}

public class MinimalTestsContentLoader : IContentLoader<ContentMeta>
{
    private Dictionary<string, IContentItemLoader> _loaders = new Dictionary<string, IContentItemLoader>();

    public MinimalTestsContentLoader()
    {
        _loaders.Add(".fontzip", new FontLoader());
    }

    public string GetIdentifierForSource(IContentSource source)
    {
        using var structure = source.GetStructure();

        using (var stream = structure.GetEntryStream("meta.json", out var entry))
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var metadata = JsonSerializer.Deserialize<ContentMeta>(stream, options);

            return metadata.Name;
        }
    }

    public IEnumerable<IContentLoadingStage> GetLoadingStages()
    {
        yield return new CoreLoadingStage(_loaders, false, ".fontzip");
        yield return new NormalLoadingStage(_loaders, false, ".fontzip");
    }

    public IEnumerable<IContentSource> GetSourceLoadOrder(IEnumerable<IContentSource> sources)
    {
        return sources;
    }
}