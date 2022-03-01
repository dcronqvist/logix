using System.Diagnostics.CodeAnalysis;

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
    public List<WireNode> Children { get; set; }
    public WireNode? Parent { get; set; }
    public Wire Wire { get; set; }

    public WireNode(Wire wire, WireNode? parent)
    {
        this.Wire = wire;
        this.Children = new List<WireNode>();
        this.Parent = parent;
    }

    public bool IsHorizontalTo(WireNode other)
    {
        return this.GetPosition().Y == other.GetPosition().Y;
    }

    public bool IsVerticalTo(WireNode other)
    {
        return this.GetPosition().X == other.GetPosition().X;
    }

    public bool TryFindChildIOWireNode(IO io, [NotNullWhen(true)] out IOWireNode? node)
    {
        if (this is IOWireNode ioNode && ioNode.IO == io)
        {
            node = ioNode;
            return true;
        }

        foreach (WireNode child in this.Children)
        {
            if (child is IOWireNode iowire && iowire.IO == io)
            {
                node = iowire;
                return true;
            }

            if (child.TryFindChildIOWireNode(io, out node))
            {
                return true;
            }
        }
        node = null;
        return false;
    }

    public void AddChild(WireNode child)
    {
        this.Children.Add(child);
    }

    public WireNode AddIOWireNode(IO io)
    {
        IOWireNode iwn = new IOWireNode(this.Wire, this, io);
        this.AddChild(iwn);
        return iwn;
    }

    public WireNode AddJunctionWireNode(Vector2 position)
    {
        JunctionWireNode jwn = new JunctionWireNode(this.Wire, this, position);
        this.AddChild(jwn);
        return jwn;
    }

    public void RenderSelected()
    {
        //Raylib.DrawLineEx(this.Parent!.GetPosition(), this.GetPosition(), 8, Color.ORANGE);
        Raylib.DrawCircleV(this.GetPosition(), 4.5f, Color.ORANGE);
    }

    public bool IsPositionOnAnyDirectChildWire(Vector2 position, [NotNullWhen(true)] out WireNode? directChild)
    {
        float wireThickness = 3f;

        foreach (WireNode child in this.Children)
        {
            Rectangle r = Util.CreateRecFromTwoCorners(this.GetPosition(), child.GetPosition(), wireThickness / 2);
            if (r.ContainsVector2(position) && Util.DistanceToLine(this.GetPosition(), child.GetPosition(), position) < wireThickness)
            {
                directChild = child;
                return true;
            }
        }

        directChild = null;
        return false;
    }

    public List<WireNode> CollectChildrenRecursively()
    {
        List<WireNode> children = new List<WireNode>();
        foreach (WireNode child in this.Children)
        {
            children.Add(child);
            children.AddRange(child.CollectChildrenRecursively());
        }
        return children;
    }

    public bool TryGetChildWireNodeFromPosition(Vector2 position, [NotNullWhen(true)] out WireNode? node)
    {
        if (this.IsPositionOnAnyDirectChildWire(position, out node))
        {
            return true;
        }

        foreach (WireNode child in this.Children)
        {
            if (child.TryGetChildWireNodeFromPosition(position, out node))
            {
                return true;
            }
        }

        node = null;
        return false;
    }

    public WireNode DisconnectChild(WireNode child)
    {
        this.Children.Remove(child);
        return child;
    }

    public abstract Vector2 GetPosition();

    public abstract void Move(Vector2 delta);

    public bool IsPositionOn(Vector2 position)
    {
        return (this.GetPosition() - position).Length() < 5;
    }
}

public class IOWireNode : WireNode
{
    public IO IO { get; set; }

    public IOWireNode(Wire wire, WireNode? parent, IO io) : base(wire, parent)
    {
        this.IO = io;
    }

    public override Vector2 GetPosition()
    {
        return this.IO.GetPosition();
    }

    public override void Move(Vector2 delta)
    {
        // DO NOTHING
    }
}

public class JunctionWireNode : WireNode
{
    public Vector2 Position { get; set; }

    public JunctionWireNode(Wire wire, WireNode? parent, Vector2 position) : base(wire, parent)
    {
        this.Position = position;
    }

    public override Vector2 GetPosition()
    {
        return this.Position;
    }

    public override void Move(Vector2 delta)
    {
        this.Position += delta;

        // IF ANY DIRECT CHILD IS AN IOWIRENODE, OR ANY PARENT IS ONE, IF MOVED ONTO THE SAME
        // POSITION AS THAT PARENT OR CHILD, MERGE THESE TWO NODES

        if (this.Parent.GetPosition() == this.Position)
        {
            // MERGE WITH PARENT
            this.Parent.Children.Remove(this);

            foreach (IOWireNode child in this.Children)
            {
                child.Parent = this.Parent;
                this.Parent.AddChild(child);
            }
        }

        foreach (WireNode child in this.Children)
        {
            if (child.GetPosition() == this.Position)
            {
                // MERGE WITH CHILD
                child.Parent = this.Parent;
                this.Children.Remove(child);
                this.Parent.AddChild(child);
                this.Parent.Children.Remove(this);

                break;
            }
        }
    }
}

public class Wire
{
    public List<IO> IOs { get; private set; }
    public WireStatus Status { get; set; }
    public WireNode? Root { get; set; }

    public Wire()
    {
        this.IOs = new List<IO>();
        this.Root = null;
    }

    public Wire(IO initialIO)
    {
        this.IOs = new List<IO>();
        this.IOs.Add(initialIO);
        this.Root = new IOWireNode(this, null, initialIO);
        initialIO.Wire = this;
    }

    public int GetWireNodeLength()
    {
        return this.Root!.CollectChildrenRecursively().Count + 1;
    }

    public void ConnectIO(IO io)
    {
        this.IOs.Add(io);
        io.Wire = this;
    }

    public void DisconnectIO(IO io)
    {
        //io.SetValues(Util.NValues(LogicValue.UNKNOWN, io.BitWidth).ToArray());
        io.Wire = null;
        this.IOs.Remove(io);
    }

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

    public void RenderWireNodes(WireNode root)
    {
        Vector2 start = root.GetPosition();

        Color color = this.IOs.Count > 0 ? this.IOs[0].GetColor() : Color.GRAY;

        foreach (WireNode child in root.Children)
        {
            Vector2 end = child.GetPosition();
            Raylib.DrawLineEx(start, end, 5f, Color.BLACK);
            Raylib.DrawLineEx(start, end, 3f, color);
            this.RenderWireNodes(child);
        }

        if (root is JunctionWireNode)
        {
            float rad = 2f;
            Raylib.DrawCircleV(start, rad + 1f, Color.BLACK);
            Raylib.DrawCircleV(start, rad, color);
        }
    }

    public bool TryGetChildWireNodeFromPosition(Vector2 position, [NotNullWhen(true)] out WireNode? node)
    {
        if (this.Root != null)
        {
            if (this.Root.TryGetChildWireNodeFromPosition(position, out node))
            {
                return true;
            }
        }

        node = null;
        return false;
    }

    public bool TryGetJunctionWireNodeFromPosition(Vector2 position, [NotNullWhen(true)] out JunctionWireNode? node)
    {
        List<WireNode> nodes = this.Root.CollectChildrenRecursively();

        foreach (WireNode child in nodes)
        {
            if (child is JunctionWireNode junction)
            {
                if (junction.IsPositionOn(position))
                {
                    node = junction;
                    return true;
                }
            }
        }
        node = null;
        return false;
    }

    public void Render()
    {
        if (this.Root != null)
            this.RenderWireNodes(this.Root);
    }
}