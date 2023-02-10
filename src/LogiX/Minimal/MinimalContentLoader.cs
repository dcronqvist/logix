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

    public IEnumerable<IContentLoadingStage> GetLoadingStages()
    {
        yield return new CoreLoadingStage(_loaders, false, ".fontzip");
        yield return new NormalLoadingStage(_loaders, false, ".dll", ".fontzip", ".md");
    }
}

public class MinimalTestsContentLoader : IContentLoader<ContentMeta>
{
    private Dictionary<string, IContentItemLoader> _loaders = new Dictionary<string, IContentItemLoader>();

    public MinimalTestsContentLoader()
    {
        _loaders.Add(".fontzip", new FontLoader());
    }

    public IEnumerable<IContentLoadingStage> GetLoadingStages()
    {
        yield return new CoreLoadingStage(_loaders, false, ".fontzip");
        yield return new NormalLoadingStage(_loaders, false, ".fontzip");
    }
}