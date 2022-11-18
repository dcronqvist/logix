using LogiX.Graphics;
using Symphony;

namespace LogiX.Content;

public class NormalLoadingStage : BaseLoadingStage
{
    public override string StageName => "Normal Loading";

    public NormalLoadingStage(Dictionary<string, IContentItemLoader> loaders, bool doGLInit, params string[] extensions) : base(loaders, doGLInit, extensions)
    {
    }

    public override IEnumerable<ContentEntry> GetAffectedEntries(IEnumerable<ContentEntry> allEntries)
    {
        return base.GetAffectedEntries(allEntries).Where(entry => !entry.EntryPath.StartsWith("core"));
    }

    public override void OnStageStarted()
    {

    }

    public override void OnStageCompleted()
    {

    }
}