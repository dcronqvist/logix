using LogiX.Components;
using LogiX.SaveSystem;

namespace LogiX.Editor.Commands;

public class CommandDeleteComponent : Command<Editor>
{
    Component c;

    Dictionary<Vector2, IO> changedPositions;

    public CommandDeleteComponent(Component c)
    {
        this.c = c;
    }

    public override void Execute(Editor arg)
    {
        List<Wire> allWires = arg.Simulator.AllWires;
        this.changedPositions = new Dictionary<Vector2, IO>();

        foreach (Wire wire in allWires)
        {

            foreach ((IO io, IOConfig ioc) in this.c.IOs)
            {
                if (wire.IsConnectedTo(io, out IOWireNode? node))
                {
                    wire.ChangeIOWireNodeToJunction(node);
                    this.changedPositions.Add(node.GetPosition(), io);
                }
            }
        }

        arg.Simulator.RemoveComponent(this.c);
    }

    public override string ToString()
    {
        return "Deleted " + c.Text + " component";
    }

    public override void Undo(Editor arg)
    {
        arg.Simulator.AddComponent(this.c);

        foreach (KeyValuePair<Vector2, IO> kvp in changedPositions)
        {
            JunctionWireNode jwn = Util.GetJunctionWireNodeFromPos(arg.Simulator, kvp.Key, out Wire wire);
            wire.ChangeJunctionNodeToIONode(jwn, kvp.Value);
        }
    }
}