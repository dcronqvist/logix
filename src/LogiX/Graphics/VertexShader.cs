using System.Diagnostics.CodeAnalysis;
using Symphony;
using static GoodGame.OpenGL.GL;

namespace GoodGame.Content;

public class VertexShader : Shader
{
    public VertexShader(string identifier, IContentSource source, string content) : base(identifier, source, content)
    {
    }

    public override void DestroyGL()
    {
        if (this.ShaderID != 0)
        {
            glDeleteShader(this.ShaderID);
            this.ShaderID = 0;
        }
    }

    public override bool IsGLInitialized()
    {
        return this.ShaderID != 0;
    }

    public unsafe override bool TryCreateShader(string code, out uint shaderID, [NotNullWhen(false)] out string error)
    {
        uint vs = glCreateShader(GL_VERTEX_SHADER);
        glShaderSource(vs, code);
        glCompileShader(vs);

        int* status = stackalloc int[1];
        glGetShaderiv(vs, GL_COMPILE_STATUS, status);

        if (*status == GL_FALSE)
        {
            int* length = stackalloc int[1];
            glGetShaderiv(vs, GL_INFO_LOG_LENGTH, length);
            string info = glGetShaderInfoLog(vs, *length);

            error = $"Failed to compile vertex shader: {info}";
            glDeleteShader(vs);
            shaderID = 0;
            return false;
        }

        error = null;
        shaderID = vs;
        return true;
    }
}