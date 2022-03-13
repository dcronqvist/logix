using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandConnectJunctionToNothing : Command<Editor>
{
    public Vector2 startPos;
    public Vector2 endPos;
    public Vector2 corner;

    public CommandConnectJunctionToNothing(Vector2 startPos, Vector2 endPos, Vector2 corner)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        this.corner = corner;
    }

    public bool IsCornerNeeded()
    {
        return corner.X != endPos.X || corner.Y != endPos.Y;
    }

    public override void Execute(Editor arg)
    {
        WireNode startNode = Util.GetWireNodeFromPos(arg.Simulator, this.startPos, out Wire startWire);
        WireNode endNode = startWire.CreateJunctionWireNode(this.endPos);

        if (IsCornerNeeded())
        {
            // INCLUDE CORNER
            WireNode cornerNode = startWire.CreateJunctionWireNode(this.corner);
            startWire.ConnectNodes(startNode, startWire, cornerNode);
            startWire.ConnectNodes(cornerNode, startWire, endNode);
        }
        else
        {
            // NO CORNER
            startWire.ConnectNodes(startNode, startWire, endNode);
        }
    }

    public override void Undo(Editor arg)
    {
        WireNode startNode = Util.GetWireNodeFromPos(arg.Simulator, this.startPos, out Wire startWire);
        WireNode endNode = Util.GetWireNodeFromPos(arg.Simulator, this.endPos, out Wire endWire);
        endWire.RemoveNode(endNode);

        if (IsCornerNeeded())
        {
            WireNode cornerNode = Util.GetWireNodeFromPos(arg.Simulator, this.corner, out Wire cornerWire);
            cornerWire.RemoveNode(cornerNode);
        }
    }
}