using System.Text;
using LogiX.Content;
using NSubstitute;
using Symphony;

namespace LogiX.Tests.Content;

public class ContentOverwriterTests
{
    [Fact]
    public void IsEntryAffectedByOverwrite_EntryIsNotDefinedInFinalSourceOverwriteList_ReturnsFalse()
    {
        // Arrange
        var introductionSourceMeta = """
        {
            "identifier": "test-introduction",
            "overwrites": []
        }
        """;
        var finalSourceMeta = """
        {
            "identifier": "test-final",
            "overwrites": []
        }
        """;

        var entryPath = "test-entry";

        var introductionStructure = Substitute.For<IContentStructure>();
        introductionStructure.GetEntry("meta.json").Returns(new ContentEntry("meta.json"));
        introductionStructure.GetEntryStream("meta.json").Returns(new MemoryStream(Encoding.UTF8.GetBytes(introductionSourceMeta)));
        var introductionSource = Substitute.For<IContentSource>();
        introductionSource.GetStructure().Returns(introductionStructure);

        var finalStructure = Substitute.For<IContentStructure>();
        finalStructure.GetEntry("meta.json").Returns(new ContentEntry("meta.json"));
        finalStructure.GetEntryStream("meta.json").Returns(new MemoryStream(Encoding.UTF8.GetBytes(finalSourceMeta)));
        var finalSource = Substitute.For<IContentSource>();
        finalSource.GetStructure().Returns(finalStructure);

        var contentOverwriter = new ContentOverwriter();

        // Act
        var result = contentOverwriter.IsEntryAffectedByOverwrite(entryPath, introductionSource, finalSource);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEntryAffectedByOverwrite_EntryIsDefinedInFinalSourceOverwriteList_ReturnsTrue()
    {
        // Arrange
        var introductionSourceMeta = """
        {
            "identifier": "test-introduction",
            "overwrites": []
        }
        """;
        var finalSourceMeta = """
        {
            "identifier": "test-final",
            "overwrites": [
                "test-introduction:test-entry"
            ]
        }
        """;

        var entryPath = "test-entry";

        var introductionStructure = Substitute.For<IContentStructure>();
        introductionStructure.GetEntry("meta.json").Returns(new ContentEntry("meta.json"));
        introductionStructure.GetEntryStream("meta.json").Returns(new MemoryStream(Encoding.UTF8.GetBytes(introductionSourceMeta)));
        var introductionSource = Substitute.For<IContentSource>();
        introductionSource.GetStructure().Returns(introductionStructure);

        var finalStructure = Substitute.For<IContentStructure>();
        finalStructure.GetEntry("meta.json").Returns(new ContentEntry("meta.json"));
        finalStructure.GetEntryStream("meta.json").Returns(new MemoryStream(Encoding.UTF8.GetBytes(finalSourceMeta)));
        var finalSource = Substitute.For<IContentSource>();
        finalSource.GetStructure().Returns(finalStructure);

        var contentOverwriter = new ContentOverwriter();

        // Act
        var result = contentOverwriter.IsEntryAffectedByOverwrite(entryPath, introductionSource, finalSource);

        // Assert
        Assert.True(result);
    }
}
