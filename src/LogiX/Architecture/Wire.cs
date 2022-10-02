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

    public WireNode[] CollectNodes()
    {
        List<WireNode> nodes = new();
        this.CollectNodes(ref nodes);
        return nodes.ToArray();
    }

    private void CollectNodes(ref List<WireNode> nodes)
    {
        nodes.Add(this);
        foreach (var child in Children)
        {
            child.CollectNodes(ref nodes);
        }
    }

    private void CollectPositions(ref List<Vector2i> positions)
    {
        foreach (WireNode child in Children)
        {
            var positionsBetween = Utilities.GetAllGridPointsBetween(this.Position, child.Position);
            positions.AddRange(positionsBetween.Except(positions).Distinct());
            child.CollectPositions(ref positions);
        }
    }

    public WireDescription GetDescription()
    {
        WireDescription description = new(Position);

        foreach (var child in this.Children)
        {
            description.AddChild(child.GetDescription());
        }

        return description;
    }

    public void MakeRoot()
    {
        if (Parent == null)
        {
            return;
        }

        this.Parent.Children.Remove(this);
        var oldParent = this.Parent;
        this.Parent = null;

        this.AddChild(oldParent);
    }
}
public class Wire
{
    public WireNode RootNode { get; set; }

    public Wire(Vector2i startPos, Vector2i endPos)
    {
        RootNode = new WireNode(startPos);
        RootNode.Parent = null;
        RootNode.AddChild(new WireNode(endPos));
    }

    public Wire()
    {

    }

    public void MakeNodeRoot(WireNode node)
    {
        node.MakeRoot();
        this.RootNode = node;
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

    private void RenderNode(WireNode node, ShaderProgram pShader, Camera2D camera, ColorF color)
    {
        var startPos = node.Position.ToVector2(16);

        foreach (var child in node.Children)
        {
            var endPos = child.Position.ToVector2(16);
            PrimitiveRenderer.RenderLine(pShader, startPos, endPos, 2, color, camera);
            this.RenderNode(child, pShader, camera, color);
        }

        if (node.Children.Count > 1)
        {
            PrimitiveRenderer.RenderCircle(pShader, node.Position.ToVector2(16), 3f, 0f, color, camera);
        }

        if (node.Parent == null)
        {
            // I AM ROOT, RENDER BIGGER CIRCLE TO IDENTIFY
            PrimitiveRenderer.RenderCircle(pShader, node.Position.ToVector2(16), 5f, 0f, ColorF.CoralRed, camera);
        }

        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");
        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        TextRenderer.RenderText(tShader, font, node.Position.ToString(), node.Position.ToVector2(16), 1f, ColorF.Red, camera);
    }

    public bool IsPositionOnWire(Vector2i position)
    {
        var positions = this.RootNode.CollectPositions();
        return positions.Contains(position);
    }

    public WireNode GetNodeAtPosition(Vector2i position, bool createIfNone = false)
    {
        var node = this.GetNodeAtPosition(this.RootNode, position);
        if (node is null && createIfNone)
        {
            (var left, var right) = this.GetBetweenNodes(position);

            left.Children.Remove(right);
            var newNode = new WireNode(position);
            left.AddChild(newNode);
            newNode.AddChild(right);

            return newNode;
        }

        return node;
    }

    private WireNode GetNodeAtPosition(WireNode root, Vector2i position)
    {
        if (root.Position == position)
        {
            return root;
        }

        foreach (var child in root.Children)
        {
            var node = this.GetNodeAtPosition(child, position);
            if (node != null)
            {
                return node;
            }
        }

        return null;
    }

    private (WireNode, WireNode) GetBetweenNodes(Vector2i position)
    {
        var positions = this.RootNode.CollectPositions();
        var index = Array.IndexOf(positions, position);

        // Go left until find a node
        var leftIndex = index - 1;
        WireNode leftNode = null;
        while (leftIndex >= 0)
        {
            var leftPos = positions[leftIndex];
            leftNode = this.GetNodeAtPosition(leftPos);
            if (leftNode != null)
            {
                break;
            }

            leftIndex--;
        }

        // Go right until find a node
        var rightIndex = index + 1;
        WireNode rightNode = null;
        while (rightIndex < positions.Length)
        {
            var rightPos = positions[rightIndex];
            rightNode = this.GetNodeAtPosition(rightPos);
            if (rightNode != null)
            {
                break;
            }

            rightIndex++;
        }

        return (leftNode, rightNode);
    }

    public WireDescription GetDescriptionOfInstance()
    {
        return this.RootNode.GetDescription();
    }

    public static Wire Connect(Wire w1, WireNode wn1, Wire w2, WireNode wn2)
    {
        if (w1 == w2)
        {
            throw new Exception("Cannot connect wire to itself");
        }
        else
        {
            // Connect two different wires
            Wire wire = new();
            wn1.MakeRoot();
            wn2.MakeRoot();

            wn1.AddChild(wn2);
            wire.RootNode = wn1;
            return wire;
        }
    }
}