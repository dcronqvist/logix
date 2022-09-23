using System.Diagnostics.CodeAnalysis;
using GoodGame.Graphics;
using Symphony;

namespace GoodGame.Content;

public class ShaderProgramLoadingStage : BaseLoadingStage
{
    public ShaderProgramLoadingStage(Dictionary<string, IContentItemLoader> loaders, params string[] extensions) : base(loaders, extensions)
    {
    }

    public override string StageName => "Shader Program Creation";

    public override void OnStageCompleted()
    {

    }

    public override void OnStageStarted()
    {

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
}