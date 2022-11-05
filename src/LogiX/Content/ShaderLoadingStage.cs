using System.Diagnostics.CodeAnalysis;
using LogiX.Graphics;
using Symphony;

namespace LogiX.Content;

public class ShaderLoadingStage : BaseLoadingStage
{
    // Assuming loaders for .vs and .fs
    public ShaderLoadingStage(Dictionary<string, IContentItemLoader> loaders, params string[] extensions) : base(loaders, extensions)
    {
    }

    public override string StageName => "Shader Loading";

    public override void OnStageCompleted()
    {

    }

    public override void OnStageStarted()
    {

    }
}