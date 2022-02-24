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
    public List<WireNode> Next { get; set; }

    public WireNode()
    {
        this.Next = new List<WireNode>();
    }

    public abstract Vector2 GetPosition();
    public abstract bool CanBeMoved();

    private WireNode AddWireNode(WireNode wn)
    {
        this.Next.Add(wn);
        return wn;
    }


    public IOWireNode AddIONode(IO io)
    {
        IOWireNode iown = new IOWireNode(io);
        this.AddWireNode(iown);
        return iown;
    }

    public FreeWireNode InsertFreeNode(Vector2 position, WireNode between)
    {
        FreeWireNode fwn = new FreeWireNode(position);
        this.Next.Add(fwn);
        this.Next.Remove(between);

        //fwn.Next = between.Next.Copy();
        fwn.Next.Add(between);
        return fwn;
    }

    public List<WireNode> RemoveNode(WireNode node, out List<IO> iosToRemove)
    {
        List<WireNode> allRemovedNodes = new List<WireNode>();
        iosToRemove = new List<IO>();

        allRemovedNodes.Add(node);

        if (node is IOWireNode)
        {
            IOWireNode iw = (IOWireNode)node;
            iosToRemove.Add(iw.IO);
        }

        foreach (WireNode n in node.Next.Copy())
        {
            allRemovedNodes.AddRange(node.RemoveNode(n, out List<IO> ios));
            iosToRemove.AddRange(ios);
        }

        this.Next.Remove(node);
        return allRemovedNodes;
    }

    public FreeWireNode AddFreeNode(Vector2 position)
    {
        FreeWireNode fwn = new FreeWireNode(position);
        this.AddWireNode(fwn);
        return fwn;
    }

    public bool RemoveIONode(IO io)
    {
        foreach (WireNode wn in this.Next)
        {
            if (wn is IOWireNode)
            {
                IOWireNode iown = (IOWireNode)wn;
                if (iown.IO == io)
                {
                    this.Next.Remove(wn);
                    return true;
                }
            }
            if (wn.RemoveIONode(io))
            {
                return true;
            }
        }

        return false;
    }

    public abstract void RenderSelected();
    public abstract void Move(Vector2 delta);
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
        return this.IO.OnComponent.GetIOPosition(this.IO); // TODO: Implement stuff to get position of IO
    }

    public override bool CanBeMoved()
    {
        return false;
    }

    public override void RenderSelected()
    {
        // DO NOTHING
    }

    public override void Move(Vector2 delta)
    {
        // DO NOTHING
        return;
    }
}

public class FreeWireNode : WireNode
{
    public Vector2 Position { get; set; }

    public FreeWireNode(Vector2 position)
    {
        this.Position = position;
    }

    public override Vector2 GetPosition()
    {
        return this.Position;
    }

    public override bool CanBeMoved()
    {
        return true;
    }

    public override void RenderSelected()
    {
        Raylib.DrawCircleV(this.GetPosition(), 8f, Color.ORANGE);
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
    public WireNode RootWireNode { get; set; }

    public Wire(IO initialIO)
    {
        this.IOs = new List<IO>();
        this.IOs.Add(initialIO);
        this.RootWireNode = new IOWireNode(initialIO);
        initialIO.Wire = this;
    }

    public void ConnectIO(IO io)
    {
        this.IOs.Add(io);
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

    public List<WireNode> GetAllWireNodes(WireNode root)
    {
        List<WireNode> nodes = new List<WireNode>();
        if (!nodes.Contains(root))
            nodes.Add(root);
        foreach (WireNode wn in root.Next)
        {
            nodes.AddRange(this.GetAllWireNodes(wn));
        }
        return nodes;
    }

    public List<WireNode> GetAllWireNodes()
    {
        return this.GetAllWireNodes(this.RootWireNode);
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
        float wireThickness = 5f;
        //float wireNodeRadius = 10f;

        Color color = this.IOs[0].GetColor();

        Vector2 pos = root.GetPosition();


        foreach (WireNode wn in root.Next)
        {
            Vector2 otherPos = wn.GetPosition();

            Raylib.DrawLineEx(pos, otherPos, wireThickness + 2f, Color.BLACK);
            Raylib.DrawLineEx(pos, otherPos, wireThickness, color);

            Raylib.DrawLineV(pos, otherPos, color);
            this.RenderWireNodes(wn);
        }
        if (root is FreeWireNode)
        {
            Raylib.DrawCircleV(pos, 6f, Color.BLACK);
            Raylib.DrawCircleV(pos, 5, color);
        }
    }

    private bool TryGetWireNode(WireNode root, Vector2 position, [NotNullWhen(true)] out WireNode? wireNodeFrom, [NotNullWhen(true)] out WireNode? wireNodeTo)
    {
        float wireThickness = 5f;

        foreach (WireNode wn in root.Next)
        {
            Vector2 start = root.GetPosition();
            Vector2 end = wn.GetPosition();

            Rectangle rec = Util.CreateRecFromTwoCorners(start.Vector2Towards(5, end), end.Vector2Towards(5, start), wireThickness / 2);
            if (Raylib.CheckCollisionPointRec(position, rec))
            {
                float distance = Util.DistanceToLine(start, end, position);
                if (distance <= wireThickness / 2)
                {
                    wireNodeFrom = root;
                    wireNodeTo = wn;
                    return true;
                }
            }

            if (this.TryGetWireNode(wn, position, out wireNodeFrom, out wireNodeTo))
            {
                return true;
            }
        }

        wireNodeFrom = null;
        wireNodeTo = null;
        return false;
    }

    private bool TryGetFreeWireNodeFromPosition(WireNode root, Vector2 position, out WireNode? from, out WireNode? wireNode)
    {
        foreach (WireNode wn in root.Next)
        {
            if ((wn.GetPosition() - position).Length() < 5)
            {
                wireNode = wn;
                from = root;
                return true;
            }
            if (this.TryGetFreeWireNodeFromPosition(wn, position, out from, out wireNode))
            {
                return true;
            }
        }
        from = null;
        wireNode = null;
        return false;
    }

    public bool TryGetFreeWireNodeFromPosition(Vector2 position, out WireNode? from, out WireNode? wireNode)
    {
        return this.TryGetFreeWireNodeFromPosition(this.RootWireNode, position, out from, out wireNode);
    }

    public bool TryGetWireNode(Vector2 position, [NotNullWhen(true)] out WireNode? wireNodeFrom, [NotNullWhen(true)] out WireNode? wireNodeTo)
    {
        return this.TryGetWireNode(this.RootWireNode, position, out wireNodeFrom, out wireNodeTo);
    }

    public void Render()
    {
        this.RenderWireNodes(this.RootWireNode);
    }
}