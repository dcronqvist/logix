namespace LogiX.Architecture.Commands;

public class CMoveSelection : Command<Editor>
{
    public List<Guid> Components { get; set; }
    public List<(Vector2i, Vector2i)> Segments { get; set; }
    public Vector2i Delta { get; set; }

    public CMoveSelection(List<Guid> components, List<(Vector2i, Vector2i)> segments, Vector2i delta)
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
            s.SelectedComponents = this.Components.Select(c => s.GetComponentFromID(c)).ToList();
            s.SelectedWireSegments = this.Segments.ToList();
            s.PickUpSelection();
            s.CommitMovedPickedUpSelection(this.Delta);

            foreach (var c in s.SelectedComponents)
            {
                c.TriggerSizeRecalculation();
            }
        });
    }
}