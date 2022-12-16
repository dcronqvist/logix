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
        _loaders.Add(".font", new FontLoader());
        _loaders.Add(".md", new MarkdownFileLoader());
    }

    public IEnumerable<IContentLoadingStage> GetLoadingStages()
    {
        yield return new CoreLoadingStage(_loaders, false, ".font");
        yield return new NormalLoadingStage(_loaders, false, ".dll", ".font", ".md");
    }
}

public class MinimalTestsContentLoader : IContentLoader<ContentMeta>
{
    private Dictionary<string, IContentItemLoader> _loaders = new Dictionary<string, IContentItemLoader>();

    public MinimalTestsContentLoader()
    {
        _loaders.Add(".font", new FontLoader());
    }

    public IEnumerable<IContentLoadingStage> GetLoadingStages()
    {
        yield return new CoreLoadingStage(_loaders, false, ".font");
        yield return new NormalLoadingStage(_loaders, false, ".font");
    }
}