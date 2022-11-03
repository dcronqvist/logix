using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Symphony.Tests;

public class ContentLoadingTests
{
    class TestValueUpdatesMetadata : ContentMetadata
    {
        public string Name { get; set; }
        public string Author { get; set; }
    }

    class TestValueUpdatesValidator : IContentStructureValidator<TestValueUpdatesMetadata>
    {
        public bool TryValidateStructure(IContentStructure structure, [NotNullWhen(true)] out TestValueUpdatesMetadata? metadata, [NotNullWhen(false)] out string? error)
        {
            if (structure.TryGetEntryStream("metadata.json", out var entry, out var stream))
            {
                try
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var options = new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            IncludeFields = true
                        };
                        metadata = JsonSerializer.Deserialize<TestValueUpdatesMetadata>(reader.ReadToEnd(), options)!;

                        error = null;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    error = $"Failed to deserialize metadata.json: {ex.Message}";
                    metadata = null;
                    return false;
                }
            }
            else
            {
                error = "Missing metadata.json";
                metadata = null;
                return false;
            }
        }
    }

    class TestValueUpdatesContentItem : ContentItem<string>
    {
        public TestValueUpdatesContentItem(string identifier, IContentSource source, string content) : base(identifier, source, content)
        {
        }

        protected override void OnContentUpdated(string newContent)
        {
            // Do nothing
        }
    }

    class SyncStage : IContentLoadingStage
    {
        public string StageName => "SyncStage";

        public bool RunAsync => false;

        public IEnumerable<ContentEntry> GetAffectedEntries(IEnumerable<ContentEntry> allEntries)
        {
            return allEntries;
        }

        public bool TryLoadEntry(IContentSource source, IContentStructure structure, ContentEntry entry, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out ContentItem? item)
        {
            if (structure.TryGetEntryStream(entry.EntryPath, out var e, out var stream))
            {
                try
                {
                    var entryName = Path.GetFileName(e.EntryPath);
                    using (var reader = new StreamReader(stream))
                    {
                        item = new TestValueUpdatesContentItem($"{source.GetIdentifier()}.{entryName}", source, reader.ReadToEnd());
                        error = null;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    error = $"Failed to read: {ex.Message}";
                    item = null;
                    return false;
                }
            }
            else
            {
                error = $"Missing {entry.EntryPath}";
                item = null;
                return false;
            }
        }
    }

    class AsyncStage : IContentLoadingStage
    {
        public string StageName => "AsyncStage";

        public bool RunAsync => true;

        public IEnumerable<ContentEntry> GetAffectedEntries(IEnumerable<ContentEntry> allEntries)
        {
            return allEntries;
        }

        public bool TryLoadEntry(IContentSource source, IContentStructure structure, ContentEntry entry, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out ContentItem? item)
        {
            if (structure.TryGetEntryStream(entry.EntryPath, out var e, out var stream))
            {
                try
                {
                    var entryName = Path.GetFileName(e.EntryPath);
                    using (var reader = new StreamReader(stream))
                    {
                        item = new TestValueUpdatesContentItem($"{source.GetIdentifier()}.{entryName}", source, reader.ReadToEnd());
                        error = null;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    error = $"Failed to read: {ex.Message}";
                    item = null;
                    return false;
                }
            }
            else
            {
                error = $"Missing {entry.EntryPath}";
                item = null;
                return false;
            }
        }
    }

    class TestValueUpdatesLoader : IContentLoader<TestValueUpdatesMetadata>
    {
        public IEnumerable<IContentLoadingStage> GetLoadingStages()
        {
            yield return new SyncStage();
            yield return new AsyncStage();
            yield return new SyncStage();
        }
    }

    [Fact]
    public async Task TestValueUpdates()
    {
        // Setup
        var entryMetadata = new TestContentEntry("metadata.json", Encoding.UTF8.GetBytes("{\"name\": \"Content 1, the BEST content\", \"author\": \"Author McAuthorson\"}"));
        var content1 = new TestContentSource("content1", entryMetadata);

        // Create manager with config
        var collection = IContentCollectionProvider.FromListOfSources(content1);
        var config = new ContentManagerConfiguration<TestValueUpdatesMetadata>(new TestValueUpdatesValidator(), collection, new TestValueUpdatesLoader());
        var manager = new ContentManager<TestValueUpdatesMetadata>(config);

        manager.StartedLoading += (sender, e) =>
        {
            Console.WriteLine($"Started loading!");
        };

        manager.FinishedLoading += (sender, e) =>
        {
            Console.WriteLine($"Finished loading!");
        };

        manager.StartedLoadingStage += (sender, e) =>
        {
            Console.WriteLine($"Started loading stage {e.Stage.StageName}");
        };

        manager.FinishedLoadingStage += (sender, e) =>
        {
            Console.WriteLine($"Finished loading stage {e.Stage.StageName}");
        };

        manager.ContentItemStartedLoading += (sender, e) =>
        {
            Console.WriteLine($"Started loading item {e.ItemPath}");
        };

        // Load content
        Console.WriteLine("Loading content...");
        _ = manager.LoadAsync();
        Console.WriteLine("Content loaded");

        // // Get value of a content item
        // var content1ItemMetadata = manager.GetContentItem<TestValueUpdatesContentItem>("content1.metadata");
        // Assert.NotNull(content1ItemMetadata);
        // var valueBefore = content1ItemMetadata!.Content;

        // // Only change the underlying data inside the content source
        // entryMetadata.Data = Encoding.UTF8.GetBytes("{\"name\": \"Content 1, the BEST content, NOW UPDATED\", \"author\": \"Author McAuthorson\"}");

        // // Reload the content
        // manager.Load();

        // // Get value of the content item after reload
        // var valueAfter = content1ItemMetadata!.Content;

        // // Value should have changed
        // Assert.NotEqual(valueBefore, valueAfter);
    }
}