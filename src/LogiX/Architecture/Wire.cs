using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX;

public enum WireStatus
{
    DISAGREE,
    AGREE,
    NONE,
}

public class WireNode
{
    public List<WireNode> Children { get; set; }
    public WireNode Parent { get; set; }

    public Vector2i Position { get; set; }

    public WireNode(Vector2i position)
    {
        Children = new List<WireNode>();
        Parent = null;
        Position = position;
    }

    public void AddChild(WireNode child)
    {
        Children.Add(child);
        child.Parent = this;
    }

    public Vector2i[] CollectPositions()
    {
        List<Vector2i> positions = new();
        this.CollectPositions(ref positions);
        return positions.ToArray();
    }

    private void CollectPositions(ref List<Vector2i> positions)
    {
        positions.Add(Position);

        foreach (WireNode child in Children)
        {
            child.CollectPositions(ref positions);
        }
    }
}

public class Wire
{
    public WireNode RootNode { get; set; }

    public Wire(Vector2i startPos, Vector2i endPos)
    {
        RootNode = new WireNode(startPos);
        RootNode.AddChild(new WireNode(endPos));
    }

    public void Render(Simulation simulation, Camera2D cam)
    {
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var color = this.GetWireColor(simulation);
        this.RenderNode(this.RootNode, pShader, cam, color);
    }

    private ColorF GetWireColor(Simulation simulation)
    {
        var positions = this.RootNode.CollectPositions();

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

        int highs = first.Count(v => v == LogicValue.HIGH);

        return ColorF.Lerp(ColorF.White, ColorF.Blue, (float)highs / first.Length);
    }

    private void RenderNode(WireNode node, ShaderProgram pShader, Camera2D camera, ColorF color)
    {
        var startPos = node.Position.ToVector2(16);

        foreach (var child in node.Children)
        {
            var endPos = child.Position.ToVector2(16);
            PrimitiveRenderer.RenderLine(pShader, startPos, endPos, 2, color, camera);
            this.RenderNode(child, pShader, camera, color);
        }
    }
}