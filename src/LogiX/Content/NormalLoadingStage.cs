using GoodGame.Graphics;
using Symphony;

namespace GoodGame.Content;

public class NormalLoadingStage : BaseLoadingStage
{
    public override string StageName => "Normal Loading";

    public NormalLoadingStage(Dictionary<string, IContentItemLoader> loaders, params string[] extensions) : base(loaders, extensions)
    {
    }

    public override IEnumerable<ContentEntry> GetAffectedEntries(IEnumerable<ContentEntry> allEntries)
    {
        return base.GetAffectedEntries(allEntries).Where(entry => !entry.EntryPath.StartsWith("core"));
    }

    public override async Task<LoadEntryResult> TryLoadEntry(IContentSource source, IContentStructure structure, ContentEntry entry)
    {
        var extension = Path.GetExtension(entry.EntryPath);

        if (!this._loaders.ContainsKey(extension))
        {
            return LoadEntryResult.CreateFailure($"No loader found for {extension}");
        }

        var loader = this._loaders[extension];
        var result = await loader.TryLoad(source, structure, entry.EntryPath);

        if (result.Success)
        {
            if (result.Item is GLContentItem glItem)
            {
                DisplayManager.LockedGLContext(() =>
                {
                    glItem.InitGL(glItem.Content);
                });
            }
        }

        return result;
    }

    public override void OnStageStarted()
    {

    }

    public override void OnStageCompleted()
    {

    }
}