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
                if (io.Value.Item1.TryGetIOWireNode(out IOWireNode? ioWireNode))
                {
                    arg.FirstClickedWireNode = ioWireNode;
                    this.GoToState<ESCreateWireFromWireNode>(0);
                }
                else
                {
                    arg.FirstClickedIO = io.Value.Item1;
                    this.GoToState<ESCreateWireFromIO>(0);
                }

                // GO TO STATE IO->CONNECT
                // LEFT PRESSED ON IO, WE ARE PERFORMING SOME KIND OF CONNECTIONTHINGY HERE
                // EITHER WE HAVE TWO OPTIONS
                // 1. CONNECT DIRECTLY TO ANOTHER IO -> NEW WIRE MUST BE CREATED BETWEEN THESE
                // 2. CONNECT TO AN EXISTING WIRE -> WE MUST CONNECT THIS IO TO THE WIRE AND ADD AN IOWIREPOINT TO THAT WIRE WHICH POINTS TO THIS IO
            }
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
            {
                arg.OpenContextMenu("IO", () =>
                {
                    ImGui.Text($"IO on {io.Value.Item1.OnComponent.Text}");
                    ImGui.Text($"Connected to wire {arg.Simulator.AllWires.IndexOf(io.Value.Item1.Wire)}");

                    ImGui.Separator();

                    if (ImGui.MenuItem("Make Root"))
                    {
                        io.Value.Item1.GetIOWireNode().MakeRoot();
                    }

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