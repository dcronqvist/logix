using LogiX.Components;

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
                        // CommandDeleteWireSegment deleteWireSegment = new CommandDeleteWireSegment(node);
                        // arg.Execute(deleteWireSegment, arg);
                        node!.Parent!.DisconnectFrom(node, out Wire? newWire, out Wire? wireToDelete);

                        if (newWire != null)
                            arg.Simulator.AddWire(newWire);

                        if (wireToDelete != null)
                            arg.Simulator.RemoveWire(wireToDelete);

                        return false;
                    }
                    if (ImGui.MenuItem("Add Junction"))
                    {
                        // CommandAddWireJunction addWireJunction = new CommandAddWireJunction(node, clickedMousePos);
                        // arg.Execute(addWireJunction, arg);
                        List<int> descriptor = node.GetLocationDescriptor();
                        Wire wire = node.Wire;

                        WireNode n = wire.GetWireNodeByDescriptor(descriptor);
                        n.Parent!.InsertBetween(new JunctionWireNode(n.Wire, null, clickedMousePos), n);

                        //node.Parent.InsertBetween(new JunctionWireNode(node.Wire, null, clickedMousePos), node);

                        return false;
                    }
                    if (ImGui.MenuItem("Delete Wire"))
                    {
                        arg.Simulator.RemoveWire(node.Wire);
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