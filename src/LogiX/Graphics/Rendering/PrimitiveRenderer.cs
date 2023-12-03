using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using LogiX.Graphics;
using LogiX.Graphics.Cameras;
using static DotGL.GL;

namespace LogiX.Rendering;

public interface IPrimitive
{
    IEnumerable<(Vector2, Vector2, Vector2)> GetTris();
}

public class PRectangle : IPrimitive
{
    public IEnumerable<(Vector2, Vector2, Vector2)> GetTris()
    {
        yield return (new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1));
        yield return (new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 0));
    }
}

public class PCircle : IPrimitive
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
        Vector2[] points = new Vector2[sides + 1];
        points[0] = new Vector2(0, 0);

        float samplePointStepSize = MathF.PI * 2f / sides;
        float phi = 0f;

        for (int i = 1; i < sides + 1; i++)
        {
            float x = MathF.Cos(phi);
            float y = MathF.Sin(phi);

            phi += samplePointStepSize;

            points[i] = new Vector2(x, y);
        }

        return points;
    }

    private uint[] GetIndices()
    {
        uint[] indices = new uint[(_sides) * 3];

        for (int i = 0; i < _sides - 1; i++)
        {
            indices[i * 3] = 0;
            indices[i * 3 + 1] = (uint)(i + 1);
            indices[i * 3 + 2] = (uint)(i + 2);
        }

        indices[indices.Length - 3] = 0;
        indices[indices.Length - 2] = (uint)_sides;
        indices[indices.Length - 1] = 1;

        return indices;
    }

    public IEnumerable<(Vector2, Vector2, Vector2)> GetTris()
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
}

public struct PrimitiveInstance // Just a triangle
{
    public Vector2 V1 { get; set; }
    public Vector2 V2 { get; set; }
    public Vector2 V3 { get; set; }
    public Matrix4x4 ModelMatrix { get; set; }
    public ColorF Color { get; set; }

    public PrimitiveInstance(Vector2 v1, Vector2 v2, Vector2 v3, Matrix4x4 modelMatrix, ColorF color)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        ModelMatrix = modelMatrix;
        Color = color;
    }

    public PrimitiveInstance((Vector2, Vector2, Vector2) tri, Matrix4x4 modelMatrix, ColorF color)
    {
        V1 = tri.Item1;
        V2 = tri.Item2;
        V3 = tri.Item3;
        ModelMatrix = modelMatrix;
        Color = color;
    }

    public float[] GetVerticesAsFloatArray()
    {
        return new float[]
        {
            V1.X, V1.Y,
            V2.X, V2.Y,
            V3.X, V3.Y
        };
    }

    public float[] GetModelMatrixAsFloatArray()
    {
        return new float[]
        {
            ModelMatrix.M11, ModelMatrix.M12, ModelMatrix.M13, ModelMatrix.M14,
            ModelMatrix.M21, ModelMatrix.M22, ModelMatrix.M23, ModelMatrix.M24,
            ModelMatrix.M31, ModelMatrix.M32, ModelMatrix.M33, ModelMatrix.M34,
            ModelMatrix.M41, ModelMatrix.M42, ModelMatrix.M43, ModelMatrix.M44
        };
    }

    public float[] GetColorAsFloatArray()
    {
        return new float[]
        {
            Color.R, Color.G, Color.B, Color.A
        };
    }

    public float[] GetInstanceData()
    {
        float[] instanceData = new float[26];
        GetVerticesAsFloatArray().CopyTo(instanceData, 0);
        GetColorAsFloatArray().CopyTo(instanceData, 6);
        GetModelMatrixAsFloatArray().CopyTo(instanceData, 10);
        return instanceData;
    }
}

public class PrimitiveRenderer
{
    private float[] _instanceData = new float[0];
    private float[] _lastInstanceData = new float[0];
    public int _submittedInstances = 0;
    private uint _vao;
    private uint _vbo;

    public PrimitiveRenderer()
    {
        InitGL(1000);
    }

    public unsafe void InitGL(int initialCapacity)
    {
        // VBO STRUCTURE
        // xy1, xy2, xy3, mat4, color
        SetCapacity(initialCapacity);

        _vao = glGenVertexArray();
        glBindVertexArray(_vao);

        _vbo = glGenBuffer();
        glBindBuffer(GL_ARRAY_BUFFER, _vbo);
        glBufferData(GL_ARRAY_BUFFER, 0, (void*)0, GL_DYNAMIC_DRAW);
        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 2, GL_FLOAT, false, 26 * sizeof(float), (void*)0);
        glVertexAttribDivisor(0, 1);

