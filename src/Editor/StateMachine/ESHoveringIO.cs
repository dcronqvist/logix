using LogiX.Components;

namespace LogiX.Editor.StateMachine;

public class ESHoveringIO : State<Editor, int>
{
    public override bool ForcesSameTab => false;

    public override void Update(Editor arg)
    {
        if (arg.Simulator.TryGetIOFromWorldPosition(arg.GetWorldMousePos(), out (IO, int)? io))
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                if (io.Value.Item1.TryGetIOWireNode(out IOWireNode? iow))
                {
                    arg.FirstClickedWireNode = iow;
                    this.GoToState<ESCreateWireFromWireNode>(0);
                }
                else
                {
                    arg.FirstClickedIO = io.Value.Item1;
                    this.GoToState<ESCreateWireFromIO>(0);
                }
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
            {
                arg.OpenContextMenu("IO", () =>
                {
                    ImGui.Text($"IO on {io.Value.Item1.OnComponent.Text}");
                    ImGui.Text($"Connected to wire {arg.Simulator.AllWires.IndexOf(io.Value.Item1.Wire!)}");

                    ImGui.Separator();

                    return true;
                });
            }
        }
        else
        {
            this.GoToState<ESNone>(0);
        }
    }
}