using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Symphony.Tests;


public class TestContentManager
{
    public sealed class TestMetaData { }

    [Fact]
    public async Task Load_NoSuppliedSources_ContentIsEmpty()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        var sources = Array.Empty<IContentSource>();
        var loader = Substitute.For<IContentLoader>();
        var overwriter = Substitute.For<IContentOverwriter>();

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            false
        );

        await manager.LoadAsync();

        Assert.Empty(manager.GetContentItems());
    }

    [Fact]
    public async Task Load_NoSuppliedSources_ValidatorIsNotCalled()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        var sources = Array.Empty<IContentSource>();
        var loader = Substitute.For<IContentLoader>();
        var overwriter = Substitute.For<IContentOverwriter>();

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            false
        );

        await manager.LoadAsync();

        structureValidator.DidNotReceive().TryValidateStructure(Arg.Any<IContentStructure>(), out Arg.Any<TestMetaData?>(), out Arg.Any<string?>());
    }

    [Fact]
    public async Task Load_NoSuppliedSources_LoaderIsAskedForLoadOrderOfEmptySourceIEnumerable()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        var sources = Array.Empty<IContentSource>();
        var loader = Substitute.For<IContentLoader>();
        var overwriter = Substitute.For<IContentOverwriter>();

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            false
        );

        await manager.LoadAsync();

        loader.Received().GetSourceLoadOrder(Arg.Is<IEnumerable<IContentSource>>(x => x.Count() == 0));
    }

    [Fact]
    public async Task Load_NoSuppliedSources_LoaderIsAskedForLoadingStages()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        var sources = Array.Empty<IContentSource>();
        var loader = Substitute.For<IContentLoader>();
        var overwriter = Substitute.For<IContentOverwriter>();

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            false
        );

        await manager.LoadAsync();

        loader.Received().GetLoadingStages();
    }

    [Fact]
    public async Task Load_NoSuppliedSources_OverwriterIsNotCalled()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        var sources = Array.Empty<IContentSource>();
        var loader = Substitute.For<IContentLoader>();
        var overwriter = Substitute.For<IContentOverwriter>();

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            false
        );

        await manager.LoadAsync();

        overwriter.DidNotReceive().IsEntryAffectedByOverwrite(Arg.Any<string>());
    }

    [Fact]
    public async Task Load_SingleSourceWithSingleEntryWithOneItem_ContentContainsOneItem()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        structureValidator.TryValidateStructure(Arg.Any<IContentStructure>(), out Arg.Any<TestMetaData?>(), out Arg.Any<string?>())
                         .ReturnsForAnyArgs(true);

        var structure = Substitute.For<IContentStructure>();
        structure.GetEntries(Arg.Any<Predicate<ContentEntry>>())
                 .ReturnsForAnyArgs(new[]
                 {
                    new ContentEntry("entry_path.file"),
                 });

        structure.GetEntry(Arg.Any<string>())
                 .Returns(arg => new ContentEntry(arg.Arg<string>()));

        structure.HasEntry(Arg.Any<string>())
                 .Returns(arg => arg.Arg<string>() == "entry_path.file");

        structure.GetEntryStream(Arg.Any<string>())
                 .Returns(Substitute.For<Stream>());

        structure.GetLastWriteTimeForEntry(Arg.Any<string>())
                 .Returns(DateTime.Now);

        var source = Substitute.For<IContentSource>();
        source.GetStructure().Returns(structure);

        var sources = new IContentSource[]
        {
            source
        };

        async IAsyncEnumerable<LoadEntryResult> getLoadEntryResults()
        {
            yield return await LoadEntryResult.CreateSuccessAsync("item_identifier", Substitute.For<IContent>());
        }

        var loadingStage = Substitute.For<IContentLoadingStage>();
        loadingStage.StageName.Returns("stage_name");
        loadingStage.IsEntryAffectedByStage(Arg.Any<string>()).Returns(true);
        loadingStage.LoadEntry(Arg.Any<ContentEntry>(), Arg.Any<Stream>())
                    .ReturnsForAnyArgs(getLoadEntryResults());

        var loader = Substitute.For<IContentLoader>();
        loader.GetIdentifierForSource(Arg.Any<IContentSource>()).Returns("source_identifier");
        loader.GetSourceLoadOrder(Arg.Any<IEnumerable<IContentSource>>()).Returns(arg => arg.Arg<IEnumerable<IContentSource>>());
        loader.GetLoadingStages().Returns(new IContentLoadingStage[] { loadingStage });

        var overwriter = Substitute.For<IContentOverwriter>();
        overwriter.IsEntryAffectedByOverwrite(Arg.Any<string>()).Returns(true);

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            false
        );

        await manager.LoadAsync();

        loader.Received().GetSourceLoadOrder(Arg.Is<IEnumerable<IContentSource>>(x => x.Count() == 1));
        loader.Received().GetIdentifierForSource(source);
        loader.Received().GetLoadingStages();

        overwriter.Received().IsEntryAffectedByOverwrite("entry_path.file");

        loadingStage.Received().IsEntryAffectedByStage("entry_path.file");
        loadingStage.Received().LoadEntry(Arg.Is<ContentEntry>(x => x.EntryPath == "entry_path.file"), Arg.Any<Stream>());

        Assert.Single(manager.GetContentItems());
    }

    [Fact]
    public async Task Load_SingleSourceWithSingleEntryWithTwoItems_ContentContainsTwoItems()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        structureValidator.TryValidateStructure(Arg.Any<IContentStructure>(), out Arg.Any<TestMetaData?>(), out Arg.Any<string?>())
                         .ReturnsForAnyArgs(true);

        var structure = Substitute.For<IContentStructure>();
        structure.GetEntries(Arg.Any<Predicate<ContentEntry>>())
                 .ReturnsForAnyArgs(new[]
                 {
                    new ContentEntry("entry_path.file"),
                 });

        structure.GetEntry(Arg.Any<string>())
                 .Returns(arg => new ContentEntry(arg.Arg<string>()));

        structure.HasEntry(Arg.Any<string>())
                 .Returns(arg => arg.Arg<string>() == "entry_path.file");

        structure.GetEntryStream(Arg.Any<string>())
                 .Returns(Substitute.For<Stream>());

        structure.GetLastWriteTimeForEntry(Arg.Any<string>())
                 .Returns(DateTime.Now);

        var source = Substitute.For<IContentSource>();
        source.GetStructure().Returns(structure);

        var sources = new IContentSource[]
        {
            source
        };

        async IAsyncEnumerable<LoadEntryResult> getLoadEntryResults()
        {
            yield return await LoadEntryResult.CreateSuccessAsync("item_identifier_1", Substitute.For<IContent>());
            yield return await LoadEntryResult.CreateSuccessAsync("item_identifier_2", Substitute.For<IContent>());
        }

        var loadingStage = Substitute.For<IContentLoadingStage>();
        loadingStage.StageName.Returns("stage_name");
        loadingStage.IsEntryAffectedByStage(Arg.Any<string>()).Returns(true);
        loadingStage.LoadEntry(Arg.Any<ContentEntry>(), Arg.Any<Stream>())
                    .ReturnsForAnyArgs(getLoadEntryResults());

        var loader = Substitute.For<IContentLoader>();
        loader.GetIdentifierForSource(Arg.Any<IContentSource>()).Returns("source_identifier");
        loader.GetSourceLoadOrder(Arg.Any<IEnumerable<IContentSource>>()).Returns(arg => arg.Arg<IEnumerable<IContentSource>>());
        loader.GetLoadingStages().Returns(new IContentLoadingStage[] { loadingStage });

        var overwriter = Substitute.For<IContentOverwriter>();
        overwriter.IsEntryAffectedByOverwrite(Arg.Any<string>()).Returns(true);

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            false
        );

        await manager.LoadAsync();

        loader.Received().GetSourceLoadOrder(Arg.Is<IEnumerable<IContentSource>>(x => x.Count() == 1));
        loader.Received().GetIdentifierForSource(source);
        loader.Received().GetLoadingStages();

        overwriter.Received().IsEntryAffectedByOverwrite("entry_path.file");

        loadingStage.Received().IsEntryAffectedByStage("entry_path.file");
        loadingStage.Received().LoadEntry(Arg.Is<ContentEntry>(x => x.EntryPath == "entry_path.file"), Arg.Any<Stream>());

        Assert.Equal(2, manager.GetContentItems().Count());
        Assert.NotNull(manager.GetContent("source_identifier:item_identifier_1"));
        Assert.NotNull(manager.GetContent("source_identifier:item_identifier_2"));
    }

    public sealed class StringContent : Content<StringContent>
    {
        private string _content;

        public StringContent(string content)
        {
            _content = content;
        }

        public override void Unload()
        {
            _content = string.Empty;
        }

        protected override void OnContentUpdated(StringContent newContent)
        {
            _content = newContent._content;
        }

        public string GetString() => _content;
    }

    [Fact]
    public async Task Load_TwoSourcesWithSameEntryWithOneItemEachAllowOverwrite_ContentItemShouldHaveFirstSourceIdentifierAndLastSourceContent()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        structureValidator.TryValidateStructure(Arg.Any<IContentStructure>(), out Arg.Any<TestMetaData?>(), out Arg.Any<string?>())
                         .ReturnsForAnyArgs(true);

        var getStreamForString = (string s) =>
        {
            return new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(s))).BaseStream;
        };

        var getSourceWithItemContent = (string content) =>
        {
            var structure = Substitute.For<IContentStructure>();
            structure.GetEntries(Arg.Any<Predicate<ContentEntry>>())
                    .ReturnsForAnyArgs(new[]
                    {
                        new ContentEntry("entry_path.file"),
                    });

            structure.GetEntry(Arg.Any<string>())
                    .Returns(arg => new ContentEntry(arg.Arg<string>()));

            structure.HasEntry(Arg.Any<string>())
                    .Returns(arg => arg.Arg<string>() == "entry_path.file");

            structure.GetEntryStream(Arg.Any<string>())
                    .Returns(getStreamForString(content));

            structure.GetLastWriteTimeForEntry(Arg.Any<string>())
                    .Returns(DateTime.Now);

            var source = Substitute.For<IContentSource>();
            source.GetStructure().Returns(structure);

            return source;
        };

        var source1 = getSourceWithItemContent("content_1");
        var source2 = getSourceWithItemContent("content_2");

        var sources = new IContentSource[]
        {
            source1,
            source2
        };

        async IAsyncEnumerable<LoadEntryResult> getLoadEntryResults(Stream stream)
        {
            string content = await new StreamReader(stream).ReadToEndAsync();
            yield return await LoadEntryResult.CreateSuccessAsync("item_identifier", new StringContent(content));
        }

        var loadingStage = Substitute.For<IContentLoadingStage>();
        loadingStage.StageName.Returns("stage_name");
        loadingStage.IsEntryAffectedByStage(Arg.Any<string>()).Returns(true);
        loadingStage.LoadEntry(Arg.Any<ContentEntry>(), Arg.Any<Stream>())
                    .ReturnsForAnyArgs(args => getLoadEntryResults(args.ArgAt<Stream>(1)));

        var loader = Substitute.For<IContentLoader>();
        loader.GetIdentifierForSource(Arg.Any<IContentSource>()).Returns(arg => arg.Arg<IContentSource>() == source1 ? "source_identifier_1" : "source_identifier_2");
        loader.GetSourceLoadOrder(Arg.Any<IEnumerable<IContentSource>>()).Returns(arg => arg.Arg<IEnumerable<IContentSource>>());
        loader.GetLoadingStages().Returns(new IContentLoadingStage[] { loadingStage });

        var overwriter = Substitute.For<IContentOverwriter>();
        overwriter.IsEntryAffectedByOverwrite(Arg.Any<string>()).Returns(true);

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            false
        );

        await manager.LoadAsync();

        Assert.Single(manager.GetContentItems());

        var contentItem = manager.GetContentItems().First();

        Assert.Equal("source_identifier_1:item_identifier", contentItem.Identifier);

        var contentItemString = (StringContent)contentItem.Content;

        Assert.Equal("content_2", contentItemString.GetString());
    }

    [Fact]
    public async Task Load_TwoSourcesWithSameEntryWithOneItemEachNoOverwrite_TwoContentItemsOneFromEachSource()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        structureValidator.TryValidateStructure(Arg.Any<IContentStructure>(), out Arg.Any<TestMetaData?>(), out Arg.Any<string?>())
                         .ReturnsForAnyArgs(true);

        var getStreamForString = (string s) =>
        {
            return new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(s))).BaseStream;
        };

        var getSourceWithItemContent = (string content) =>
        {
            var structure = Substitute.For<IContentStructure>();
            structure.GetEntries(Arg.Any<Predicate<ContentEntry>>())
                    .ReturnsForAnyArgs(new[]
                    {
                        new ContentEntry("entry_path.file"),
                    });

            structure.GetEntry(Arg.Any<string>())
                    .Returns(arg => new ContentEntry(arg.Arg<string>()));

            structure.HasEntry(Arg.Any<string>())
                    .Returns(arg => arg.Arg<string>() == "entry_path.file");

            structure.GetEntryStream(Arg.Any<string>())
                    .Returns(getStreamForString(content));

            structure.GetLastWriteTimeForEntry(Arg.Any<string>())
                    .Returns(DateTime.Now);

            var source = Substitute.For<IContentSource>();
            source.GetStructure().Returns(structure);

            return source;
        };

        var source1 = getSourceWithItemContent("content_1");
        var source2 = getSourceWithItemContent("content_2");

        var sources = new IContentSource[]
        {
            source1,
            source2
        };

        async IAsyncEnumerable<LoadEntryResult> getLoadEntryResults(Stream stream)
        {
            string content = await new StreamReader(stream).ReadToEndAsync();
            yield return await LoadEntryResult.CreateSuccessAsync("item_identifier", new StringContent(content));
        }

        var loadingStage = Substitute.For<IContentLoadingStage>();
        loadingStage.StageName.Returns("stage_name");
        loadingStage.IsEntryAffectedByStage(Arg.Any<string>()).Returns(true);
        loadingStage.LoadEntry(Arg.Any<ContentEntry>(), Arg.Any<Stream>())
                    .ReturnsForAnyArgs(args => getLoadEntryResults(args.ArgAt<Stream>(1)));

        var loader = Substitute.For<IContentLoader>();
        loader.GetIdentifierForSource(Arg.Any<IContentSource>()).Returns(arg => arg.Arg<IContentSource>() == source1 ? "source_identifier_1" : "source_identifier_2");
        loader.GetSourceLoadOrder(Arg.Any<IEnumerable<IContentSource>>()).Returns(arg => arg.Arg<IEnumerable<IContentSource>>());
        loader.GetLoadingStages().Returns(new IContentLoadingStage[] { loadingStage });

        var overwriter = Substitute.For<IContentOverwriter>();
        overwriter.IsEntryAffectedByOverwrite(Arg.Any<string>()).Returns(false);

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            false
        );

        await manager.LoadAsync();

        Assert.Equal(2, manager.GetContentItems().Count());

        var contentItemFirstSource = manager.GetContentItems().First();
        Assert.Equal("source_identifier_1:item_identifier", contentItemFirstSource.Identifier);
        Assert.Equal("content_1", ((StringContent)contentItemFirstSource.Content).GetString());

        var contentItemSecondSource = manager.GetContentItems().Last();
        Assert.Equal("source_identifier_2:item_identifier", contentItemSecondSource.Identifier);
        Assert.Equal("content_2", ((StringContent)contentItemSecondSource.Content).GetString());
    }

    [Fact]
    public async Task HotReload_SingleSourceWithSingleEntryWithOneItemThatIsUpdatedOnce_ContentContainsOneItemAndShouldBeUpdatedAfterPoll()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        structureValidator.TryValidateStructure(Arg.Any<IContentStructure>(), out Arg.Any<TestMetaData?>(), out Arg.Any<string?>())
                         .ReturnsForAnyArgs(true);

        var now = DateTime.Now;

        string content = "content_before_reload";

        var getStreamForString = () =>
        {
            return new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content))).BaseStream;
        };

        var getSourceWithItemContent = () =>
        {
            var structure = Substitute.For<IContentStructure>();
            structure.GetEntries(Arg.Any<Predicate<ContentEntry>>())
                    .ReturnsForAnyArgs(new[]
                    {
                        new ContentEntry("entry_path.file"),
                    });

            structure.GetEntry(Arg.Any<string>())
                    .Returns(arg => new ContentEntry(arg.Arg<string>()));

            structure.HasEntry(Arg.Any<string>())
                    .Returns(arg => arg.Arg<string>() == "entry_path.file");

            structure.GetEntryStream(Arg.Any<string>())
                    .Returns(arg => getStreamForString());

            structure.GetLastWriteTimeForEntry(Arg.Any<string>())
                    .Returns(arg => now);

            var source = Substitute.For<IContentSource>();
            source.GetStructure().Returns(structure);

            return source;
        };

        var sources = new IContentSource[]
        {
            getSourceWithItemContent()
        };

        async IAsyncEnumerable<LoadEntryResult> getLoadEntryResults(Stream stream)
        {
            string content = await new StreamReader(stream).ReadToEndAsync();
            yield return await LoadEntryResult.CreateSuccessAsync("item_identifier", new StringContent(content));
        }

        var loadingStage = Substitute.For<IContentLoadingStage>();
        loadingStage.StageName.Returns("stage_name");
        loadingStage.IsEntryAffectedByStage(Arg.Any<string>()).Returns(true);
        loadingStage.LoadEntry(Arg.Any<ContentEntry>(), Arg.Any<Stream>())
                        .ReturnsForAnyArgs(arg => getLoadEntryResults(arg.ArgAt<Stream>(1)));

        var loader = Substitute.For<IContentLoader>();
        loader.GetIdentifierForSource(Arg.Any<IContentSource>()).Returns("source_identifier");
        loader.GetSourceLoadOrder(Arg.Any<IEnumerable<IContentSource>>()).Returns(arg => arg.Arg<IEnumerable<IContentSource>>());
        loader.GetLoadingStages().Returns(new IContentLoadingStage[] { loadingStage });

        var overwriter = Substitute.For<IContentOverwriter>();
        overwriter.IsEntryAffectedByOverwrite(Arg.Any<string>()).Returns(true);

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            true
        );

        await manager.LoadAsync();

        Assert.Single(manager.GetContentItems());

        var itemBeforeReload = manager.GetContentItems().First();
        var contentBeforeReload = ((StringContent)itemBeforeReload.Content).GetString();

        Assert.Equal("content_before_reload", contentBeforeReload);

        content = "content_after_reload";
        now = now.AddSeconds(1);

        await manager.PollForSourceUpdatesAsync();

        Assert.Single(manager.GetContentItems());

        var itemAfterReload = manager.GetContentItems().First();
        var contentAfterReload = ((StringContent)itemAfterReload.Content).GetString();

        Assert.Equal("content_after_reload", contentAfterReload);
    }

    [Fact]
    public async Task HotReload_SingleSourceWithSingleEntryWithOneItemThatIsNotUpdated_ContentContainsOneItemAndShouldNotBeUpdatedAfterPoll()
    {
        var structureValidator = Substitute.For<IContentStructureValidator<TestMetaData>>();
        structureValidator.TryValidateStructure(Arg.Any<IContentStructure>(), out Arg.Any<TestMetaData?>(), out Arg.Any<string?>())
                         .ReturnsForAnyArgs(true);

        var now = DateTime.Now;

        string content = "content_before_reload";

        var getStreamForString = () =>
        {
            return new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content))).BaseStream;
        };

        var getSourceWithItemContent = () =>
        {
            var structure = Substitute.For<IContentStructure>();
            structure.GetEntries(Arg.Any<Predicate<ContentEntry>>())
                    .ReturnsForAnyArgs(new[]
                    {
                        new ContentEntry("entry_path.file"),
                    });

            structure.GetEntry(Arg.Any<string>())
                    .Returns(arg => new ContentEntry(arg.Arg<string>()));

            structure.HasEntry(Arg.Any<string>())
                    .Returns(arg => arg.Arg<string>() == "entry_path.file");

            structure.GetEntryStream(Arg.Any<string>())
                    .Returns(arg => getStreamForString());

            structure.GetLastWriteTimeForEntry(Arg.Any<string>())
                    .Returns(arg => now);

            var source = Substitute.For<IContentSource>();
            source.GetStructure().Returns(structure);

            return source;
        };

        var sources = new IContentSource[]
        {
            getSourceWithItemContent()
        };

        async IAsyncEnumerable<LoadEntryResult> getLoadEntryResults(Stream stream)
        {
            string content = await new StreamReader(stream).ReadToEndAsync();
            yield return await LoadEntryResult.CreateSuccessAsync("item_identifier", new StringContent(content));
        }

        var loadingStage = Substitute.For<IContentLoadingStage>();
        loadingStage.StageName.Returns("stage_name");
        loadingStage.IsEntryAffectedByStage(Arg.Any<string>()).Returns(true);
        loadingStage.LoadEntry(Arg.Any<ContentEntry>(), Arg.Any<Stream>())
                        .ReturnsForAnyArgs(arg => getLoadEntryResults(arg.ArgAt<Stream>(1)));

        var loader = Substitute.For<IContentLoader>();
        loader.GetIdentifierForSource(Arg.Any<IContentSource>()).Returns("source_identifier");
        loader.GetSourceLoadOrder(Arg.Any<IEnumerable<IContentSource>>()).Returns(arg => arg.Arg<IEnumerable<IContentSource>>());
        loader.GetLoadingStages().Returns(new IContentLoadingStage[] { loadingStage });

        var overwriter = Substitute.For<IContentOverwriter>();
        overwriter.IsEntryAffectedByOverwrite(Arg.Any<string>()).Returns(true);

        var manager = new ContentManager<TestMetaData>(
            structureValidator,
            sources,
            loader,
            overwriter,
            true
        );

        await manager.LoadAsync();

        Assert.Single(manager.GetContentItems());

        var itemBeforeReload = manager.GetContentItems().First();
        var contentBeforeReload = ((StringContent)itemBeforeReload.Content).GetString();

        Assert.Equal("content_before_reload", contentBeforeReload);

        content = "content_after_reload";

        await manager.PollForSourceUpdatesAsync();

        Assert.Single(manager.GetContentItems());

        var itemAfterReload = manager.GetContentItems().First();
        var contentAfterReload = ((StringContent)itemAfterReload.Content).GetString();

        Assert.Equal("content_before_reload", contentAfterReload);
    }
}