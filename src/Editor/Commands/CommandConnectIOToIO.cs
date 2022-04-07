using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandConnectIOToIO : Command<Editor>
{
    public IO startIO;
    public IO endIO;
    public Vector2 corner;

    public CommandConnectIOToIO(IO start, IO end, Vector2 corner)
    {
        this.startIO = start;
        this.endIO = end;
        this.corner = corner;
    }

    public bool IsCornerNeeded()
    {
        return corner.X != endIO.GetPosition().X || corner.Y != endIO.GetPosition().Y;
    }

    public override void Execute(Editor arg)
    {
        Wire newWire = new Wire();
        WireNode startNode = newWire.CreateIOWireNode(this.startIO);
        WireNode endNode = newWire.CreateIOWireNode(this.endIO);

        if (IsCornerNeeded())
        {
            WireNode cornerNode = newWire.CreateJunctionWireNode(this.corner);
            newWire.AddNode(cornerNode);
            newWire.ConnectNodes(startNode, newWire, cornerNode);
            newWire.ConnectNodes(cornerNode, newWire, endNode);
        }
        else
        {
            newWire.ConnectNodes(startNode, newWire, endNode);
        }

        arg.Simulator.AddWire(newWire);
    }

    public override void Undo(Editor arg)
    {
        WireNode startNode = Util.GetIOWireNodeFromPos(arg.Simulator, this.startIO.GetPosition(), out Wire startWire);
        startWire.RemoveNode(startNode);

        WireNode endNode = Util.GetIOWireNodeFromPos(arg.Simulator, this.endIO.GetPosition(), out Wire endWire);
        endWire.RemoveNode(endNode);

        if (IsCornerNeeded())
        {
            WireNode cornerNode = Util.GetJunctionWireNodeFromPos(arg.Simulator, this.corner, out Wire cornerWire);
            cornerWire.RemoveNode(cornerNode);
        }

        arg.Simulator.RemoveWire(startWire);
    }
}