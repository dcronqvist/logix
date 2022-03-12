using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandConnectJunctions : Command<Editor>
{
    public Vector2 startJunc;
    public Vector2 endJunc;
    public Vector2 corner;

    public CommandConnectJunctions(Vector2 startJunc, Vector2 endJunc, Vector2 corner)
    {
        this.startJunc = startJunc;
        this.endJunc = endJunc;
        this.corner = corner;
    }

    public bool IsCornerNeeded()
    {
        return corner.X != this.endJunc.X || corner.Y != this.endJunc.Y;
    }

    public override void Execute(Editor arg)
    {
        WireNode startNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.startJunc, out Wire startWire);
        WireNode endNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.endJunc, out Wire endWire);

        if (IsCornerNeeded())
        {
            // INCLUDE CORNER
            JunctionWireNode cornerNode = startWire.CreateJunctionWireNode(this.corner);
            startWire.ConnectNodes(startNode, startWire, cornerNode);
            if (startWire.ConnectNodes(cornerNode, endWire, endNode))
            {
                arg.Simulator.RemoveWire(endWire);
            }
        }
        else
        {
            // NO CORNER
            if (startWire.ConnectNodes(startNode, endWire, endNode))
            {
                arg.Simulator.RemoveWire(endWire);
            }
        }
    }

    public override void Undo(Editor arg)
    {
        WireNode startNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.startJunc, out Wire startWire);
        WireNode endNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.endJunc, out Wire endWire);

        if (IsCornerNeeded())
        {
            // INCLUDED CORNER
            JunctionWireNode cornerNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.corner, out Wire cornerWire);
            if (cornerWire.DisconnectNodes(cornerNode, endNode, out Wire? newWire))
            {
                arg.Simulator.AddWire(newWire);
            }
            cornerWire.RemoveNode(cornerNode);
        }
        else
        {
            // NO CORNER
            if (startWire.DisconnectNodes(startNode, endNode, out Wire? newWire))
            {
                arg.Simulator.AddWire(newWire);
            }
        }
    }
}