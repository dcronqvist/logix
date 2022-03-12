using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandConnectIOToNothing : Command<Editor>
{
    public IO startIO;
    public Vector2 endPos;
    public Vector2 corner;

    public CommandConnectIOToNothing(IO start, Vector2 endPos, Vector2 corner)
    {
        this.startIO = start;
        this.endPos = endPos;
        this.corner = corner;
    }

    public bool IsCornerNeeded()
    {
        return corner.X != endPos.X || corner.Y != endPos.Y;
    }

    public override void Execute(Editor arg)
    {
        Wire wire = new Wire();

        WireNode startNode = wire.CreateIOWireNode(this.startIO);
        WireNode endNode = wire.CreateJunctionWireNode(this.endPos);

        if (IsCornerNeeded())
        {
            // INCLUDE CORNER
            WireNode cornerNode = wire.CreateJunctionWireNode(this.corner);
            wire.AddNode(cornerNode);
            wire.ConnectNodes(startNode, wire, cornerNode);
            wire.ConnectNodes(cornerNode, wire, endNode);
        }
        else
        {
            // NO CORNER
            wire.ConnectNodes(startNode, wire, endNode);
        }

        arg.Simulator.AddWire(wire);
    }

    public override void Undo(Editor arg)
    {
        WireNode startNode = Util.GetIOWireNodeFromPos(arg.Simulator, this.startIO.GetPosition(), out Wire startWire);
        startWire.RemoveNode(startNode);

        WireNode endNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.endPos, out Wire endWire);
        endWire.RemoveNode(endNode);

        if (IsCornerNeeded())
        {
            WireNode cornerNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.corner, out Wire cornerWire);
            cornerWire.RemoveNode(cornerNode);
        }

        arg.Simulator.RemoveWire(startWire);
    }
}