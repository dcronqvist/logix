using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CAddComponent : Command<EditorTab>
{
    public Component Component { get; set; }
    public Vector2i Position { get; set; }

    public CAddComponent(Component comp, Vector2i position)
    {
        this.Component = comp;
        this.Position = position;
    }

    public override void Execute(EditorTab arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.AddComponent(this.Component, this.Position);
            s.ClearSelection();
            s.SelectComponent(this.Component);
        });
    }

    public override void Undo(EditorTab arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.RemoveComponent(this.Component);
        });
    }
}