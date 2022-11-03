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

    public string GetIdentifier()
    {
        return Path.GetFileNameWithoutExtension(_pathToZip)!;
    }

    public IContentStructure GetStructure()
    {
        return new ZipFileContentStructure(this._pathToZip);
    }
}