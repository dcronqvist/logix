using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CAddComponent : Command<EditorTab>
{
    public ComponentDescription Component { get; set; }

    private Component _addedComponent;

    public CAddComponent(ComponentDescription description)
    {
        this.Component = description;
    }

    public override void Execute(EditorTab arg)
    {
        arg.Sim.LockedAction(s =>
        {
            _addedComponent = this.Component.CreateComponent();
            s.AddComponent(_addedComponent, this.Component.Position);
        });
    }

    public override void Undo(EditorTab arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.RemoveComponent(_addedComponent);
        });
    }
}