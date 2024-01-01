using System.IO;
using System.IO.Compression;

namespace Symphony.Common;

public class ZipFileContentSource : IContentSource
{
    private string _pathToZip;

    public ZipFileContentSource(string pathToZip)
    {
        _pathToZip = pathToZip;
    }

    public IContentStructure GetStructure()
    {
        return new ZipFileContentStructure(_pathToZip);
    }

    public override string ToString()
    {
        return $"ZipSource: {_pathToZip}";
    }
}
