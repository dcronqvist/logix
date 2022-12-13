using System.Drawing;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Graphics;
using LogiX.Rendering;
using QuikGraph;

namespace LogiX;

public enum WireStatus
{
    DISAGREE,
    AGREE,
    NONE,
    DIFF_WIDTH
}

public class Wire
{
    public List<(Vector2i, Vector2i)> Segments { get; set; } = new();

    [JsonConstructor]
    public Wire()
    {

    }

    public Wire(List<Edge<Vector2i>> edges)
    {
        this.Segments = edges.Select(e => (e.Source, e.Target)).ToList();
    }

    public static void Render(IEnumerable<Edge<Vector2i>> edges, ColorF color, Camera2D cam)
    {
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");

        var pointDegrees = new Dictionary<Vector2i, int>();
        foreach (var edge in edges)
        {
            var start = edge.Source;
            var end = edge.Target;

            if (!pointDegrees.ContainsKey(start))
            {
                pointDegrees.Add(start, 0);
            }
            if (!pointDegrees.ContainsKey(end))
            {
                pointDegrees.Add(end, 0);
            }

            pointDegrees[start]++;
            pointDegrees[end]++;

            RenderSegment(edge, color);
        }

        foreach (var (point, degree) in pointDegrees)
        {
            var worldPos = point.ToVector2(Constants.GRIDSIZE);

            // if (degree > 2)
            // {
            PrimitiveRenderer.RenderCircle(worldPos, Constants.WIRE_POINT_RADIUS, 0f, color);
            // }
            // else
            // {
            //     PrimitiveRenderer.RenderRectangle(new RectangleF(worldPos.X, worldPos.Y, 0, 0).Inflate(Constants.WIRE_WIDTH / 2f), Vector2.Zero, 0, color);
            // }
        }
    }

    public static void RenderSegment(Edge<Vector2i> edge, ColorF color)
    {
        var a = edge.Source.ToVector2(Constants.GRIDSIZE);
        var b = edge.Target.ToVector2(Constants.GRIDSIZE);

        PrimitiveRenderer.RenderLine(a, b, Constants.WIRE_WIDTH, color);
    }

    public static void RenderSegmentAsSelected(Edge<Vector2i> edge)
    {
        var a = edge.Source.ToVector2(Constants.GRIDSIZE);
        var b = edge.Target.ToVector2(Constants.GRIDSIZE);

        PrimitiveRenderer.RenderLine(a, b, Constants.WIRE_WIDTH + 2, Constants.COLOR_SELECTED);
    }

    public WireDescription GetDescriptionOfInstance()
    {
        return new(this.Segments);
    }
}