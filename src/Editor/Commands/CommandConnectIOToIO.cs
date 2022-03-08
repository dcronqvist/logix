using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandConnectIOToIO : Command<Editor>
{
    public IO startIO;
    public IO endIO;
    public Vector2 corner;

    public Wire wire;
    public WireNode startNode;
    public WireNode endNode;
    public WireNode? cornerNode;

    public CommandConnectIOToIO(IO start, IO end, Vector2 corner)
    {
        this.startIO = start;
        this.endIO = end;
        this.corner = corner;

        this.wire = new Wire();
        this.startNode = new IOWireNode(this.wire, null, this.startIO);
        this.endNode = new IOWireNode(this.wire, null, this.endIO);

        this.cornerNode = this.IsCornerNeeded() ? new JunctionWireNode(this.wire, null, this.corner) : null;
    }

    public bool IsCornerNeeded()
    {
        return corner.X != endNode.GetPosition().X || corner.Y != endNode.GetPosition().Y;
    }

    public override void Execute(Editor arg)
    {
        this.startNode.ConnectTo(this.endNode, out Wire? wireToDelete);

        if (wireToDelete != null)
        {
            arg.Simulator.RemoveWire(wireToDelete);
        }

        if (this.cornerNode != null)
        {
            this.startNode.InsertBetween(this.cornerNode, this.endNode);
        }

        arg.Simulator.AddWire(wire);
    }

    public override void Undo(Editor arg)
    {
        if (this.cornerNode != null)
        {
            // We have a corner node
            this.cornerNode.RemoveOnlyNode(out Wire? wireDel);

            if (wireDel != null)
            {
                arg.Simulator.RemoveWire(wireDel);
            }
        }

        // We dont have a corner node
        this.startNode.DisconnectFrom(this.endNode, out Wire? newWire, out Wire? wireToDelete);

        if (wireToDelete != null)
        {
            arg.Simulator.RemoveWire(wireToDelete);
        }

        if (newWire != null)
        {
            arg.Simulator.AddWire(newWire);
        }
    }
}