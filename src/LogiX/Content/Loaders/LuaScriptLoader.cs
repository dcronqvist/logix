using System.Collections.Generic;
using System.IO;
using LogiX.Scripting;
using Symphony;

namespace LogiX.Content.Loaders;

public class LuaScriptLoader : ILoader
{
    public bool IsEntryAffectedByStage(string entryPath) => entryPath.EndsWith(".lua");

    public async IAsyncEnumerable<LoadEntryResult> LoadEntry(ContentEntry entry, Stream stream)
    {
        using StreamReader reader = new(stream);
        string scriptContent = reader.ReadToEnd();

        yield return await LoadEntryResult.CreateSuccessAsync(entry.EntryPath, new LuaScript(scriptContent));
    }
}