        glEnableVertexAttribArray(1);
        glVertexAttribPointer(1, 2, GL_FLOAT, false, 26 * sizeof(float), (void*)(2 * sizeof(float)));
        glVertexAttribDivisor(1, 1);

        glEnableVertexAttribArray(2);
        glVertexAttribPointer(2, 2, GL_FLOAT, false, 26 * sizeof(float), (void*)(4 * sizeof(float)));
        glVertexAttribDivisor(2, 1);

        glEnableVertexAttribArray(3);
        glVertexAttribPointer(3, 4, GL_FLOAT, false, 26 * sizeof(float), (void*)(6 * sizeof(float)));
        glVertexAttribDivisor(3, 1);

        // Here we have the matrix4x4
        glEnableVertexAttribArray(4);
        glVertexAttribPointer(4, 4, GL_FLOAT, false, 26 * sizeof(float), (void*)(10 * sizeof(float)));
        glVertexAttribDivisor(4, 1);

        glEnableVertexAttribArray(5);
        glVertexAttribPointer(5, 4, GL_FLOAT, false, 26 * sizeof(float), (void*)(14 * sizeof(float)));
        glVertexAttribDivisor(5, 1);

        glEnableVertexAttribArray(6);
        glVertexAttribPointer(6, 4, GL_FLOAT, false, 26 * sizeof(float), (void*)(18 * sizeof(float)));
        glVertexAttribDivisor(6, 1);

