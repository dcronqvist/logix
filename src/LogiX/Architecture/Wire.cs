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
        foreach (var edge in edges)
        {
            RenderSegment(edge, color);
            PrimitiveRenderer.RenderCircle(edge.Source.ToVector2(Constants.GRIDSIZE), Constants.WIRE_POINT_RADIUS, 0f, color);
            PrimitiveRenderer.RenderCircle(edge.Target.ToVector2(Constants.GRIDSIZE), Constants.WIRE_POINT_RADIUS, 0f, color);
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