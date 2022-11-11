using System.Drawing;
using System.Numerics;
using LogiX.Graphics;
using static LogiX.OpenGL.GL;

namespace LogiX.Rendering;


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
        return Utilities.GetMatrix4x4Values(ModelMatrix);
    }

    public float[] GetColorAsFloatArray()
    {
        return this.Color.ToFloatArray();
    }

    public float[] GetInstanceData()
    {
        float[] vData = GetVerticesAsFloatArray();
        float[] cData = GetColorAsFloatArray();
        float[] mData = GetModelMatrixAsFloatArray();

        float[] instanceData = new float[vData.Length + mData.Length + cData.Length];

        vData.CopyTo(instanceData, 0);
        cData.CopyTo(instanceData, vData.Length);
        mData.CopyTo(instanceData, vData.Length + cData.Length);

        return instanceData;
    }
}

public static class PrimitiveRenderer
{
    private static List<PrimitiveInstance> _instances = new List<PrimitiveInstance>();
    private static uint _vao;
    private static uint _vbo;

    public unsafe static void InitGL()
    {
        // VBO STRUCTURE
        // xy1, xy2, xy3, mat4, color

        _vao = glGenVertexArray();
        glBindVertexArray(_vao);

        _vbo = glGenBuffer();
        glBindBuffer(GL_ARRAY_BUFFER, _vbo);
        glBufferData(GL_ARRAY_BUFFER, 0, (void*)0, GL_STREAM_DRAW);
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

    public unsafe static void FinalizeRender(ShaderProgram shader, Camera2D camera)
    {
        glBindVertexArray(_vao);
        glBindBuffer(GL_ARRAY_BUFFER, _vbo);

        var instanceData = new List<float>();
        foreach (var instance in _instances)
        {
            instanceData.AddRange(instance.GetInstanceData());
        }

        var data = instanceData.ToArray();

        fixed (float* ptr = data)
        {
            glBufferData(GL_ARRAY_BUFFER, data.Length * sizeof(float), (void*)ptr, GL_STREAM_DRAW);
        }

        shader.Use(() =>
        {
            shader.SetMatrix4x4("projection", camera.GetProjectionMatrix());
            glDrawArraysInstanced(GL_TRIANGLES, 0, 3, _instances.Count);
        });


        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);

        _instances.Clear();
    }

    public static void RenderRectangle(RectangleF rect, Vector2 origin, float rotation, ColorF color)
    {
        var position = new Vector2(rect.X, rect.Y);
        var size = new Vector2(rect.Width, rect.Height);

        var model = Utilities.CreateModelMatrixFromPosition(position, rotation, origin, size);

        var tris = new PRectangle().GetTris();

        foreach (var tri in tris)
        {
            _instances.Add(new PrimitiveInstance(tri, model, color));
        }
    }

    public static void RenderCircle(Vector2 position, float radius, float rotation, ColorF color, float segmentPercentage = 1f, int sides = 10)
    {
        var model = Utilities.CreateModelMatrixFromPosition(position, rotation, Vector2.Zero, new Vector2(radius, radius));
        var tris = new PCircle(sides, segmentPercentage).GetTris();

        foreach (var tri in tris)
        {
            _instances.Add(new PrimitiveInstance(tri, model, color));
        }
    }

    public static void RenderLine(Vector2 start, Vector2 end, int width, ColorF color)
    {
        // We need a rectangle which is located at start.
        // Its width will be equal to the distance from start to end
        // Its height will be equal to width, but this might get strange - prefer 1
        // The rotation will be equal to the arctan of triangle

        // Calculate width (distance from start to end)
        float distance = (end - start).Length();

        // Height should be one, bu
        int height = width;

        // Rotation (angle from start to end)
        float xDist = (end.X - start.X);
        float yDist = (end.Y - start.Y);

        float rotation = MathF.Atan2(yDist, xDist);

        RectangleF rec = new RectangleF(start.X, start.Y - height / 2, distance, height);

        RenderRectangle(rec, new Vector2(0, 0.5f), rotation, color);
    }

    public static void RenderFunction(Vector2 position, float rightDirectionAngle, float length, Func<float, float> func, ColorF color, float deltaSteps = 1f)
    {
        var start = position;
        var points = new List<Vector2>();
        var x = 0f;

        while (x < length)
        {
            var y = func(x);
            var point = new Vector2(x, -y);
            var end = start + point;
            points.Add(end.RotateAround(start, rightDirectionAngle));

            x += deltaSteps;
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            RenderLine(points[i], points[i + 1], 1, color);
        }
    }

    public static void RenderFunction(Vector2 startPosition, Vector2 endPosition, Func<float, float> func, ColorF color, float deltaSteps = 1f)
    {
        var length = (endPosition - startPosition).Length();
        var rightDirectionAngle = MathF.Atan2(endPosition.Y - startPosition.Y, endPosition.X - startPosition.X);

        RenderFunction(startPosition, rightDirectionAngle, length, func, color, deltaSteps);
    }
}