using LogiX.Components;
using QuikGraph;

namespace LogiX.Editor.Commands;

public class CommandConnectIOToWire : Command<Editor>
{
    public IO startIO;
    public Vector2 endPos;
    public Vector2 corner;

    public Vector2 sourcePos;
    public Vector2 targetPos;

    public CommandConnectIOToWire(IO start, Vector2 newJuncPos, Vector2 corner)
    {
        this.startIO = start;
        this.endPos = newJuncPos;
        this.corner = corner;
    }

    public bool IsCornerNeeded()
    {
        return corner.X != this.endPos.X || corner.Y != this.endPos.Y;
    }

    public override void Execute(Editor arg)
    {
        Edge<WireNode> onEdge = Util.GetEdgeFromPos(arg.Simulator, this.endPos, out Wire wire);

        this.sourcePos = onEdge.Source.GetPosition();
        this.targetPos = onEdge.Target.GetPosition();

        WireNode newJunc = wire.CreateJunctionWireNode(this.endPos);
        wire.InsertNodeBetween(onEdge.Source, onEdge.Target, newJunc);

        IOWireNode ioNode = wire.CreateIOWireNode(this.startIO);

        if (IsCornerNeeded())
        {
            // INCLUDE CORNER
            JunctionWireNode cornerNode = wire.CreateJunctionWireNode(this.corner);
            wire.ConnectNodes(newJunc, wire, cornerNode);
            wire.ConnectNodes(cornerNode, wire, ioNode);
        }
        else
        {
            // NO CORNER
            wire.ConnectNodes(newJunc, wire, ioNode);
        }
    }

    public override void Undo(Editor arg)
    {
        WireNode startNode = Util.GetIOWireNodeFromPos(arg.Simulator, this.startIO.GetPosition(), out Wire startWire);
        startWire.RemoveNode(startNode);

        if (this.IsCornerNeeded())
        {
            // INCLUDE CORNER
            WireNode cornerNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.corner, out Wire cornerWire);
            cornerWire.RemoveNode(cornerNode);
        }

        WireNode endNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.endPos, out Wire endWire);

        WireNode oldSource = Util.GetWireNodeFromPos(arg.Simulator, this.sourcePos, out Wire oldSourceWire);
        WireNode oldTarget = Util.GetWireNodeFromPos(arg.Simulator, this.targetPos, out Wire oldTargetWire);

        endWire.RemoveNode(endNode);

        oldSourceWire.ConnectNodes(oldSource, oldTargetWire, oldTarget);
    }
}