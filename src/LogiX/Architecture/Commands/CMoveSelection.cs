namespace LogiX.Architecture.Commands;

public class CMoveSelection : Command<Editor>
{
    public List<Component> Components { get; set; }
    public List<(Vector2i, Vector2i)> Segments { get; set; }
    public List<(Vector2i, Vector2i)> DestSegments => this.Segments.Select(s => (s.Item1 + this.Delta, s.Item2 + this.Delta)).ToList();
    public Vector2i Delta { get; set; }

    public CMoveSelection(List<Component> components, List<(Vector2i, Vector2i)> segments, Vector2i delta)
    {
        this.Components = components.ToList();
        this.Segments = segments.ToList();
        this.Delta = delta;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.ClearSelection();
            s.SelectedComponents = this.Components.ToList();
            s.SelectedWireSegments = this.Segments.ToList();
            s.PickUpSelection();
            s.CommitMovedPickedUpSelection(this.Delta);
        });
    }

    public override void Undo(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.ClearSelection();
            s.SelectedComponents = this.Components.ToList();
            s.SelectedWireSegments = this.DestSegments.ToList();
            s.PickUpSelection();
            s.CommitMovedPickedUpSelection(-this.Delta);
        });
    }
}