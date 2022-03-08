using LogiX.Components;
using LogiX.Editor.Commands;

namespace LogiX.Editor.StateMachine;

public class ESHoveringWire : State<Editor, int>
{
    public override bool ForcesSameTab => true;
    public WireNode? node;

    public override void OnEnter(Editor? updateArg, int arg)
    {
        updateArg!.Simulator.TryGetChildWireNodeFromPosition(updateArg.GetWorldMousePos(), out node);
    }

    public override void Update(Editor arg)
    {
        if (arg.Simulator.TryGetJunctionWireNodeFromPosition(arg.GetWorldMousePos(), out JunctionWireNode? jwn))
        {
            this.GoToState<ESHoveringJunctionNode>(0);
        }

        if (!arg.Simulator.TryGetChildWireNodeFromPosition(arg.GetWorldMousePos(), out WireNode? n))
        {
            this.GoToState<ESNone>(0);
        }
        else
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                Vector2 clickedMousePos = arg.GetWorldMousePos().SnapToGrid();

                arg.OpenContextMenu("test", () =>
                {
                    ImGui.Text($"Wire {arg.Simulator.AllWires.IndexOf(node!.Wire)}");
                    ImGui.Text($"{node.Wire.IOs.Count} IOs");
                    foreach (IO io in node.Wire.IOs)
                    {
                        ImGui.Text($"{io.OnComponent.Text}");
                    }

                    ImGui.Separator();
                    if (ImGui.MenuItem("Delete Segment"))
                    {
                        CommandDeleteWireSegment cdws = new CommandDeleteWireSegment(node);
                        arg.Execute(cdws, arg);

                        return false;
                    }
                    if (ImGui.MenuItem("Add Junction"))
                    {
                        CommandAddJunction caj = new CommandAddJunction(node, clickedMousePos);
                        arg.Execute(caj, arg);

                        return false;
                    }
                    if (ImGui.MenuItem("Delete Wire"))
                    {
                        CommandDeleteWire cdw = new CommandDeleteWire(node.Wire);
                        arg.Execute(cdw, arg);
                        return false;
                    }

                    return true;
                });
            }
        }
    }

    public override void Render(Editor arg)
    {
        if (arg.Simulator.TryGetChildWireNodeFromPosition(arg.GetWorldMousePos(), out WireNode? node))
        {
            Raylib.DrawLineV(node.Parent!.GetPosition(), node.GetPosition(), Color.BLUE);
        }
    }
}