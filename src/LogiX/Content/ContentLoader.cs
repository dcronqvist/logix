using System.Text.Json;
using Symphony;

namespace LogiX.Content;

public class ContentLoader : IContentLoader<ContentMeta>
{
    private Dictionary<string, IContentItemLoader> _loaders = new Dictionary<string, IContentItemLoader>();

    public ContentLoader()
    {
        _loaders.Add(".png", new TextureLoader());
        _loaders.Add(".fs", new ShaderLoader());
        _loaders.Add(".vs", new ShaderLoader());
        _loaders.Add(".shader", new ShaderProgramLoader());
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

            return metadata.Identifier;
        }
    }

    public IEnumerable<IContentLoadingStage> GetLoadingStages()
    {
        yield return new ShaderLoadingStage(_loaders, true, ".fs", ".vs");
        yield return new ShaderProgramLoadingStage(_loaders, true, ".shader");

        yield return new CoreLoadingStage(_loaders, true, ".png", ".fontzip");
        yield return new NormalLoadingStage(_loaders, true, ".png", ".dll", ".fontzip", ".md");

        yield return new ScriptTypeStage(this);
    }

    public IEnumerable<IContentSource> GetSourceLoadOrder(IEnumerable<IContentSource> sources)
    {
        return sources;
    }
}