using System.IO;
using System.Linq;

namespace Symphony.Common;

public class DirectoryContentSource : IContentSource
{
    private string _path;

    public DirectoryContentSource(string path)
    {
        _path = path;
    }

    public string GetIdentifier()
    {
        return _path.Split(Path.DirectorySeparatorChar).Last();
    }

    public IContentStructure GetStructure()
    {
        return new DirectoryContentStructure(_path);
    }
}