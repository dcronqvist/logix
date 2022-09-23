using System.Drawing;
using System.Numerics;
using GoodGame.Graphics;

namespace GoodGame.Rendering;

public static class PrimitiveRenderer
{
    private static PRectangle _rectangle;
    private static PCircle _circle;

    public static void InitGL(int circleSides)
    {
        _rectangle = new PRectangle();
        _circle = new PCircle(circleSides);

        _rectangle.InitGL();
        _circle.InitGL();
    }

    public static void RenderRectangle(ShaderProgram shader, RectangleF rect, Vector2 origin, float rotation, ColorF color, Camera2D camera)
    {
        var position = new Vector2(rect.X, rect.Y);
        var size = new Vector2(rect.Width, rect.Height);

        var model = Utilities.CreateModelMatrixFromPosition(position, rotation, origin, size);
        _rectangle.Render(shader, model, color, camera);
    }

    public static void RenderCircle(ShaderProgram shader, Vector2 position, float radius, float rotation, ColorF color, Camera2D camera, float segmentPercentage = 1f)
    {
        var model = Utilities.CreateModelMatrixFromPosition(position, rotation, Vector2.Zero, new Vector2(radius, radius));
        _circle.Render(shader, model, color, camera, segmentPercentage);
    }

    public static void RenderCircle(ShaderProgram shader, Vector2 position, Vector2 radius, float rotation, ColorF color, Camera2D camera, float segmentPercentage = 1f)
    {
        var model = Utilities.CreateModelMatrixFromPosition(position, rotation, Vector2.Zero, radius);
        _circle.Render(shader, model, color, camera, segmentPercentage);
    }

    public static void RenderLine(ShaderProgram shader, Vector2 start, Vector2 end, int width, ColorF color, Camera2D camera)
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

        RectangleF rec = new RectangleF(start.X, start.Y, distance, height);

        RenderRectangle(shader, rec, new Vector2(0, 0.5f), rotation, color, camera);
    }

    public static void RenderFunction(ShaderProgram shader, Vector2 position, float rightDirectionAngle, float length, Func<float, float> func, ColorF color, Camera2D camera, float deltaSteps = 1f)
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
            RenderLine(shader, points[i], points[i + 1], 1, color, camera);
        }
    }

    public static void RenderFunction(ShaderProgram shader, Vector2 startPosition, Vector2 endPosition, Func<float, float> func, ColorF color, Camera2D camera, float deltaSteps = 1f)
    {
        var length = (endPosition - startPosition).Length();
        var rightDirectionAngle = MathF.Atan2(endPosition.Y - startPosition.Y, endPosition.X - startPosition.X);

        RenderFunction(shader, startPosition, rightDirectionAngle, length, func, color, camera, deltaSteps);
    }
}