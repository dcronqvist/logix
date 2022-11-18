using System.Diagnostics.CodeAnalysis;
using LogiX.Graphics;
using Symphony;

namespace LogiX.Content;

public abstract class BaseLoadingStage : IContentLoadingStage
{
    public abstract string StageName { get; }

    protected Dictionary<string, IContentItemLoader> _loaders = new Dictionary<string, IContentItemLoader>();
    private string[] _extensions;
    private bool doGLInit = false;

    public BaseLoadingStage(Dictionary<string, IContentItemLoader> loaders, bool performGLInit, params string[] extensionsToAffect)
    {
        _extensions = extensionsToAffect;
        _loaders = loaders;
        doGLInit = performGLInit;
    }

    public virtual IEnumerable<ContentEntry> GetAffectedEntries(IEnumerable<ContentEntry> allEntries)
    {
        if (this._extensions.Length != 0)
        {
            return allEntries.Where(entry => _extensions.Contains(Path.GetExtension(entry.EntryPath)));
        }

        return allEntries.Where(entry => this._loaders.ContainsKey(Path.GetExtension(entry.EntryPath)));
    }

    public abstract void OnStageStarted();
    public abstract void OnStageCompleted();

    public async IAsyncEnumerable<LoadEntryResult> TryLoadEntry(IContentSource source, IContentStructure structure, ContentEntry entry)
    {
        var extension = Path.GetExtension(entry.EntryPath);

        if (!this._loaders.ContainsKey(extension))
        {
            yield return await LoadEntryResult.CreateFailureAsync($"No loader found for {extension}");
        }

        var loader = this._loaders[extension];
        var results = loader.TryLoadAsync(source, structure, entry.EntryPath);

        await foreach (var result in results)
        {
            if (result.Success)
            {
                if (result.Item is GLContentItem glItem && doGLInit)
                {
                    DisplayManager.LockedGLContext(() =>
                    {
                        glItem.InitGL(glItem.Content);
                    });
                }
            }

            yield return result;
        }
    }
}