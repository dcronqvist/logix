using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CDeleteComponent : Command<Editor>
{
    public Component Component { get; set; }

    public CDeleteComponent(Component comp)
    {
        this.Component = comp;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.RemoveComponent(this.Component);
        });
    }

    public override void Undo(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.AddComponent(this.Component, this.Component.Position);
        });
    }
}