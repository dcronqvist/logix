using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandDeleteWireSegment : Command<Editor>
{
    public Wire wire;
    public WireNode rootBeforeDelete;

    public Wire newWire;
    public Wire wireToDelete;

    public WireNode child;
    public WireNode parent;

    public WireNode undoChild;
    public WireNode undoParent;

    public CommandDeleteWireSegment(WireNode childToDelete)
    {
        this.wire = childToDelete.Wire;
        this.child = childToDelete;
        this.rootBeforeDelete = wire.Root!;
        this.parent = childToDelete.Parent;
    }

    public override void Execute(Editor arg)
    {
        (this.undoParent, this.undoChild) = parent.DisconnectFrom(child, out this.newWire, out this.wireToDelete);

        if (newWire != null)
        {
            arg.Simulator.AddWire(newWire);
        }

        if (wireToDelete != null)
        {
            arg.Simulator.RemoveWire(wireToDelete);
        }
    }

    public override string ToString()
    {
        return $"Deleted wire segment";
    }

    public override void Undo(Editor arg)
    {
        if (this.wireToDelete != null)
        {
            arg.Simulator.AddWire(wireToDelete);
        }

        if (this.newWire != null)
        {
            arg.Simulator.RemoveWire(wire);
        }

        this.undoParent.ConnectTo(this.undoChild, out Wire? wireDelete);

        this.rootBeforeDelete.MakeRoot();

        if (wireDelete != null)
        {
            arg.Simulator.RemoveWire(wireDelete);
        }
    }
}