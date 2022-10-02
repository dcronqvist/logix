using LogiX.GLFW;

namespace LogiX.Architecture.StateMachine;

public class StateHoveringIOGroup : State<EditorTab, int>
{
    public override void Update(EditorTab arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);

        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetIOGroupFromPosition(mouseWorldPos, out var ioGroup, out var component))
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    this.GoToState<StateDraggingWire>(0);
                }
            }
            else
            {
                // Not hovering IOgroup anymore, go back to idle
                this.GoToState<StateIdle>(0);
            }
        });
    }
}