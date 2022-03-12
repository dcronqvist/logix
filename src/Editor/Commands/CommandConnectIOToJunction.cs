using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandConnectIOToJunction : Command<Editor>
{
    public IO startIO;
    public Vector2 junctionPosition;
    public Vector2 corner;

    public CommandConnectIOToJunction(IO start, Vector2 junctionPosition, Vector2 corner)
    {
        this.startIO = start;
        this.junctionPosition = junctionPosition;
        this.corner = corner;
    }

    public bool IsCornerNeeded()
    {
        return corner.X != this.junctionPosition.X || corner.Y != this.junctionPosition.Y;
    }

    public override void Execute(Editor arg)
    {
        WireNode junctionNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.junctionPosition, out Wire wire);
        WireNode startNode = wire.CreateIOWireNode(this.startIO);

        WireNode? cornerNode = this.IsCornerNeeded() ? wire.CreateJunctionWireNode(this.corner) : null;

        wire.AddNode(startNode);

        if (cornerNode != null)
        {
            // INCLUDE CORNER
            wire.AddNode(cornerNode);
            wire.ConnectNodes(startNode, wire, cornerNode);
            wire.ConnectNodes(cornerNode, wire, junctionNode);
        }
        else
        {
            // NO CORNER
            wire.ConnectNodes(startNode, wire, junctionNode);
        }
    }

    public override void Undo(Editor arg)
    {
        WireNode junctionNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.junctionPosition, out Wire wire);

        if (this.IsCornerNeeded())
        {
            // CORNER IS INCLUDED
            WireNode corner = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.corner, out Wire cornerWire);
            cornerWire.RemoveNode(corner);
        }

        // REMOVE START NODE IOWIRENODE
        IOWireNode startNode = Util.GetIOWireNodeFromPos(arg.Simulator, this.startIO.GetPosition(), out Wire startWire);
        startWire.RemoveNode(startNode);
    }
}