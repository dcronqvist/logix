using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Symphony;

public struct LoadEntryResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public ContentItem? Item { get; set; }

    public static LoadEntryResult CreateSuccess(ContentItem item)
    {
        return new LoadEntryResult
        {
            Success = true,
            Item = item
        };
    }

    public static LoadEntryResult CreateFailure(string error)
    {
        return new LoadEntryResult
        {
            Success = false,
            Error = error
        };
    }
}

public interface IContentLoadingStage
{
    string StageName { get; }
    IEnumerable<ContentEntry> GetAffectedEntries(IEnumerable<ContentEntry> allEntries);
    void OnStageStarted();
    Task<LoadEntryResult> TryLoadEntry(IContentSource source, IContentStructure structure, ContentEntry entry);
    void OnStageCompleted();
}

public interface IContentLoader<TMeta> where TMeta : ContentMetadata
{
    IEnumerable<IContentLoadingStage> GetLoadingStages();
}