using System.Diagnostics.CodeAnalysis;
using QuikGraph;
using QuikGraph.Algorithms.Search;
using QuikGraph.Algorithms.ConnectedComponents;

namespace LogiX.Components;

public enum WireStatus
{
    NORMAL,
    ERROR,
    CONFLICT,
    DIFF_BITWIDTH,
    UNKNOWN,
}

public abstract class WireNode : ISelectable
{
    public abstract Vector2 GetPosition();
    public abstract bool IsPositionOn(Vector2 position);
    public abstract void Move(Vector2 delta);
    public void RenderSelected()
    {
        Raylib.DrawCircleV(this.GetPosition(), 4.5f, Color.ORANGE);
    }
}

public class IOWireNode : WireNode
{
    public IO IO { get; set; }

    public IOWireNode(IO io)
    {
        this.IO = io;
    }

    public override Vector2 GetPosition()
    {
        return this.IO.GetPosition();
    }

    public override bool IsPositionOn(Vector2 position)
    {
        return (this.GetPosition() - position).Length() < 5;
    }

    public override void Move(Vector2 delta)
    {
        // DO NOTHING
    }
}

public class JunctionWireNode : WireNode
{
    public Vector2 Position { get; set; }

    public JunctionWireNode(Vector2 position)
    {
        this.Position = position;
    }

    public override Vector2 GetPosition()
    {
        return this.Position;
    }

    public override bool IsPositionOn(Vector2 position)
    {
        return (this.GetPosition() - position).Length() < 5;
    }

    public override void Move(Vector2 delta)
    {
        this.Position += delta;
    }
}

public class Wire
{
    public List<IO> IOs { get; private set; }
    public WireStatus Status { get; set; }

    private UndirectedGraph<WireNode, Edge<WireNode>> _graph;
    public UndirectedGraph<WireNode, Edge<WireNode>> Graph { get => _graph; set => _graph = value; }

    public Wire()
    {
        this.IOs = new List<IO>();
        this._graph = new UndirectedGraph<WireNode, Edge<WireNode>>();
    }

    public Wire(IO initialIO)
    {
        this.IOs = new List<IO>();
        this._graph = new UndirectedGraph<WireNode, Edge<WireNode>>();

        this.ConnectIO(initialIO);
    }

    public void ConnectIO(IO io)
    {
        this.IOs.Add(io);
        io.Wire = this;
    }

    public void DisconnectAllIOs()
    {
        foreach (IO io in this.IOs)
        {
            io.Wire = null;
        }
        this.IOs.Clear();
    }

    public void DisconnectIO(IO io)
    {
        //io.SetValues(Util.NValues(LogicValue.UNKNOWN, io.BitWidth).ToArray());
        io.Wire = null;
        this.IOs.Remove(io);
    }

    public bool HasAnyIOs() => this.IOs.Count > 0;

