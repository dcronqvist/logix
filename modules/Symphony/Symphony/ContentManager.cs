using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Symphony;

internal class ContentCollection
{
    // Every content entry can consist of multiple items, so we need to store these efficiently.

    // From content item identifier to entry.
    private Dictionary<string, ContentEntry> _entries = new Dictionary<string, ContentEntry>();

    // From entry to items
    private Dictionary<ContentEntry, Dictionary<string, ContentItem>> _items = new Dictionary<ContentEntry, Dictionary<string, ContentItem>>();

    public ContentCollection()
    {
    }

    public ContentCollection(IEnumerable<(ContentEntry, ContentItem[])> entries)
    {
        foreach (var (entry, items) in entries)
        {
            foreach (var item in items)
            {
                this.AddItem(entry, item);
            }
        }
    }

    public bool HasItem(string identifier)
    {
        var entry = _entries.GetValueOrDefault(identifier);
        return entry != null;
    }

    public IEnumerable<ContentEntry> GetEntriesWhere(Func<ContentEntry, bool> predicate)
    {
        return _items.Keys.Where(predicate);
    }

    public ContentEntry GetEntryForItem(string identifier)
    {
        var entry = _entries.GetValueOrDefault(identifier);
        if (entry == null)
        {
            throw new KeyNotFoundException($"No entry found for item {identifier}");
        }
        return entry;
    }

    public void AddItem(ContentEntry entry, ContentItem item)
    {
        _entries[item.Identifier!] = entry;

        if (!_items.ContainsKey(entry))
        {
            _items.Add(entry, new());
        }

        _items[entry][item.Identifier!] = item;
    }

    public void RemoveItem(string identifier)
    {
        var entry = _entries.GetValueOrDefault(identifier);
        if (entry == null)
            return;
        _entries.Remove(identifier);
        _items.Remove(entry);
    }

    public ContentItem? GetContentItem(string identifier)
    {
        var entry = _entries.GetValueOrDefault(identifier);
        if (entry == null)
            return null;

        if (_items.TryGetValue(entry, out var itemDict))
        {
            return itemDict[identifier];
        }
        else
        {
            return null;
        }
    }

    public T? GetContentItem<T>(string identifier) where T : IContent
    {
        var entry = _entries.GetValueOrDefault(identifier);
        if (entry == null)
            return default;

        if (_items.TryGetValue(entry, out var itemDict))
        {
            return (T)itemDict[identifier].Content;
        }
        else
        {
            return default;
        }
    }

    public void ReplaceContentItem(string identifier, ContentItem item)
    {
        var entry = _entries.GetValueOrDefault(identifier);
        if (entry == null)
            return;
        _items[entry][identifier] = item;
    }

    public ContentCollection GetCopy()
    {
        var copy = new ContentCollection();
        foreach (var item in _items)
        {
            foreach (var (k, v) in item.Value)
            {
                copy.AddItem(item.Key, v);
            }
        }
        return copy;
    }

    public IEnumerable<ContentItem> GetItems()
    {
        return _items.Values.Select(x => x.Values).SelectMany(x => x);
    }

    public IEnumerable<(ContentEntry, ContentItem[])> GetEntriesAndItems()
    {
        return _items.Select(x => (x.Key, x.Value.Values.ToArray()));
    }
}

public abstract class EventArgsWithProgress : EventArgs
{
    public float Progress { get; }

    internal EventArgsWithProgress(float progress)
    {
        this.Progress = progress;
    }
}

public abstract class EventArgsWithContent : EventArgsWithProgress
{
    private ContentCollection _currentContent;

    internal EventArgsWithContent(ContentCollection currentContent, float progress) : base(progress)
    {
        _currentContent = currentContent;
    }

    public IContent? GetContent(string identifier)
    {
        return _currentContent.GetContentItem(identifier)!.Content;
    }

    public T? GetContent<T>(string identifier) where T : IContent
    {
        return _currentContent.GetContentItem<T>(identifier);
    }

    public IEnumerable<ContentItem> GetContentItems()
    {
        return _currentContent.GetItems();
    }
}

public class StageStartedEventArgs : EventArgsWithContent
{
    public IContentLoadingStage Stage { get; }

    internal StageStartedEventArgs(IContentLoadingStage stage, ContentCollection currentContent, float progress) : base(currentContent, progress)
    {
        this.Stage = stage;
    }
}

public class StageFinishedEventArgs : EventArgsWithContent
{
    public IContentLoadingStage Stage { get; }

