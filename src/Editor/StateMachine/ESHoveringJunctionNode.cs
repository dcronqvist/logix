using LogiX.Components;

namespace LogiX.Editor.StateMachine;

public class ESHoveringJunctionNode : State<Editor, int>
{
    public override bool ForcesSameTab => true;
    public JunctionWireNode? node;

    public override void OnEnter(Editor? updateArg, int arg)
    {
        updateArg!.Simulator.TryGetJunctionWireNodeFromPosition(updateArg.GetWorldMousePos(), out node);
    }

    public override void Update(Editor arg)
    {
        if (!arg.Simulator.TryGetJunctionWireNodeFromPosition(arg.GetWorldMousePos(), out JunctionWireNode? jwn))
        {
            this.GoToState<ESNone>(0);
            return;
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            if (!arg.Simulator.IsPositionOnSelected(arg.GetWorldMousePos()))
            {
                arg.Simulator.Selection.Clear();
                arg.Simulator.Select(node!);
            }
            this.GoToState<ESMovingSelection>(1);
            return;
        }
    }

    public override void SubmitUI(Editor arg)
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            arg.OpenContextMenu("test", () =>
            {
                ImGui.Text(node!.GetLocationDescriptor().ToStringPretty());
                ImGui.Separator();
                if (ImGui.MenuItem("Create Wire"))
                {
                    arg.FirstClickedWireNode = node;
                    this.GoToState<ESCreateWireFromWireNode>(0);
                    return false;
                }
                if (ImGui.MenuItem("Delete Junction"))
                {
                    node.RemoveOnlyNode(out Wire? wireToDelete);
                    if (wireToDelete != null)
                    {
                        arg.Simulator.RemoveWire(wireToDelete);
                    }

                    return false;
                }
                if (ImGui.MenuItem("Make Root"))
                {
                    node.MakeRoot();
                    return false;
                }

                return true;
            });
        }
    }
}