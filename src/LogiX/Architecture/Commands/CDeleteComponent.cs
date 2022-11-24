using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CDeleteComponent : Command<Editor>
{
    public Guid Component { get; set; }

    public CDeleteComponent(Guid comp)
    {
        this.Component = comp;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var comp = s.GetComponentFromID(this.Component);
            s.RemoveComponent(comp);
        });
    }

    public override string GetDescription()
    {
        return $"Delete {this.Component}";
    }
}