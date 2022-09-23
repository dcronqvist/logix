using System.Diagnostics.CodeAnalysis;
using Symphony;

namespace GoodGame.Content;

public abstract class Shader : GLContentItem<string>
{
    public uint ShaderID { get; protected set; }

    public Shader(string identifier, IContentSource source, string content) : base(identifier, source, content)
    {
    }

    public override void InitGL(string newContent)
    {
        if (this.TryCreateShader(newContent, out uint shader, out string error))
        {
            this.ShaderID = shader;
        }
        else
        {
            throw new Exception(error);
        }
    }

    public abstract bool TryCreateShader(string code, out uint shaderID, [NotNullWhen(false)] out string error);
}