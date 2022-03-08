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

    public List<int> GetLocationDescriptor()
    {
        if (this.Parent == null)
        {
            return Util.EmptyList<int>();
        }

        int index = this.Parent.Children.IndexOf(this);
        return this.Parent.GetLocationDescriptor().Append(index);
    }

    public List<IO> RecursivelyGetAllConnectedIOs()
    {
        List<IO> connectedIOs = new List<IO>();

        foreach (WireNode child in this.Children)
        {
            connectedIOs.AddRange(child.RecursivelyGetAllConnectedIOs());
        }

        if (this is IOWireNode ioWireNode)
        {
            connectedIOs.Add(ioWireNode.IO);
        }

        return connectedIOs;
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
        child.Parent = this;
    }

    public IOWireNode AddIOWireNode(IO io)
    {
        IOWireNode iwn = new IOWireNode(this.Wire, this, io);
        this.AddChild(iwn);
        return iwn;
    }

    public void RemoveIOWireNode(IO io)
    {
        foreach (WireNode child in this.Children)
        {
            if (child is IOWireNode iowire && iowire.IO == io)
            {
                this.Children.Remove(child);
                return;
            }
        }
    }

    public JunctionWireNode AddJunctionWireNode(Vector2 position)
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
        children.Add(this);
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

    public WireNode RecursivelyGetParent()
    {
        if (this.Parent == null)
        {
            return this;
        }
        return this.Parent.RecursivelyGetParent();
    }

    public WireNode Split(out WireNode splitFrom)
    {
        this.Parent.Children.Remove(this);
        WireNode parent = this.RecursivelyGetParent();
        splitFrom = this.Parent;
        this.Parent = null;
        return parent;
    }

    public virtual void RecursivelySetWire(Wire wire)
    {
        this.Wire = wire;
        foreach (WireNode child in this.Children)
        {
            child.RecursivelySetWire(wire);
        }
    }

    private WireNode MakeRoot(List<WireNode> visited)
    {
        visited = visited.Append(this);

        WireNode newRoot = this;
        List<WireNode> allChildren = this.Children.ToList();
        if (this.Parent != null)
            allChildren.Add(this.Parent);

        allChildren = allChildren.Except(visited).ToList();

        newRoot.Parent = null;

        newRoot.Children.Clear();

        if (allChildren.Count == 0)
        {
            return this;
        }

        foreach (WireNode child in allChildren)
        {
            WireNode childRoot = child.MakeRoot(visited);
            newRoot.AddChild(childRoot);
        }

        return newRoot;
    }

    public WireNode MakeRoot()
    {
        WireNode root = this.MakeRoot(Util.EmptyList<WireNode>());
        this.Wire.Root = root;
        return root;
    }

    public void InsertBetween(WireNode insert, WireNode child)
    {
        this.Children.Remove(child);
        this.AddChild(insert);
        insert.AddChild(child);
    }

    public WireNode RemoveOnlyNode(out Wire? wireToDelete)
    {
        if (this.Parent == null)
        {
            // I AM ROOT NODE, MUST MAKE SOMEONE ELSE ROOT NODE
            this.Children.First().MakeRoot();
        }
        wireToDelete = null;
        // I AM CHILD NODE, REMOVE ME FROM PARENT AND ADD MY CHILDREN AND CLEAR MY CHILDREN

        List<WireNode> children = this.Children;
        this.Parent.Children.Remove(this);
        children.ForEach(c => this.Parent.AddChild(c));
        this.Children.Clear();

        WireNode newRoot = this.Parent.RecursivelyGetParent();
        if (newRoot.Children.Count == 0)
        {
            wireToDelete = newRoot.Wire;
        }

        this.Wire.Root = newRoot;

        return newRoot;
    }

    public WireNode ConnectTo(WireNode other, out Wire? wireToBeDeleted)
    {
        // 1. THEY ARE ON THE SAME WIRE
        //  1.1 NONE OF US ARE ROOT
        //      We must simply add other as a child to ourselves.    
        //      This should not be allowed, since it would overwrite the parent
        //      of one of the nodes, potentially destroying the tree structure.  
        //
        //  1.2 I AM ROOT, OTHER IS NOT
        //      We must simply add other as a child to ourselves
        //
        //  1.3 I AM NOT ROOT, OTHER IS ROOT
        //      We simply add other as child to ourselves.
        //
        // 2. THEY ARE NOT ON THE SAME WIRE
        //  2.1 NONE OF US ARE ROOT
        //      We must make ourselves root, then make other root, then simply add other as a child to ourselves.
        //      Then we must make sure that all of our children are connected to the same wire as us.
        //
        //  2.2 I AM ROOT, OTHER IS NOT
        //      We must make other root, and then add other as a child to ourselves.
        //      Then we must make sure that all of our children are connected to the same wire as us.
        //
        //  2.3 I AM NOT ROOT, OTHER IS ROOT
        //      Add ourselves to other as child.
        //      Then we must make sure that all children of other are connected to the same wire.
        //
        //  2.4 BOTH ARE ROOT
        //      Simply add other as a child to ourselves.
        //      Then we must make sure that all of our children are connected to the same wire as us.
        WireNode newRoot = null;
        wireToBeDeleted = null;

        if (this.Wire == other.Wire)
        {
            if (this.Parent != null && other.Parent != null)
            {
                // 1.1 NONE OF US ARE ROOT
                newRoot = this.ConnectToSameWireNoneRoot(other);
            }
            else if (this.Parent == null && other.Parent != null)
            {
                // 1.2 I AM ROOT, OTHER IS NOT
                newRoot = this.ConnectToSameWireThisRoot(other);
            }
            else if (this.Parent != null && other.Parent == null)
            {
                // 1.3 I AM NOT ROOT, OTHER IS ROOT
                newRoot = this.ConnectToSameWireOtherRoot(other);
            }
            else if (this.Parent == null && other.Parent == null)
            {
                // 1.4 BOTH ARE ROOT
                newRoot = this.ConnectToSameWireBothRoot(other);
            }
        }
        else
        {
            if (this.Parent != null && other.Parent != null)
            {
                // 2.1 NONE OF US ARE ROOT
                newRoot = this.ConnectToDiffWireNoneRoot(other, out wireToBeDeleted);
            }
            else if (this.Parent == null && other.Parent != null)
            {
                // 2.2 I AM ROOT, OTHER IS NOT
                newRoot = this.ConnectToDiffWireThisRoot(other, out wireToBeDeleted);
            }
            else if (this.Parent != null && other.Parent == null)
            {
                // 2.3 I AM NOT ROOT, OTHER IS ROOT
                newRoot = this.ConnectToDiffWireOtherRoot(other, out wireToBeDeleted);
            }
            else if (this.Parent == null && other.Parent == null)
            {
                // 2.4 BOTH ARE ROOT
                newRoot = this.ConnectToDiffWireBothRoot(other, out wireToBeDeleted);
            }
        }

        newRoot.Wire.Root = newRoot;
        newRoot.Wire.Root.RecursivelySetWire(newRoot.Wire);
        return newRoot;
    }

    public (WireNode, WireNode?) DisconnectFrom(WireNode child, out Wire? newWire, out Wire? wireToDelete)
    {
        // THEY MUST ALWAYS BE ON THE SAME WIRE FOR A DISCONNECTION TO EVER OCCUR
        // 1. Child is an IOWireNode.
        //    We must remove it from our children and disconnect the IO from the Wire and not create a new wire.
        newWire = null;
        wireToDelete = null;

        WireNode newRootFirst = null;
        WireNode? newRootSecond = null;

        if (child is IOWireNode ioWire)
        {
            (newRootFirst, newRootSecond) = this.DisconnectFromChildIO(ioWire);
        }
        else if (this is IOWireNode)
        {
            (newRootFirst, newRootSecond) = this.DisconnectFromThisIO(child);
        }
        else if (child.Children.Count == 0)
        {
            (newRootFirst, newRootSecond) = this.DisconnectFromNoChildren(child);
        }
        else
        {
            (newRootFirst, newRootSecond) = this.DisconnectFromDefault(child, out newWire);
        }

        // 2. This is an IOWireNode
        //    We must make child root and disconnect the IO from the wire.

        // 3. Child has no children, so it is a leaf node.
        //    We simply remove it from our children.

        if (newRootFirst.Children.Count == 0 && newRootFirst.Parent == null)
        {
            // AFTER DISCONNECT, NEWROOT NO LONGER HAS CHILDREN AND IS A ROOT NODE. SHOULD BE DELETED
            wireToDelete = newRootFirst.Wire;
        }
        if (newRootSecond != null && newRootSecond.Children.Count == 0 && newRootSecond.Parent == null)
        {
            // AFTER DISCONNECT, NEWROOT NO LONGER HAS CHILDREN AND IS A ROOT NODE. SHOULD BE DELETED
            wireToDelete = newRootSecond.Wire;
        }

        return (newRootFirst, newRootSecond);

        // newWire = null;
        // return null;
    }

    private WireNode ConnectToSameWireNoneRoot(WireNode other)
    {
        this.AddChild(other);
        return this.RecursivelyGetParent();
    }

    private WireNode ConnectToSameWireThisRoot(WireNode other)
    {
        this.AddChild(other);
        this.MakeRoot();
        return this;
    }

    private WireNode ConnectToSameWireOtherRoot(WireNode other)
    {
        this.AddChild(other);
        return this.RecursivelyGetParent();
    }

    private WireNode ConnectToSameWireBothRoot(WireNode other)
    {
        this.AddChild(other);
        this.MakeRoot();
        return this;
    }

    private WireNode ConnectToDiffWireNoneRoot(WireNode other, out Wire wireToDelete)
    {
        other.MakeRoot();
        this.AddChild(other);
        this.MakeRoot();

        // Since we are getting our own parent recursively, we are deleting the wire
        // of the other wire node
        wireToDelete = other.Wire;
        return this;
    }

    private WireNode ConnectToDiffWireThisRoot(WireNode other, out Wire wireToDelete)
    {
        other.MakeRoot();
        this.AddChild(other);

        // Once again, we are deleting the wire of the other wire node
        wireToDelete = other.Wire;
        return this;
    }

    private WireNode ConnectToDiffWireOtherRoot(WireNode other, out Wire wireToDelete)
    {
        this.AddChild(other);

        // Once again
        wireToDelete = other.Wire;
        return this.RecursivelyGetParent();
    }

    private WireNode ConnectToDiffWireBothRoot(WireNode other, out Wire wireToDelete)
    {
        this.AddChild(other);
        this.MakeRoot();

        // Once again
        wireToDelete = other.Wire;
        return this;
    }

    private (WireNode, WireNode?) DisconnectFromDefault(WireNode other, out Wire newWire)
    {
        // Remove child from ourselves
        this.Children.Remove(other);
        this.MakeRoot();

        // Make child root
        other.Parent = null;
        other.MakeRoot();

        // Other will keep the same wire, but this node will have a new wire
        newWire = new Wire();

        // Remove all IOs on this part of the wire from the other wire
        this.RecursivelyGetAllConnectedIOs().ForEach(io => other.Wire.DisconnectIO(io));

        this.RecursivelySetWire(newWire);
        newWire.Root = this;

        return (this, other);
    }

    private (WireNode, WireNode?) DisconnectFromChildIO(IOWireNode other)
    {
        this.Children.Remove(other);
        this.Wire.DisconnectIO(other.IO);
        //other.Parent = null;

        return (this, other);
    }

    private (WireNode, WireNode?) DisconnectFromThisIO(WireNode other)
    {
        other.MakeRoot();
        return other.DisconnectFromChildIO(this as IOWireNode);
    }

    private (WireNode, WireNode?) DisconnectFromNoChildren(WireNode other)
    {
        this.Children.Remove(other);
        return (this, other);
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

    public override void RecursivelySetWire(Wire wire)
    {
        this.Wire = wire;
        this.IO.Wire = wire;

        if (!this.Wire.IOs.Contains(this.IO))
            this.Wire.ConnectIO(this.IO);

        foreach (WireNode child in this.Children)
        {
            child.RecursivelySetWire(wire);
        }
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

        // if (this.Parent != null)
        // {
        //     if (this.Parent.GetPosition() == this.Position)
        //     {
        //         // MERGE WITH PARENT
        //         this.Parent.Children.Remove(this);

        //         foreach (IOWireNode child in this.Children)
        //         {
        //             child.Parent = this.Parent;
        //             this.Parent.AddChild(child);
        //         }
        //     }

        //     foreach (WireNode child in this.Children)
        //     {
        //         if (child.GetPosition() == this.Position)
        //         {
        //             // MERGE WITH CHILD
        //             child.Parent = this.Parent;
        //             this.Children.Remove(child);
        //             this.Parent.AddChild(child);
        //             this.Parent.Children.Remove(this);

        //             break;
        //         }
        //     }
        // }
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

    public WireNode GetWireNodeByDescriptor(List<int> descriptor)
    {
        WireNode? node = this.Root;

        foreach (int i in descriptor)
        {
            node = node.Children[i];
        }

        return node;
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

    public List<WireNode> GetAllWireNodes()
    {
        return this.Root!.CollectChildrenRecursively();
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

    public void RenderWireNodes(WireNode root)
    {
        Vector2 start = root.GetPosition();

        Color color = this.IOs.Count > 0 ? this.IOs[0].GetColor() : Color.GRAY;

        if (root.Parent == null)
        {
            // THIS IS A ROOT NODE
            Raylib.DrawCircleV(start, 5f, Color.PURPLE);
        }
        foreach (WireNode child in root.Children)
        {
            Vector2 end = child.GetPosition();
            Raylib.DrawLineEx(start, end, 5f, Color.BLACK);
            Raylib.DrawLineEx(start, end, 3f, color);
            Raylib.DrawCircleV(start.Vector2Towards(5, end), 3f, Color.GREEN);
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