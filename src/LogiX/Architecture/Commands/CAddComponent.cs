using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CAddComponent : Command<Editor>
{
    public ComponentDescription Component { get; set; }
    public Vector2i Position { get; set; }

    public CAddComponent(ComponentDescription comp, Vector2i position)
    {
        this.Component = comp;
        this.Position = position;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var comp = this.Component.CreateComponent();
            comp.Position = this.Position;
            s.AddComponent(comp, this.Position);
            s.ClearSelection();
            s.SelectComponent(comp);
        });
    }
}