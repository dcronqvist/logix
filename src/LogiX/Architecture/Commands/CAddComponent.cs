using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CAddComponent : Command<Editor>
{
    public NodeDescription Node { get; set; }
    public Vector2i Position { get; set; }

    public CAddComponent(NodeDescription node, Vector2i position)
    {
        this.Node = node;
        this.Position = position;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var n = this.Node.CreateNode();
            n.Position = this.Position;
            s.AddNode(n);
            s.ClearSelection();
            s.SelectNode(n);
        }, (e) => { throw e; });
    }

    public override string GetDescription()
    {
        return $"Add {this.Node.NodeTypeID} at {this.Position.X},{this.Position.Y}";
    }
}