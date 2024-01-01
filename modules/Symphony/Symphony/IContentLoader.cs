using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Symphony;

public struct LoadEntryResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public IContent? Content { get; set; }
    public string? ItemIdentifier { get; set; }

    public static LoadEntryResult CreateSuccess(string itemIdentifier, IContent content)
    {
        return new LoadEntryResult
        {
            ItemIdentifier = itemIdentifier,
            Success = true,
            Content = content
        };
    }

    public static LoadEntryResult CreateFailure(string itemIdentifier, string error)
    {
        return new LoadEntryResult
        {
            ItemIdentifier = itemIdentifier,
            Success = false,
            Error = error
        };
    }

    public static async Task<LoadEntryResult> CreateSuccessAsync(string itemIdentifier, IContent content)
    {
        return await Task.FromResult(CreateSuccess(itemIdentifier, content));
    }

    public static async Task<LoadEntryResult> CreateFailureAsync(string itemIdentifier, string error)
    {
        return await Task.FromResult(CreateFailure(itemIdentifier, error));
    }
}

public interface IContentLoadingStage
{
    string StageName { get; }

    bool IsEntryAffectedByStage(string entryPath);
    IAsyncEnumerable<LoadEntryResult> LoadEntry(ContentEntry entry, Stream stream);
}

public interface IContentLoader
{
    IEnumerable<IContentSource> GetSourceLoadOrder(IEnumerable<IContentSource> sources);
    IEnumerable<IContentLoadingStage> GetLoadingStages();
    string GetIdentifierForSource(IContentSource source);
}
