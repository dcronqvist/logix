using System.Numerics;
using LogiX.Graphics;
using static LogiX.OpenGL.GL;

namespace LogiX.Rendering;

public class PCircle : Primitive
{
    private int _sides;
    private float _segmentPercentage;

    public PCircle(int sides, float segmentPercentage)
    {
        _sides = sides;
        _segmentPercentage = segmentPercentage;
    }

    private Vector2[] GenerateCirclePoints(int sides)
    {
        Vector2[] points = new Vector2[sides + 2];
        points[0] = new Vector2(0, 0);

        float samplePointStepSize = MathF.PI * 2f / sides;
        float phi = 0f;

        for (int i = 1; i < sides + 2; i++)
        {
            float x = MathF.Cos(phi);
            float y = MathF.Sin(phi);

            phi += samplePointStepSize;

            points[i] = new Vector2(x, y);
        }

        return points;
    }

    protected uint[] GetIndices()
    {
        uint[] indices = new uint[(_sides + 1) * 3];

        for (int i = 0; i < _sides; i++)
        {
            indices[i * 3] = 0;
            indices[i * 3 + 1] = (uint)(i + 1);
            indices[i * 3 + 2] = (uint)(i + 2);
        }

        indices[indices.Length - 3] = 0;
        indices[indices.Length - 2] = (uint)_sides - 1;
        indices[indices.Length - 1] = 1;

        return indices;
    }

    public override (Vector2, Vector2, Vector2)[] GetTris()
    {
        var circlePoints = GenerateCirclePoints(_sides);
        var indices = GetIndices();

        var tris = new (Vector2, Vector2, Vector2)[(int)Math.Round(indices.Length * _segmentPercentage) / 3];

        for (int i = 0; i < tris.Length; i++)
        {
            tris[i] = (circlePoints[indices[i * 3]], circlePoints[indices[i * 3 + 1]], circlePoints[indices[i * 3 + 2]]);
        }

        return tris;
    }

    // public unsafe void Render(ShaderProgram shader, Matrix4x4 model, ColorF color, Camera2D camera, float segmentPercentage = 1f)
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
    //         glDrawElements(GL_TRIANGLES, (int)MathF.Round(this.GetIndices().Length * segmentPercentage), GL_UNSIGNED_INT, (void*)0);
    //         glBindVertexArray(0);
    //     });
    // }
}