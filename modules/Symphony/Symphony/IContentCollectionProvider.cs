using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Symphony;

public interface IContentCollectionProvider
{
    IEnumerable<IContentSource> GetModSources();

    static ListContentCollectionProvider FromListOfSources(params IContentSource[] sources)
    {
        return new ListContentCollectionProvider(sources);
    }

    static ListContentCollectionProvider FromListOfSources(IEnumerable<IContentSource> sources)
    {
        return new ListContentCollectionProvider(sources.ToArray());
    }
}

public sealed class ListContentCollectionProvider : IContentCollectionProvider
{
    private readonly IEnumerable<IContentSource> _modSources;

    internal ListContentCollectionProvider(params IContentSource[] sources)
    {
        _modSources = sources;
    }

    public IEnumerable<IContentSource> GetModSources()
    {
        return _modSources;
    }
}