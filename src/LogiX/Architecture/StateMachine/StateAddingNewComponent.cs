using System.Numerics;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.GLFW;

namespace LogiX.Architecture.StateMachine;

public class StateAddingNewComponent : State<Editor, int>
{
    private ComponentDescription description;

    public override void OnEnter(Editor updateArg, int arg)
    {
        this.description = updateArg.NewComponent;
    }

    public override void Update(Editor arg)
    {
        var currentMouse = Input.GetMousePosition(arg.Camera);

        if (!Input.IsMouseButtonDown(MouseButton.Left))
        {
            if (!arg.IsMouseOverComponentWindow())
            {
                // Add the component
                arg.Sim.LockedAction(s =>
                {
                    arg.Execute(new CAddComponent(description, currentMouse.ToVector2i(Constants.GRIDSIZE)), arg);
                });
            }

            this.GoToState<StateIdle>(0);
        }
    }

    public override void Render(Editor arg)
    {
        var currentMouse = Input.GetMousePosition(arg.Camera);
        var pos = currentMouse.ToVector2i(Constants.GRIDSIZE);
        var comp = this.description.CreateComponent();
        comp.Position = pos;
        comp.RenderSelected(arg.Camera);
        comp.Render(arg.Camera);
    }

    public override bool RenderAboveGUI()
    {
        return true;
    }
}