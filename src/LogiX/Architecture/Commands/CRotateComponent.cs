using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CRotateComponent : Command<Editor>
{
    public Guid Node { get; set; }
    public int Rotation { get; set; }

    public CRotateComponent(Guid node, int rotation)
    {
        this.Node = node;
        this.Rotation = rotation;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.GetNodeFromID(this.Node).Rotate(this.Rotation);
            s.RecalculateWirePositions();
        }, (e) => { throw e; });
    }

    public override string GetDescription()
    {
        return $"Rotate {this.Node} {this.Rotation} times";
    }
}