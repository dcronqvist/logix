using System.Diagnostics.CodeAnalysis;
using LogiX.Graphics;
using Symphony;

namespace LogiX.Content;

public class ShaderProgramLoadingStage : BaseLoadingStage
{
    public ShaderProgramLoadingStage(Dictionary<string, IContentItemLoader> loaders, bool doGLInit, params string[] extensions) : base(loaders, doGLInit, extensions)
    {
    }

    public override string StageName => "Shader Program Creation";

    public override void OnStageCompleted()
    {

    }

    public override void OnStageStarted()
    {

    }
}