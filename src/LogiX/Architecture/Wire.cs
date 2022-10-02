using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX;

public enum WireStatus
{
    DISAGREE,
    AGREE,
    NONE,
}

public class Wire
{
    public List<(Vector2i, Vector2i)> Segments { get; private set; }

    public Wire(Vector2i startPos, Vector2i endPos)
    {
        this.Segments = new List<(Vector2i, Vector2i)>();
        this.Segments.Add((startPos, endPos));
    }

    public Wire()
    {

    }

    public Vector2i[] GetPoints()
    {
        var points = new List<Vector2i>();
        foreach (var segment in this.Segments)
        {
            var ps = Utilities.GetAllGridPointsBetween(segment.Item1, segment.Item2);
            points.AddRange(ps);
        }

        return points.Distinct().ToArray();
    }

    public void Render(Simulation simulation, Camera2D cam)
    {
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var color = this.GetWireColor(simulation);

        foreach (var segment in this.Segments)
        {
            var a = segment.Item1.ToVector2(16);
            var b = segment.Item2.ToVector2(16);

            PrimitiveRenderer.RenderLine(pShader, a, b, 2, color, cam);
        }

        var segmentPoints = this.Segments.SelectMany(s => new Vector2i[] { s.Item1, s.Item2 }).Distinct().ToArray();

        foreach (var point in segmentPoints)
        {
            var worldPos = point.ToVector2(16);
            PrimitiveRenderer.RenderCircle(pShader, worldPos, 4, 0, color, cam);
        }
    }

    private ColorF GetWireColor(Simulation simulation)
    {
        var positions = this.GetPoints();

        List<LogicValue[]> values = new();
        foreach (var pos in positions)
        {
            if (simulation.TryGetLogicValuesAtPosition(pos, out var vs, out var status))
            {
                values.Add(vs);
            }
            else
            {
                if (status == LogicValueRetrievalStatus.DISAGREE)
                {
                    return ColorF.Red;
                }
            }
        }

        if (values.Count == 0)
        {
            return ColorF.Gray;
        }

        var first = values[0];
        var allSame = values.All(v => v.SequenceEqual(first));
        if (!allSame)
        {
            return ColorF.Red;
        }

        return Utilities.GetValueColor(first);
    }

    public void AddSegment(Vector2i startPos, Vector2i endPos)
    {
        this.Segments.Add((startPos, endPos));
    }

    public void MergeWith(Wire other)
    {
        this.Segments.AddRange(other.Segments);
    }

    public WireDescription GetDescriptionOfInstance()
    {
        //return this.RootNode.GetDescription();
        throw new NotImplementedException();
    }
}