    internal StageFinishedEventArgs(IContentLoadingStage stage, ContentCollection currentContent, float progress) : base(currentContent, progress)
    {
        this.Stage = stage;
    }
}

public class InvalidContentStructureEncounteredEventArgs : EventArgsWithProgress
{
    public IContentSource SourceWithInvalidStructure { get; }
    public string ValidationError { get; }

    public InvalidContentStructureEncounteredEventArgs(IContentSource sourceWithInvalidStructure, string validationError, float progress) : base(progress)
    {
        this.SourceWithInvalidStructure = sourceWithInvalidStructure;
        this.ValidationError = validationError;
    }
}

public class ContentEntryStartedLoadingEventArgs : EventArgsWithContent
{
    public ContentEntry Entry { get; }

    internal ContentEntryStartedLoadingEventArgs(ContentEntry entry, ContentCollection currentContent, float progress) : base(currentContent, progress)
    {
        this.Entry = entry;
    }
}

public class ContentItemSuccessfullyLoadedEventArgs : EventArgsWithContent
{
    public ContentItem Item { get; }

    internal ContentItemSuccessfullyLoadedEventArgs(ContentItem item, ContentCollection currentContent, float progress) : base(currentContent, progress)
    {
        this.Item = item;
    }
}

public class ContentItemFailedToLoadEventArgs : EventArgsWithContent
{
    public string ItemIdentifier { get; }
    public string Error { get; }

    internal ContentItemFailedToLoadEventArgs(string itemIdentifier, ContentCollection currentContent, float progress, string error) : base(currentContent, progress)
    {
        this.ItemIdentifier = itemIdentifier;
        this.Error = error;
    }
}

public class ContentItemReloadedEventArgs : EventArgs
{
    public ContentItem Item { get; }

    internal ContentItemReloadedEventArgs(ContentItem item)
    {
        this.Item = item;
    }
}

public class ContentManager<TMeta> : IContentManager<TMeta>
{
    private readonly IContentStructureValidator<TMeta> _structureValidator;
    private readonly IEnumerable<IContentSource> _sources;
    private readonly IContentLoader _loader;
    private readonly IContentOverwriter _overwriter;

    private ContentCollection _loadedContent = new();

    public event EventHandler? StartedLoading;
    public event EventHandler<StageStartedEventArgs>? StageStarted;
    public event EventHandler<StageFinishedEventArgs>? StageFinished;
    public event EventHandler<InvalidContentStructureEncounteredEventArgs>? InvalidContentStructureEncountered;
    public event EventHandler<ContentEntryStartedLoadingEventArgs>? ContentEntryStartedLoading;
    public event EventHandler<ContentItemSuccessfullyLoadedEventArgs>? ContentItemSuccessfullyLoaded;
    public event EventHandler<ContentItemFailedToLoadEventArgs>? ContentItemFailedToLoad;
    public event EventHandler<ContentItemReloadedEventArgs>? ContentItemReloaded;
    public event EventHandler? FinishedLoading;

    public ContentManager(
        IContentStructureValidator<TMeta> structureValidator,
        IEnumerable<IContentSource> sources, IContentLoader loader,
        IContentOverwriter overwriter)
    {
        _structureValidator = structureValidator;
        _sources = sources;
        _loader = loader;
        _overwriter = overwriter;
    }

    private List<(IContentSource, TMeta)> _loadedSourcesMetadata = new();
    public IReadOnlyCollection<LoadedSourceMetadata<TMeta>> GetLoadedSourcesMetadata()
    {
        return _loadedSourcesMetadata.Select(x => new LoadedSourceMetadata<TMeta>(x.Item2, x.Item1)).ToList();
    }

    private IEnumerable<IContentSource> CollectSourcesWithValidStructures()
    {
        foreach (var source in _sources)
        {
            var structure = source.GetStructure();

            if (!_structureValidator.TryValidateStructure(structure, out TMeta? meta, out string? validationError))
            {
                this.InvalidContentStructureEncountered?.Invoke(this, new InvalidContentStructureEncounteredEventArgs(source, validationError!, 0.0f));
                continue;
            }

            yield return source;
        }
    }

