using System.Collections.Generic;
using System.IO;
using Symphony;

namespace LogiX.Content.Loaders;

public interface ILoader
{
    bool IsEntryAffectedByStage(string entryPath);
    IAsyncEnumerable<LoadEntryResult> LoadEntry(ContentEntry entry, Stream stream);
}
