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

    public IContentStructure GetStructure()
    {
        return new DirectoryContentStructure(_path);
    }

    public override string ToString()
    {
        return $"DirSource: {_path}";
    }
}
