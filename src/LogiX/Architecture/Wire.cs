using System.Drawing;
using System.Numerics;
using System.Text;
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
    DIFF_WIDTH
}

public class Wire
{
    public List<(Vector2i, Vector2i)> Segments { get; set; }

    public Wire(Vector2i startPos, Vector2i endPos)
    {
        this.Segments = new List<(Vector2i, Vector2i)>();
        this.Segments.Add((startPos, endPos));
    }

    public Wire()
    {

    }

    public Wire(List<(Vector2i, Vector2i)> segments)
    {
        this.Segments = segments;
    }

    public string GetHash()
    {
        var hash = new StringBuilder();
        foreach (var segment in this.Segments)
        {
            hash.Append(segment.Item1.X);
            hash.Append(segment.Item1.Y);
            hash.Append(segment.Item2.X);
            hash.Append(segment.Item2.Y);
        }

        return Utilities.GetHash(hash.ToString());
    }

    public bool HasEdgeVertexAt(Vector2i point)
    {
        foreach (var segment in this.Segments)
        {
            if (segment.Item1 == point || segment.Item2 == point)
            {
                return true;
            }
        }

        return false;
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
        var color = GetWireColor(this.GetPoints(), simulation);

        foreach (var segment in this.Segments)
        {
            var a = segment.Item1.ToVector2(Constants.GRIDSIZE);
            var b = segment.Item2.ToVector2(Constants.GRIDSIZE);

            PrimitiveRenderer.RenderLine(pShader, a, b, Constants.WIRE_WIDTH, color, cam);
        }

        var segmentPoints = this.Segments.SelectMany(s => new Vector2i[] { s.Item1, s.Item2 }).Distinct().ToArray();

        foreach (var point in segmentPoints)
        {
            var worldPos = point.ToVector2(Constants.GRIDSIZE);
            //PrimitiveRenderer.RenderCircle(pShader, worldPos, Constants.WIRE_POINT_RADIUS, 0, color, cam);
            PrimitiveRenderer.RenderRectangle(pShader, new RectangleF(worldPos.X, worldPos.Y, 0, 0).Inflate(Constants.WIRE_WIDTH / 2f), Vector2.Zero, 0, color, cam);
        }
    }

    private static ColorF GetWireColor(Vector2i[] points, Simulation simulation)
    {
        var positions = points;

        List<LogicValue[]> values = new();
        foreach (var pos in positions)
        {
            if (simulation.TryGetLogicValuesAtPosition(pos, out var vs, out var status))
            {
                values.Add(vs);
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
        // Check if either the startpos or endpos is already on a segment
        if (this.TryGetSegmentAtPos(startPos, out var startSegment))
        {
            // We want to split the already existing segment into two
            var newSegment = (startSegment.Item1, startPos);
            var oldSegment = (startPos, startSegment.Item2);

            this.Segments.Remove(startSegment);
            this.Segments.Add(newSegment);
            this.Segments.Add(oldSegment);
        }
        if (this.TryGetSegmentAtPos(endPos, out var endSegment))
        {
            // We want to split the already existing segment into two
            var newSegment = (endSegment.Item1, endPos);
            var oldSegment = (endPos, endSegment.Item2);

            this.Segments.Remove(endSegment);
            this.Segments.Add(newSegment);
            this.Segments.Add(oldSegment);
        }

        this.Segments.Add((startPos, endPos));
        this.MergeEdgesThatMeetAt(startPos);
        this.MergeEdgesThatMeetAt(endPos);
    }

    public static Wire[] RemoveSegmentFromWire(Wire wire, (Vector2i, Vector2i) segment)
    {
        var newWires = new List<Wire>();

        wire.Segments.Remove(segment);

        if (wire.Segments.Count == 0)
        {
            return new Wire[0];
        }

        var a = segment.Item1;
        var b = segment.Item2;

        // Check if a is reachable from b
        if (Utilities.CanFindPositionInGraph(wire.Segments, b, a))
        {
            // There was no disconnect, we will keep the wire
            newWires.Add(wire);

            wire.MergeEdgesThatMeetAt(a);
            wire.MergeEdgesThatMeetAt(b);

            return newWires.ToArray();
        }
        else
        {
            var w1 = Utilities.FindAllTraversableEdges(wire.Segments, a);
            var w2 = Utilities.FindAllTraversableEdges(wire.Segments, b);

            if (w1.Count > 0)
            {
                var v1 = new Wire(w1);
                v1.MergeEdgesThatMeetAt(a);
                newWires.Add(v1);
            }
            if (w2.Count > 0)
            {
                var v2 = new Wire(w2);
                v2.MergeEdgesThatMeetAt(b);
                newWires.Add(v2);
            }

            return newWires.ToArray();
        }
    }

    public void MergeEdgesThatMeetAt(Vector2i position)
    {
        var edges = this.Segments.Where(s => s.Item1 == position || s.Item2 == position).ToArray();

        if (edges.Length != 2)
        {
            return;
        }

        var edgeA = edges[0];
        var edgeB = edges[1];

        if (!Utilities.AreEdgesParallel(edgeA, edgeB))
        {
            return;
        }

        var newEdge = (edgeA.Item1, edgeB.Item2);
        this.Segments.Remove(edgeA);
        this.Segments.Remove(edgeB);
        this.Segments.Add(newEdge);
    }

    public void MergeWith(Wire other)
    {
        var segments = other.Segments;

        foreach (var segment in segments)
        {
            this.AddSegment(segment.Item1, segment.Item2);
        }
    }

    public WireDescription GetDescriptionOfInstance()
    {
        return new WireDescription(this.Segments);
    }

    public bool TryGetSegmentAtPos(Vector2i position, out (Vector2i, Vector2i) segment)
    {
        foreach (var (a, b) in this.Segments)
        {
            if (Utilities.IsPositionBetween(a, b, position))
            {
                segment = (a, b);
                return true;
            }
        }

        segment = default;
        return false;
    }

    public Wire[] RemoveVertex(Vector2i position)
    {
        if (Utilities.VertexOnlyHasOneEdge(this.Segments, position, out var edge))
        {
            // This is a dead end, we can just remove it
            return Wire.RemoveSegmentFromWire(this, edge);
        }
        else
        {
            // Cannot remove this vertex since it has more than one edge
            return new Wire[] { this };
        }
    }
}