using System.Numerics;
using LogiX.Graphics;
using static LogiX.OpenGL.GL;

namespace LogiX.Rendering;

public abstract class Primitive
{
    public abstract (Vector2, Vector2, Vector2)[] GetTris();

    // protected bool IsShaderValid(ShaderProgram shader, out string[] missingAttribs, out string[] missingUniforms)
    // {
    //     bool hasAttribs = shader.HasAttribs(out missingAttribs, ("position", "vec2", 0));
    //     bool hasUniforms = shader.HasUniforms(out missingUniforms, ("projection", "mat4"), ("model", "mat4"), ("color", "vec4"));

    //     return hasAttribs && hasUniforms;
    // }

    // public virtual unsafe void Render(ShaderProgram shader, Matrix4x4 model, ColorF color, Camera2D camera)
    // {
    //     if (!IsShaderValid(shader, out string[] missingAttribs, out string[] missingUniforms))
    //     {
    //         throw new ArgumentException($"Shader is missing attributes: {string.Join(", ", missingAttribs)} and/or uniforms: {string.Join(", ", missingUniforms)}");
    //     }

    //     shader.Use(() =>
    //     {
    //         shader.SetMatrix4x4("projection", camera.GetProjectionMatrix());
    //         shader.SetMatrix4x4("model", model);
    //         shader.SetVec4("color", color.R, color.G, color.B, color.A);

    //         glBindVertexArray(_vao);
    //         glDrawElements(GL_TRIANGLES, (int)this.GetIndices().Length, GL_UNSIGNED_INT, (void*)0);
    //         glBindVertexArray(0);
    //     });
    // }
}