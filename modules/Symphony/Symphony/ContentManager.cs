using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Symphony;

public class ContentStructureErrorEventArgs : EventArgs
{
    public string Error { get; }
    public IContentSource Source { get; }

    public ContentStructureErrorEventArgs(string error, IContentSource source)
    {
        Error = error;
        Source = source;
    }
}

public class ContentFailedToLoadErrorEventArgs : EventArgs
{
    public string Error { get; }
    public IContentSource Source { get; }

    public ContentFailedToLoadErrorEventArgs(string error, IContentSource source)
    {
        Error = error;
        Source = source;
    }
}

public class LoadingStageEventArgs : EventArgs
{
    public IContentLoadingStage Stage { get; }
    public ContentCollection CurrentlyLoaded { get; }

    public LoadingStageEventArgs(IContentLoadingStage stage, ContentCollection currentlyLoaded)
    {
        Stage = stage;
        CurrentlyLoaded = currentlyLoaded;
    }
}

public class ContentItemStartedLoadingEventArgs : EventArgs
{
    public string ItemPath { get; }
    public float CurrentStageProgress { get; }

    public ContentItemStartedLoadingEventArgs(string itemPath, float currentStageProgress)
    {
        ItemPath = itemPath;
        CurrentStageProgress = currentStageProgress;
    }
}

public class ContentItemReloadedEventArgs : EventArgs
{
    public IContentLoadingStage Stage { get; }
    public ContentEntry Entry { get; }
    public ContentItem Item { get; }

    public ContentItemReloadedEventArgs(IContentLoadingStage stage, ContentEntry entry, ContentItem item)
    {
        Stage = stage;
        Entry = entry;
        Item = item;
    }
}

public class ContentCollection
{
    // From content item identifier to entry.
    private Dictionary<string, ContentEntry> _entries = new Dictionary<string, ContentEntry>();

    // From entry to item
    private Dictionary<ContentEntry, ContentItem> _items = new Dictionary<ContentEntry, ContentItem>();

    public bool HasItem(string identifier)
    {
        var entry = this._entries.GetValueOrDefault(identifier);
        return entry != null;
    }

    public ContentEntry GetEntryForItem(string identifier)
    {
        var entry = this._entries.GetValueOrDefault(identifier);
        if (entry == null)
        {
            throw new KeyNotFoundException($"No entry found for item {identifier}");
        }
        return entry;
    }

    public void AddItem(ContentEntry entry, ContentItem item)
    {
        this._entries.Add(item.Identifier, entry);
        this._items.Add(entry, item);
    }

    public void RemoveItem(string identifier)
    {
        var entry = this._entries.GetValueOrDefault(identifier);
        if (entry == null)
            return;
        this._entries.Remove(identifier);
        this._items.Remove(entry);
    }

    public ContentItem? GetContentItem(string identifier)
    {
        var entry = this._entries.GetValueOrDefault(identifier);
        if (entry == null)
            return null;

        if (this._items.TryGetValue(entry, out var item))
        {
            return item;
        }
        else
        {
            return null;
        }
    }

    public T? GetContentItem<T>(string identifier) where T : ContentItem
    {
        var entry = this._entries.GetValueOrDefault(identifier);
        if (entry == null)
            return null;

        if (this._items.TryGetValue(entry, out var item))
        {
            return (T)item;
        }
        else
        {
            return null;
        }
    }

    public void ReplaceContentItem(string identifier, ContentItem item)
    {
        var entry = this._entries.GetValueOrDefault(identifier);
        if (entry == null)
            return;
        this._items[entry] = item;
    }

    public ContentCollection GetCopy()
    {
        var copy = new ContentCollection();
        foreach (var item in _items)
        {
            copy.AddItem(item.Key, item.Value);
        }
        return copy;
    }

    public IEnumerable<ContentItem> GetItems()
    {
        return _items.Values;
    }
}

public class ContentManager<TMeta> where TMeta : ContentMetadata
{
    // Manager specific stuff
    private readonly ContentManagerConfiguration<TMeta> _configuration;
    private Dictionary<IContentSource, TMeta> _validMods;
    private ContentCollection _loadedContent;

    // Events
    public event EventHandler? StartedLoading;
    public event EventHandler<LoadingStageEventArgs>? StartedLoadingStage;
    public event EventHandler<LoadingStageEventArgs>? FinishedLoadingStage;
    public event EventHandler<ContentStructureErrorEventArgs>? InvalidContentStructureError;
    public event EventHandler<ContentFailedToLoadErrorEventArgs>? ContentFailedToLoadError;
    public event EventHandler<ContentItemStartedLoadingEventArgs>? ContentItemStartedLoading;
    public event EventHandler<ContentItemReloadedEventArgs>? ContentItemReloaded;
    public event EventHandler? FinishedLoading;

    public ContentManager(ContentManagerConfiguration<TMeta> configuration)
    {
        this._configuration = configuration;
        this._validMods = new Dictionary<IContentSource, TMeta>();
        this._loadedContent = new ContentCollection();
    }