    private async Task<ContentCollection> RunStageAsync(
        IEnumerable<(IContentSource introSource, IContentSource source, ContentEntry entry)> allEntries,
        IContentLoadingStage stage,
        ContentCollection previousLoaded,
        float progressAtStartOfStage,
        float perStageProgress)
    {

        var loaded = previousLoaded.GetCopy();
        var allEntriesGroupedBySource = allEntries.GroupBy(x => x.source);

        float perGroupingProgress = perStageProgress / allEntriesGroupedBySource.Count();

        int groupingIndex = 0;
        foreach (var grouping in allEntriesGroupedBySource)
        {
            var groupingLastSource = grouping.Key;

            using var lastSourceStructure = groupingLastSource.GetStructure();

            var affectedEntries = grouping.Where(x => stage.IsEntryAffectedByStage(x.entry.EntryPath)).ToList();

            var perAffectedEntryProgress = perGroupingProgress / affectedEntries.Count();
            int affectedEntryIndex = 0;

            foreach (var affectedEntry in affectedEntries)
            {
                ContentEntryStartedLoading?.Invoke(this, new ContentEntryStartedLoadingEventArgs(affectedEntry.entry, loaded.GetCopy(), progressAtStartOfStage + perGroupingProgress * groupingIndex + perAffectedEntryProgress * affectedEntryIndex));

                IContentSource firstSource = affectedEntry.introSource;
                IContentSource lastSource = affectedEntry.source;

                string firstSourceIdentifier = _loader.GetIdentifierForSource(firstSource);
                string lastSourceIdentifier = _loader.GetIdentifierForSource(lastSource);

                ContentEntry lastSourceEntry = affectedEntry.entry;

                lastSourceEntry.LastWriteTime = lastSourceStructure.GetLastWriteTimeForEntry(lastSourceEntry.EntryPath);

                var entryLoadResults = stage.LoadEntry(lastSourceEntry, lastSourceStructure.GetEntryStream(lastSourceEntry.EntryPath));

                await foreach (var loadResult in entryLoadResults)
                {
                    var itemIdentifier = $"{firstSourceIdentifier}:{loadResult.ItemIdentifier}";
                    if (!loadResult.Success)
                    {
                        ContentItemFailedToLoad?.Invoke(this, new ContentItemFailedToLoadEventArgs(itemIdentifier, loaded.GetCopy(), progressAtStartOfStage + perGroupingProgress * groupingIndex + perAffectedEntryProgress * affectedEntryIndex, loadResult.Error!));
                        continue;
                    }

                    var content = loadResult.Content!;

                    var newContentItem = new ContentItem(itemIdentifier, firstSource, lastSource, content);
                    loaded.AddItem(lastSourceEntry, newContentItem);

                    ContentItemSuccessfullyLoaded?.Invoke(this, new ContentItemSuccessfullyLoadedEventArgs(newContentItem, loaded.GetCopy(), progressAtStartOfStage + perGroupingProgress * groupingIndex + perAffectedEntryProgress * affectedEntryIndex));
                }

                affectedEntryIndex++;
            }

            groupingIndex++;
        }

        return loaded;
    }

    private IEnumerable<ContentEntry> GetAllEntriesFromSource(IContentSource source)
    {
        return source.GetStructure().GetEntries(entry => true);
    }

    private int CollectTotalAmountOfEntriesInValidSources(IEnumerable<IContentSource> sources)
    {
        return sources.SelectMany(source => this.GetAllEntriesFromSource(source)).Count();
    }

