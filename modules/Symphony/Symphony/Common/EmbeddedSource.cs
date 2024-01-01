using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Symphony.Common;

public class EmbeddedSource : IContentSource
{
    private Assembly _assembly;
    private string _pathToEmbeddedZip;

    public EmbeddedSource(Assembly assembly, string pathToEmbeddedZip)
    {
        _assembly = assembly;
        _pathToEmbeddedZip = pathToEmbeddedZip;
    }

    public IContentStructure GetStructure()
    {
        return new EmbeddedStructure(_assembly, _pathToEmbeddedZip);
    }

    public override string ToString()
    {
        return $"EmbeddedSource: {_assembly.FullName} - {_pathToEmbeddedZip}";
    }
}