    private IEnumerable<IContentSource> CollectValidMods()
    {
        this._validMods.Clear();

        var modSources = this._configuration.CollectionProvider.GetModSources();

        foreach (var source in modSources)
        {
            using (var structure = source.GetStructure())
            {
                if (this._configuration.StructureValidator.TryValidateStructure(structure, out var metadata, out string? error))
                {
                    // Mod is valid and can be loaded
                    this._validMods.Add(source, metadata);
                    yield return source;
                }
                else
                {
                    // Mod is invalid and cannot be loaded
                    this.InvalidContentStructureError?.Invoke(this, new ContentStructureErrorEventArgs(error, source));
                }
            }
        }
    }

    private async Task<ContentCollection> RunStageAsync(IEnumerable<IContentSource> sources, IContentLoadingStage stage, ContentCollection previousLoaded)
    {
        var loaded = previousLoaded;

        this.StartedLoadingStage?.Invoke(this, new LoadingStageEventArgs(stage, loaded));
        stage.OnStageStarted();

        foreach (var source in sources)
        {
            try
            {
                using (var structure = source.GetStructure())
                {
                    var affectedEntries = stage.GetAffectedEntries(structure.GetEntries());
                    var total = affectedEntries.Count();
                    var current = 0;

                    foreach (var entry in affectedEntries)
                    {
                        current += 1;
                        entry.SetLastWriteTime(structure.GetLastWriteTimeForEntry(entry.EntryPath));
                        this.ContentItemStartedLoading?.Invoke(this, new ContentItemStartedLoadingEventArgs(entry.EntryPath, (float)current / total));
                        var loadResult = await Task.Run(() => stage.TryLoadEntry(source, structure, entry));

                        if (loadResult.Success)
                        {
                            var item = loadResult.Item!;
                            item.SetLastModified(entry.LastWriteTime);
                            loaded.AddItem(entry, item);
                        }
                        else
                        {
                            this.ContentFailedToLoadError?.Invoke(this, new ContentFailedToLoadErrorEventArgs(loadResult.Error!, source));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.ContentFailedToLoadError?.Invoke(this, new ContentFailedToLoadErrorEventArgs(ex.Message, source));
            }
        }

        this._loadedContent = loaded.GetCopy();
        stage.OnStageCompleted();
        this.FinishedLoadingStage?.Invoke(this, new LoadingStageEventArgs(stage, loaded));

        return loaded;
    }

    public async Task LoadAsync()
    {
        this.StartedLoading?.Invoke(this, EventArgs.Empty);

        var sources = this.CollectValidMods();
        var stages = this._configuration.Loader.GetLoadingStages();

        var previouslyLoaded = this._loadedContent.GetCopy();

        var currentLoad = new ContentCollection();
        foreach (var stage in stages)
        {
            currentLoad = await this.RunStageAsync(sources, stage, currentLoad);
        }

        foreach (var item in this._loadedContent.GetItems())
        {
            if (previouslyLoaded.HasItem(item.Identifier))
            {
                this._loadedContent.ReplaceContentItem(item.Identifier, previouslyLoaded.GetContentItem(item.Identifier)!);
                this._loadedContent.GetContentItem(item.Identifier)!.UpdateContent(item.Source, item.Content);
            }
        }

        this.FinishedLoading?.Invoke(this, EventArgs.Empty);
    }

    public ContentItem? GetContentItem(string identifier)
    {
        return this._loadedContent.GetContentItem(identifier);
    }

    public T? GetContentItem<T>(string identifier) where T : ContentItem
    {
        return this._loadedContent.GetContentItem<T>(identifier);
    }

    public IEnumerable<ContentItem> GetContentItems()
    {
        return this._loadedContent.GetItems();
    }

    public async Task PollForSourceUpdates()
    {
        var itemsToReload = new List<ContentItem>();

        foreach (var item in this._loadedContent.GetItems())
        {
            var entry = this._loadedContent.GetEntryForItem(item.Identifier);

            var recordedLastWriteTime = entry.LastWriteTime;
            using var structure = item.Source.GetStructure();
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

        var stages = this._configuration.Loader.GetLoadingStages();

        foreach (var stage in stages)
        {
            foreach (var item in itemsToReload)
            {
                var structure = item.Source.GetStructure();
                var entry = this._loadedContent.GetEntryForItem(item.Identifier);

                var isAffectedInStage = stage.GetAffectedEntries(new List<ContentEntry>() { entry }).Count() > 0;

                if (!isAffectedInStage)
                {
                    continue;
                }

                var loadResult = await Task.Run(() => stage.TryLoadEntry(item.Source, structure, entry));
                if (loadResult.Success)
                {
                    var newItem = loadResult.Item!;
                    this._loadedContent.GetContentItem(newItem.Identifier)!.UpdateContent(newItem.Source, newItem.Content);
                    this._loadedContent.GetContentItem(newItem.Identifier)!.SetLastModified(structure.GetLastWriteTimeForEntry(entry.EntryPath));
                    entry.SetLastWriteTime(structure.GetLastWriteTimeForEntry(entry.EntryPath));
                    this.ContentItemReloaded?.Invoke(this, new ContentItemReloadedEventArgs(stage, entry, newItem));
                }
            }
        }
    }

    public void UnloadAllContent()
    {
        foreach (var item in this._loadedContent.GetItems())
        {
            item.Unload();
        }
    }
}