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
        _loaders.Add(".ttf", new FontLoader());
    }

    public IEnumerable<IContentLoadingStage> GetLoadingStages()
    {
        yield return new ShaderLoadingStage(_loaders, ".fs", ".vs");
        yield return new ShaderProgramLoadingStage(_loaders, ".shader");

        yield return new CoreLoadingStage(_loaders, ".png", ".dll", ".ttf");
        yield return new NormalLoadingStage(_loaders, ".png", ".dll", ".ttf");
    }
}