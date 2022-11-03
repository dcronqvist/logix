namespace Symphony.Tests;

internal class TestContentSource : IContentSource
{
    public string Name { get; set; }
    public TestContentEntry[] Entries { get; set; }

    public TestContentSource(string name, params TestContentEntry[] entries)
    {
        Name = name;
        Entries = entries;
    }

    public string GetIdentifier()
    {
        return Name;
    }

    public IContentStructure GetStructure()
    {
        return new TestContentStructure(Entries);
    }
}