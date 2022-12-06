using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CDeleteNode : Command<Editor>
{
    public Guid Node { get; set; }

    public CDeleteNode(Guid node)
    {
        this.Node = node;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var comp = s.GetNodeFromID(this.Node);
            s.RemoveNode(comp);
        }, (e) => { throw e; });
    }

    public override string GetDescription()
    {
        return $"Delete {this.Node}";
    }
}