        glEnableVertexAttribArray(7);
        glVertexAttribPointer(7, 4, GL_FLOAT, false, 26 * sizeof(float), (void*)(22 * sizeof(float)));
        glVertexAttribDivisor(7, 1);

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);
    }

    private void SetCapacity(int capacity)
    {
        var newArray = new float[capacity * 26];
        _instanceData.CopyTo(newArray, 0);
        _instanceData = newArray;
    }

    public unsafe void FinalizeRender(ShaderProgram shader, ICamera2D camera)
    {
        glBindVertexArray(_vao);
        glBindBuffer(GL_ARRAY_BUFFER, _vbo);

        fixed (float* ptr = &_instanceData[0])
        {
            glBufferData(GL_ARRAY_BUFFER, _submittedInstances * 26 * sizeof(float), (void*)ptr, GL_DYNAMIC_DRAW);
        }

        shader.UseWith(() =>
        {
            shader.SetUniformMatrix4f("projection", false, camera.GetProjectionMatrix());
            glDrawArraysInstanced(GL_TRIANGLES, 0, 3, _submittedInstances);
        });

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);

        _submittedInstances = 0;
    }

    private void AddInstance(PrimitiveInstance instance)
    {
        // Increment
        if (_submittedInstances + 1 >= _instanceData.Length / 26)
        {
            SetCapacity(_instanceData.Length * 2);
        }

        var i = _submittedInstances * 26;

        _instanceData[i] = instance.V1.X;
        _instanceData[i + 1] = instance.V1.Y;
        _instanceData[i + 2] = instance.V2.X;
        _instanceData[i + 3] = instance.V2.Y;
        _instanceData[i + 4] = instance.V3.X;
        _instanceData[i + 5] = instance.V3.Y;
        _instanceData[i + 6] = instance.Color.R;
        _instanceData[i + 7] = instance.Color.G;
        _instanceData[i + 8] = instance.Color.B;
        _instanceData[i + 9] = instance.Color.A;
        _instanceData[i + 10] = instance.ModelMatrix.M11;
        _instanceData[i + 11] = instance.ModelMatrix.M12;
        _instanceData[i + 12] = instance.ModelMatrix.M13;
        _instanceData[i + 13] = instance.ModelMatrix.M14;
        _instanceData[i + 14] = instance.ModelMatrix.M21;
        _instanceData[i + 15] = instance.ModelMatrix.M22;
        _instanceData[i + 16] = instance.ModelMatrix.M23;
        _instanceData[i + 17] = instance.ModelMatrix.M24;
        _instanceData[i + 18] = instance.ModelMatrix.M31;
        _instanceData[i + 19] = instance.ModelMatrix.M32;
        _instanceData[i + 20] = instance.ModelMatrix.M33;
        _instanceData[i + 21] = instance.ModelMatrix.M34;
        _instanceData[i + 22] = instance.ModelMatrix.M41;
        _instanceData[i + 23] = instance.ModelMatrix.M42;
        _instanceData[i + 24] = instance.ModelMatrix.M43;
        _instanceData[i + 25] = instance.ModelMatrix.M44;

        _submittedInstances++;
    }

    private static Matrix4x4 CreateModelMatrixFromPosition(Vector2 position, float rotation, Vector2 origin, Vector2 scale)
    {
        Matrix4x4 translate = Matrix4x4.CreateTranslation(new Vector3(position, 0));
        Matrix4x4 rotate = Matrix4x4.CreateRotationZ(rotation);
        Matrix4x4 scaleM = Matrix4x4.CreateScale(new Vector3(scale, 0));
        Matrix4x4 originT = Matrix4x4.CreateTranslation(new Vector3(origin * scale, 0));
        Matrix4x4 originNeg = Matrix4x4.CreateTranslation(new Vector3(-origin * scale, 0));

        return scaleM * originNeg * rotate * originT * translate;
    }

    public void RenderRectangle(RectangleF rect, Vector2 origin, float rotation, ColorF color)
    {
        var position = new Vector2(rect.X, rect.Y);
        var size = new Vector2(rect.Width, rect.Height);

        var model = CreateModelMatrixFromPosition(position, rotation, origin, size);

        var tris = new PRectangle().GetTris();

        foreach (var tri in tris)
        {
            AddInstance(new PrimitiveInstance(tri, model, color));
        }
    }

    public void RenderRectangleWithBorder(RectangleF rect, Vector2 origin, float rotation, float borderSize, ColorF color, ColorF borderColor)
    {
        RenderRectangle(rect, origin, rotation, borderColor);

        rect.Inflate(-borderSize, -borderSize);

        RenderRectangle(rect, origin, rotation, color);
    }

    public void RenderCircle(Vector2 position, float radius, float rotation, ColorF color, float segmentPercentage = 1f, int sides = 10)
    {
        var model = CreateModelMatrixFromPosition(position, rotation, Vector2.Zero, new Vector2(radius, radius));
        var tris = new PCircle(sides, segmentPercentage).GetTris();

        foreach (var tri in tris)
        {
            AddInstance(new PrimitiveInstance(tri, model, color));
        }
    }

    public void RenderCircleOutline(Vector2 position, float radius, int borderSize, ColorF color, float rotation = 0f, float segmentPercentage = 1f, int sides = 10)
    {
        var perSide = MathF.PI * 2f / sides;

        for (var i = 0; i < sides * segmentPercentage; i++)
        {
            var angle = (i * perSide) + rotation;
            var angle2 = ((i + 1) * perSide) + rotation;

            var v1 = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            var v2 = new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius;

            RenderLine(position + v1, position + v2, borderSize, color);
        }
    }

    public void RenderLine(Vector2 start, Vector2 end, float width, ColorF color)
    {
        // We need a rectangle which is located at start.
        // Its width will be equal to the distance from start to end
        // Its height will be equal to width, but this might get strange - prefer 1
        // The rotation will be equal to the arctan of triangle

        // Calculate width (distance from start to end)
        float distance = (end - start).Length();

        // Height should be one, bu
        float height = width;

        // Rotation (angle from start to end)
        float xDist = (end.X - start.X);
        float yDist = (end.Y - start.Y);

        float rotation = MathF.Atan2(yDist, xDist);

        RectangleF rec = new RectangleF(start.X, start.Y - height / 2f, distance, height);

        RenderRectangle(rec, new Vector2(0, 0.5f), rotation, color);
    }

    public void RenderFunction(Vector2 position, float rightDirectionAngle, float length, Func<float, float> func, ColorF color, float deltaSteps = 1f)
    {
        var start = position;
        var points = new List<Vector2>();
        var x = 0f;

        while (x < length)
        {
            var y = func(x);
            var point = new Vector2(x, -y);
            var end = start + point;
            points.Add(RotateAround(end, start, rightDirectionAngle));

            x += deltaSteps;
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            RenderLine(points[i], points[i + 1], 1, color);
        }
    }

    public void RenderFunction(Vector2 startPosition, Vector2 endPosition, Func<float, float> func, ColorF color, float deltaSteps = 1f)
    {
        var length = (endPosition - startPosition).Length();
        var rightDirectionAngle = MathF.Atan2(endPosition.Y - startPosition.Y, endPosition.X - startPosition.X);

        RenderFunction(startPosition, rightDirectionAngle, length, func, color, deltaSteps);
    }

    private static Vector2 RotateAround(Vector2 v, Vector2 pivot, float angle)
    {
        Vector2 dir = v - pivot;
        return new Vector2(dir.X * MathF.Cos(angle) + dir.Y * MathF.Sin(angle),
                           -dir.X * MathF.Sin(angle) + dir.Y * MathF.Cos(angle)) + pivot;
    }
}
