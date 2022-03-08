using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandDeleteWire : Command<Editor>
{
    public Wire wire;

    public CommandDeleteWire(Wire wire)
    {
        this.wire = wire;
    }

    public override void Execute(Editor arg)
    {
        arg.Simulator.RemoveWire(this.wire);
    }

    public override string ToString()
    {
        return $"Deleted wire";
    }

    public override void Undo(Editor arg)
    {
        arg.Simulator.AddWire(this.wire);

        List<IO> ios = this.wire.Root.RecursivelyGetAllConnectedIOs();
        ios.ForEach(io => this.wire.ConnectIO(io));
    }
}