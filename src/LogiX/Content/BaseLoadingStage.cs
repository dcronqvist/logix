using System.Diagnostics.CodeAnalysis;
using Symphony;

namespace LogiX.Content;

public abstract class BaseLoadingStage : IContentLoadingStage
{
    public abstract string StageName { get; }

    protected Dictionary<string, IContentItemLoader> _loaders = new Dictionary<string, IContentItemLoader>();
    private string[] _extensions;

    public BaseLoadingStage(Dictionary<string, IContentItemLoader> loaders, params string[] extensionsToAffect)
    {
        _extensions = extensionsToAffect;
        _loaders = loaders;
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
    public abstract Task<LoadEntryResult> TryLoadEntry(IContentSource source, IContentStructure structure, ContentEntry entry);
}