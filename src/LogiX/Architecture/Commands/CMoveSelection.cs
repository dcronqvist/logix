namespace LogiX.Architecture.Commands;

public class CMoveSelection : Command<EditorTab>
{
    public List<Component> Components { get; set; }
    public Vector2i Delta { get; set; }

    public CMoveSelection(List<Component> components, Vector2i delta)
    {
        this.Components = components.ToList();
        this.Delta = delta;
    }

    public override void Execute(EditorTab arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.ClearSelection();

            foreach (var comp in this.Components)
            {
                s.SelectComponent(comp);
            }

            s.MoveSelection(Delta);
        });
    }

    public override void Undo(EditorTab arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.ClearSelection();

            foreach (var comp in this.Components)
            {
                s.SelectComponent(comp);
            }

            s.MoveSelection(-Delta);
        });
    }
}