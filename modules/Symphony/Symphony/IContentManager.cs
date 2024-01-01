using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Symphony;

public record LoadedSourceMetadata<TMeta>(TMeta Metadata, IContentSource Source);

public interface IContentManager<TMeta>
{
    event EventHandler? StartedLoading;
    event EventHandler<StageStartedEventArgs>? StageStarted;
    event EventHandler<StageFinishedEventArgs>? StageFinished;
    event EventHandler<InvalidContentStructureEncounteredEventArgs>? InvalidContentStructureEncountered;
    event EventHandler<ContentEntryStartedLoadingEventArgs>? ContentEntryStartedLoading;
    event EventHandler<ContentItemSuccessfullyLoadedEventArgs>? ContentItemSuccessfullyLoaded;
    event EventHandler<ContentItemFailedToLoadEventArgs>? ContentItemFailedToLoad;
    event EventHandler<ContentItemReloadedEventArgs>? ContentItemReloaded;
    event EventHandler? FinishedLoading;

    IReadOnlyCollection<LoadedSourceMetadata<TMeta>> GetLoadedSourcesMetadata();
    Task LoadAsync();
    IContent? GetContent(string identifier);
    ContentItem? GetContentItem(string identifier);
    string GetSourceIdentifierForContent(string identifier);
    T? GetContent<T>(string identifier) where T : IContent;
    IEnumerable<ContentItem> GetContentItems();
    IEnumerable<T> GetContentOfType<T>() where T : IContent;
    IEnumerable<ContentItem> GetContentItemsOfType<T>() where T : IContent;
    Task PollForSourceUpdatesAsync();
    void UnloadAllContent();
}