    public bool AllIOsSameBitWidth(out int bitWidth)
    {
        bitWidth = this.IOs[0].BitWidth;
        foreach (IO io in this.IOs)
        {
            if (io.BitWidth != bitWidth)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsConnectedTo(IO io)
    {
        return this.IOs.Contains(io);
    }

    public bool AllIOsUnknown() => this.IOs.All(io => io.HasUnknown());

    public bool AnyIOPushing() => this.IOs.Any(io => io.IsPushing());

    public bool IOsAgree(IO[] ios, int bitWidth, out LogicValue[] agreedValues)
    {
        LogicValue[] valuesToAgreeOn = ios[0].PushedValues;
        LogicValue[] conjunction = valuesToAgreeOn;
        for (int i = 1; i < ios.Length; i++)
        {
            for (int j = 0; j < bitWidth; j++)
            {
                if (ios[i].PushedValues[j] != valuesToAgreeOn[j])
                {
                    conjunction[j] = LogicValue.ERROR;
                }
                else
                {
                    conjunction[j] = valuesToAgreeOn[j];
                }
            }
        }

        agreedValues = conjunction;
        return conjunction.SameAs(valuesToAgreeOn);
    }

    public void Propagate()
    {
        if (this.TryGetValuesToPropagate(out LogicValue[]? values))
        {
            foreach (IO io in this.IOs)
            {
                io.SetValues(values);
            }
        }
    }

    public bool TryGetValuesToPropagate([NotNullWhen(true)] out LogicValue[]? values)
    {
        if (!this.HasAnyIOs())
        {
            this.Status = WireStatus.UNKNOWN;
            values = null;
            return true;
        }

        if (!this.AllIOsSameBitWidth(out int bitWidth))
        {
            this.Status = WireStatus.DIFF_BITWIDTH;
            values = null;
            return false;
        }

        if (this.AllIOsUnknown() && !this.AnyIOPushing())
        {
            this.Status = WireStatus.UNKNOWN;
            values = Util.NValues(LogicValue.UNKNOWN, bitWidth).ToArray();
            return true;
        }

        if (!this.AnyIOPushing())
        {
            this.Status = WireStatus.UNKNOWN;
            values = Util.NValues(LogicValue.UNKNOWN, bitWidth).ToArray();
            return true;
        }

        IO[] pushingIOs = this.IOs.Where(io => io.IsPushing()).ToArray();
        if (!this.IOsAgree(pushingIOs, bitWidth, out LogicValue[] agreedValues))
        {
            this.Status = WireStatus.CONFLICT;
        }

        this.Status = WireStatus.NORMAL;
        values = agreedValues;
        return true;
    }

    public void Render()
    {
        Color color = this.IOs.Count > 0 ? this.IOs[0].GetColor() : Color.GRAY;

        foreach (Edge<WireNode> edge in this.Graph.Edges)
        {
            Vector2 start = edge.Source.GetPosition();
            Vector2 end = edge.Target.GetPosition();
            Raylib.DrawLineEx(start, end, 6f, Color.BLACK);
            Raylib.DrawLineEx(start, end, 4f, color);
            start = end;
        }

        foreach (WireNode node in this.Graph.Vertices.Where(x => x is JunctionWireNode))
        {
            Vector2 position = node.GetPosition();
            Raylib.DrawCircleV(position, 3.5f, Color.BLACK);
            Raylib.DrawCircleV(position, 2.5f, color);
        }
    }

    public bool TryGetJunctionFromPosition(Vector2 position, [NotNullWhen(true)] out JunctionWireNode? junction)
    {
        foreach (WireNode vertex in this.Graph.Vertices)
        {
            if (vertex.IsPositionOn(position) && vertex is JunctionWireNode jwn)
            {
                junction = jwn;
                return true;
            }
        }

        junction = null;
        return false;
    }

    public bool TryGetEdgeFromPosition(Vector2 position, [NotNullWhen(true)] out Edge<WireNode>? edge)
    {
        foreach (Edge<WireNode> e in this.Graph.Edges)
        {
            Rectangle rec = Util.CreateRecFromTwoCorners(e.Source.GetPosition(), e.Target.GetPosition()).Inflate(5f / 2);

            if (rec.ContainsVector2(position) && Util.DistanceToLine(e.Source.GetPosition(), e.Target.GetPosition(), position) <= 5f)
            {
                edge = e;
                return true;
            }
        }

        edge = null;
        return false;
    }

    public bool TryGetIOWireNodeFromPosition(Vector2 position, [NotNullWhen(true)] out IOWireNode? node)
    {
        foreach (WireNode vertex in this.Graph.Vertices)
        {
            if (vertex.IsPositionOn(position) && vertex is IOWireNode iown)
            {
                node = iown;
                return true;
            }
        }

        node = null;
        return false;
    }

    public IOWireNode CreateIOWireNode(IO io)
    {
        IOWireNode node = new IOWireNode(io);
        this.Graph.AddVertex(node);
        return node;
    }

    public JunctionWireNode CreateJunctionWireNode(Vector2 junction)
    {
        JunctionWireNode node = new JunctionWireNode(junction);
        this.Graph.AddVertex(node);
        return node;
    }

    public void UpdateIOs()
    {
        this.DisconnectAllIOs();

        foreach (WireNode wn in this.Graph.Vertices)
        {
            if (wn is IOWireNode iown)
            {
                this.ConnectIO(iown.IO);
            }
        }
    }

    public void AddNode(WireNode node)
    {
        this.Graph.AddVertex(node);

        this.UpdateIOs();
    }

    public void RemoveNode(WireNode node)
    {
        this.Graph.RemoveVertex(node);

        this.UpdateIOs();
    }

    public bool ConnectNodes(WireNode source, Wire targetWire, WireNode target)
    {
        this.Graph = Util.ConnectVertices(this.Graph, source, targetWire.Graph, target, out bool delete);

        this.UpdateIOs();
        return delete;
    }

    public bool DisconnectNodes(WireNode source, WireNode target, [NotNullWhen(true)] out Wire? newWire)
    {
        if (Util.DisconnectVertices(ref this._graph, source, target, out UndirectedGraph<WireNode, Edge<WireNode>>? newGraph))
        {
            newWire = new Wire();
            newWire.Graph = newGraph;
            newWire.UpdateIOs();

            this.UpdateIOs();
            return true;
        }

        this.UpdateIOs();

        newWire = null;
        return false;
    }

    public void InsertNodeBetween(WireNode source, WireNode target, WireNode insert)
    {
        this.Graph.RemoveEdgeIf(e => e.Source == source && e.Target == target);
        this.Graph.AddEdge(new Edge<WireNode>(source, insert));
        this.Graph.AddEdge(new Edge<WireNode>(insert, target));
    }
}