using System.Collections.Generic;

namespace Symphony.Common;

public class ContentCollectionCombiner : IContentCollectionProvider
{
    private IContentCollectionProvider[] _providers;

    public ContentCollectionCombiner(params IContentCollectionProvider[] providers)
    {
        _providers = providers;
    }

    public IEnumerable<IContentSource> GetModSources()
    {
        foreach (var provider in _providers)
        {
            foreach (var source in provider.GetModSources())
            {
                yield return source;
            }
        }
    }
}