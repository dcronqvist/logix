using System.Text;
using LogiX.Content;
using NSubstitute;
using Symphony;

namespace LogiX.Tests.Content;

public class ContentStructureValidatorTests
{
    [Fact]
    public void TryValidateStructure_StructureDoesNotContainMetaJson_ReturnsFalse()
    {
        // Arrange
        var structure = Substitute.For<IContentStructure>();
        structure.HasEntry("meta.json").Returns(false);

        var contentStructureValidator = new ContentStructureValidator();

        // Act
        var result = contentStructureValidator.TryValidateStructure(structure, out _, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryValidateStructure_StructureContainsMetaJson_ReturnsTrue()
    {
        // Arrange
        var structure = Substitute.For<IContentStructure>();
        structure.HasEntry("meta.json").Returns(true);
        structure.GetEntryStream("meta.json").Returns(new MemoryStream(Encoding.UTF8.GetBytes("{}")));

        var contentStructureValidator = new ContentStructureValidator();

        // Act
        var result = contentStructureValidator.TryValidateStructure(structure, out _, out _);

        // Assert
        Assert.True(result);
    }
}
