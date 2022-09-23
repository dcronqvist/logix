using System.Numerics;
using LogiX.Graphics;
using static LogiX.OpenGL.GL;

namespace LogiX.Rendering;

public abstract class Primitive
{
    protected uint _vao;
    protected uint _vbo;
    protected uint _ebo;

    protected abstract Vector2[] GetVertices();
    protected abstract uint[] GetIndices();

    public unsafe void InitGL()
    {
        _vao = glGenVertexArray();
        _vbo = glGenBuffer();
        _ebo = glGenBuffer();

        glBindVertexArray(_vao);

        var vertices = this.GetVertices().SelectMany(v => new float[] { v.X, v.Y }).ToArray();
        var indices = this.GetIndices();

        glBindBuffer(GL_ARRAY_BUFFER, _vbo);
        fixed (float* v = &vertices[0])
        {
            glBufferData(GL_ARRAY_BUFFER, (int)(vertices.Length * sizeof(float)), v, GL_STATIC_DRAW);
        }

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);
        fixed (uint* i = &indices[0])
        {
            glBufferData(GL_ELEMENT_ARRAY_BUFFER, (int)(indices.Length * sizeof(uint)), i, GL_STATIC_DRAW);
        }

        glVertexAttribPointer(0, 2, GL_FLOAT, false, 2 * sizeof(float), (void*)0);
        glEnableVertexAttribArray(0);

        glBindVertexArray(0);
    }

    protected bool IsShaderValid(ShaderProgram shader, out string[] missingAttribs, out string[] missingUniforms)
    {
        bool hasAttribs = shader.HasAttribs(out missingAttribs, ("position", "vec2", 0));
        bool hasUniforms = shader.HasUniforms(out missingUniforms, ("projection", "mat4"), ("model", "mat4"), ("color", "vec4"));

        return hasAttribs && hasUniforms;
    }

    public virtual unsafe void Render(ShaderProgram shader, Matrix4x4 model, ColorF color, Camera2D camera)
    {
        if (!IsShaderValid(shader, out string[] missingAttribs, out string[] missingUniforms))
        {
            throw new ArgumentException($"Shader is missing attributes: {string.Join(", ", missingAttribs)} and/or uniforms: {string.Join(", ", missingUniforms)}");
        }

        shader.Use(() =>
        {
            shader.SetMatrix4x4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4x4("model", model);
            shader.SetVec4("color", color.R, color.G, color.B, color.A);

            glBindVertexArray(_vao);
            glDrawElements(GL_TRIANGLES, (int)this.GetIndices().Length, GL_UNSIGNED_INT, (void*)0);
            glBindVertexArray(0);
        });
    }
}