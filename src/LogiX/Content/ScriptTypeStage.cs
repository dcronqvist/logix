using System.Reflection;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using Symphony;

namespace LogiX.Content;

public class ScriptTypeStage : IContentLoadingStage
{
    public string StageName => "Script Type Loading";
    private ContentLoader _loader;

    public ScriptTypeStage(ContentLoader loader)
    {
        _loader = loader;
    }

    public IEnumerable<ContentEntry> GetAffectedEntries(IEnumerable<ContentEntry> allEntries)
    {
        return allEntries.Where(entry => entry.EntryPath.EndsWith(".dll"));
    }

    public void OnStageCompleted()
    {

    }

    public void OnStageStarted()
    {

    }

    public async IAsyncEnumerable<LoadEntryResult> TryLoadEntry(IContentSource source, IContentStructure structure, ContentEntry entry)
    {
        // Create script types from assemblies
        var ident = _loader.GetIdentifierForSource(source);

        var ass = Utilities.ContentManager.GetContentItem<AssemblyContentItem>($"{ident}:{entry.EntryPath}");
        var types = ass.Content.GetTypes();

        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            var attr = type.GetCustomAttribute<ScriptTypeAttribute>();
            if (attr is not null)
            {
                var identifier = $"script/{attr.Identifier}";
                yield return await LoadEntryResult.CreateSuccessAsync($"script/{attr.Identifier}", new ScriptType(source, type));
            }
        }
    }
}