    public async Task LoadAsync()
    {
        StartedLoading?.Invoke(this, EventArgs.Empty);

        var validSources = this.CollectSourcesWithValidStructures();
        var validSourcesOrdered = _loader.GetSourceLoadOrder(validSources).ToList();
        _loadedSourcesMetadata = validSourcesOrdered.Select(x => (x, _structureValidator.TryValidateStructure(x.GetStructure(), out var meta, out string? validationError) ? meta! : default!)).ToList();

        var allEntriesWithTheirSource = validSourcesOrdered
            .SelectMany(source => this.GetAllEntriesFromSource(source)
                                  .Select(entry => (source, entry))).ToList();

        var entriesGroupedByEntryPath = allEntriesWithTheirSource.GroupBy(pair => pair.entry.EntryPath).ToList();

        var entryPathToSources = entriesGroupedByEntryPath
            .ToDictionary(grouping => grouping.Key, x => x.OrderBy(y => validSourcesOrdered.IndexOf(y.source)).Select(y => y.source).ToList());

        var entriesToLoad = new List<(IContentSource introSource, IContentSource source, ContentEntry entry)>();

        foreach (var grouping in entriesGroupedByEntryPath)
        {
            string entryPath = grouping.Key;
            var firstSource = grouping.First().source;
            var selectedSources = _overwriter.SelectContentSourcesForEntry(entryPath, entryPathToSources[entryPath]);

            foreach (var (introSource, source) in selectedSources)
            {
                entriesToLoad.Add((introSource, source, new ContentEntry(entryPath)));
            }
        }

        var stagesToRun = _loader.GetLoadingStages();

        var previouslyLoadedContent = _loadedContent.GetCopy();

        var newLoadedContent = new ContentCollection();

        float perStageProgress = 1.0f / stagesToRun.Count();
        int stageIndex = 0;

        foreach (var stage in stagesToRun)
        {
            StageStarted?.Invoke(this, new StageStartedEventArgs(stage, newLoadedContent.GetCopy(), stageIndex * perStageProgress));
            newLoadedContent = await this.RunStageAsync(entriesToLoad, stage, newLoadedContent, stageIndex * perStageProgress, perStageProgress);
            stageIndex++;
            StageFinished?.Invoke(this, new StageFinishedEventArgs(stage, newLoadedContent.GetCopy(), stageIndex * perStageProgress));

            _loadedContent = newLoadedContent;
        }

        foreach (var item in _loadedContent.GetItems())
        {
            if (previouslyLoadedContent.HasItem(item.Identifier!))
            {
                _loadedContent.ReplaceContentItem(item.Identifier!, previouslyLoadedContent.GetContentItem(item.Identifier!)!);
                _loadedContent.GetContentItem(item.Identifier!)!.UpdateContent(item.Content);
            }
        }

        FinishedLoading?.Invoke(this, EventArgs.Empty);
    }

    public IContent? GetContent(string identifier)
    {
        return _loadedContent.GetContentItem(identifier)!.Content;
    }

    public T? GetContent<T>(string identifier) where T : IContent
    {
        return _loadedContent.GetContentItem<T>(identifier);
    }

    public IEnumerable<ContentItem> GetContentItems()
    {
        return _loadedContent.GetItems();
    }

    public IEnumerable<T> GetContentOfType<T>() where T : IContent
    {
        return _loadedContent.GetItems().Select(x => x.Content).OfType<T>();
    }

    public IEnumerable<ContentItem> GetContentItemsOfType<T>() where T : IContent
    {
        return _loadedContent.GetItems().Where(x => x.Content is T);
    }

    public async Task PollForSourceUpdatesAsync()
    {
        var itemsToReload = new List<ContentItem>();

        foreach (var item in _loadedContent.GetItems())
        {
            var entry = _loadedContent.GetEntryForItem(item.Identifier!);

            var recordedLastWriteTime = entry.LastWriteTime;
            using var structure = item.FinalSource.GetStructure();
            var currentLastWriteTime = structure.GetLastWriteTimeForEntry(entry.EntryPath);

            if (currentLastWriteTime > recordedLastWriteTime)
            {
                itemsToReload.Add(item);
            }
        }

        if (itemsToReload.Count == 0)
        {
            return;
        }

        var stages = _loader.GetLoadingStages();

        var updatedContent = new ContentCollection();
        float perStageProgress = 1.0f / stages.Count();
        int stageIndex = 0;
        foreach (var stage in stages)
        {
            updatedContent = await RunStageAsync(
                itemsToReload.Select(i => (i.SourceFirstLoadedIn, i.FinalSource, _loadedContent.GetEntryForItem(i.Identifier))),
                stage,
                updatedContent,
                stageIndex * perStageProgress,
                perStageProgress
            );
            stageIndex++;
        }

        foreach (var (updatedEntry, updatedItems) in updatedContent.GetEntriesAndItems())
        {
            foreach (var updatedItem in updatedItems)
            {
                var previousItem = _loadedContent.GetContentItem(updatedItem.Identifier)!;
                previousItem.UpdateContent(updatedItem.Content);

                var previousEntry = _loadedContent.GetEntryForItem(updatedItem.Identifier);
                previousEntry.LastWriteTime = updatedEntry.LastWriteTime;

                ContentItemReloaded?.Invoke(this, new ContentItemReloadedEventArgs(previousItem));
            }
        }
    }

    public void UnloadAllContent()
    {
        foreach (var item in _loadedContent.GetItems())
        {
            item.Content.Unload();
        }
    }

    public string GetSourceIdentifierForContent(string identifier) => _loader.GetIdentifierForSource(_loadedContent.GetContentItem(identifier)!.FinalSource);
    public ContentItem? GetContentItem(string identifier) => _loadedContent.GetContentItem(identifier);
}
