using System.Collections.Generic;
using System.Linq;

namespace Symphony;

public interface IContentOverwriter
{
    IEnumerable<(IContentSource, IContentSource)> SelectContentSourcesForEntry(string entryPath, IEnumerable<IContentSource> sources);
}
