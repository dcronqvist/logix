using System.Numerics;
using LogiX.GLFW;
using LogiX.Graphics.UI;

namespace LogiX.Architecture.StateMachine;

public class StateIdle : State<EditorTab, int>
{
    public override void Update(EditorTab arg)
    {
        // Pan around with mouse
        if (Input.IsMouseButtonDown(MouseButton.Middle))
        {
            arg.Camera.FocusPosition -= Input.GetMouseWindowDelta() / arg.Camera.Zoom;
        }

        var mouseWorldPosition = Input.GetMousePosition(arg.Camera);

        if (!NewGUI.AnyWindowHovered())
        {
            arg.Sim.LockedAction(s =>
            {
                s.Interact(arg.Camera);

                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    this.GoToState<StateDraggingWire>(0);
                }
            });
        }
    }
}