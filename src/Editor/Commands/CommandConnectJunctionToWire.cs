using LogiX.Components;
using QuikGraph;

namespace LogiX.Editor.Commands;

public class CommandConnectJunctionToWire : Command<Editor>
{
    public Vector2 startPos;
    public Vector2 endPos;
    public Vector2 corner;

    public Vector2 sourcePos;
    public Vector2 targetPos;

    public CommandConnectJunctionToWire(Vector2 startPos, Vector2 newJuncPos, Vector2 corner)
    {
        this.startPos = startPos;
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

        WireNode startNode = Util.GetWireNodeFromPos(arg.Simulator, this.startPos, out Wire startWire);

        if (IsCornerNeeded())
        {
            // INCLUDE CORNER
            JunctionWireNode cornerNode = wire.CreateJunctionWireNode(this.corner);
            wire.ConnectNodes(newJunc, wire, cornerNode);
            if (wire.ConnectNodes(cornerNode, startWire, startNode))
            {
                arg.Simulator.RemoveWire(startWire);
            }
        }
        else
        {
            // NO CORNER
            if (wire.ConnectNodes(newJunc, startWire, startNode))
            {
                arg.Simulator.RemoveWire(startWire);
            }
        }
    }

    public override void Undo(Editor arg)
    {
        if (this.IsCornerNeeded())
        {
            // INCLUDED CORNER
            // DISCONNECT CORNER FROM START NODE
            WireNode cornerNode = Util.GetWireNodeFromPos(arg.Simulator, this.corner, out Wire cornerWire);
            WireNode startNode = Util.GetWireNodeFromPos(arg.Simulator, this.startPos, out Wire startWire);

            if (cornerWire.DisconnectNodes(cornerNode, startNode, out Wire? newWire))
            {
                arg.Simulator.AddWire(newWire);
            }

            // DELETE CORNER NODE
            cornerNode = Util.GetWireNodeFromPos(arg.Simulator, this.corner, out cornerWire);
            cornerWire.RemoveNode(cornerNode);
        }
        else
        {
            // NO CORNER
            // DISCONNECT END FROM START NODE
            WireNode startNode = Util.GetWireNodeFromPos(arg.Simulator, this.startPos, out Wire startWire);
            WireNode end = Util.GetWireNodeFromPos(arg.Simulator, this.endPos, out Wire endW);

            if (startWire.DisconnectNodes(startNode, end, out Wire? newWire))
            {
                arg.Simulator.AddWire(newWire);
            }
        }

        // DELETE END NODE
        WireNode endNode = Util.GetWireNodeFromPos(arg.Simulator, this.endPos, out Wire endWire);

        WireNode oldSource = Util.GetWireNodeFromPos(arg.Simulator, this.sourcePos, out Wire oldSourceWire);
        WireNode oldTarget = Util.GetWireNodeFromPos(arg.Simulator, this.targetPos, out Wire oldTargetWire);

        endWire.RemoveNode(endNode);

        oldSourceWire.ConnectNodes(oldSource, oldTargetWire, oldTarget);
    }
}