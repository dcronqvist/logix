using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandDeleteWire : Command<Editor>
{
    public Vector2 wireAtPosition;

    public Wire deletedWire;

    public CommandDeleteWire(Vector2 wireAtPosition)
    {
        this.wireAtPosition = wireAtPosition;
    }

    public override void Execute(Editor arg)
    {
        Wire wire = Util.GetWireFromPos(arg.Simulator, wireAtPosition);
        this.deletedWire = wire;
        wire.DisconnectAllIOs();
        arg.Simulator.RemoveWire(wire);
    }

    public override string ToString()
    {
        return $"Deleted wire";
    }

    public override void Undo(Editor arg)
    {
        arg.Simulator.AddWire(deletedWire);
        deletedWire.UpdateIOs();
    }
}