namespace Symphony;

public interface IContentSource
{
    string GetIdentifier();
    IContentStructure GetStructure();
}