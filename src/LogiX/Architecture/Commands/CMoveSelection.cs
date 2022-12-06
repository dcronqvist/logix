namespace LogiX.Architecture.Commands;

public class CMoveSelection : Command<Editor>
{
    public List<Guid> Nodes { get; set; }
    public Vector2i Delta { get; set; }

    public CMoveSelection(List<Guid> nodes, Vector2i delta)
    {
        this.Nodes = nodes.ToList();
        this.Delta = delta;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.ClearSelection();
            s.SelectedNodes = this.Nodes.Select(c => s.GetNodeFromID(c)).ToList();
            s.PickUpSelection();
            s.CommitMovedPickedUpSelection(this.Delta);
        }, (e) => { throw e; });
    }

    public override string GetDescription()
    {
        return $"Move {this.Nodes.Count} nodes by {this.Delta.X},{this.Delta.Y}";
    }
}