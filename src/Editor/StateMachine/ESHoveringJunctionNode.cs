using LogiX.Components;
using LogiX.Editor.Commands;
using QuikGraph;

namespace LogiX.Editor.StateMachine;

public class ESHoveringJunctionNode : State<Editor, int>
{
    public override bool ForcesSameTab => true;
    public JunctionWireNode? node;
    public Wire? wire;

    public override void OnEnter(Editor? updateArg, int arg)
    {
        updateArg!.Simulator.TryGetJunctionFromPosition(updateArg.GetWorldMousePos(), out node, out wire);
    }

    public override void Update(Editor arg)
    {
        if (!arg.Simulator.TryGetJunctionFromPosition(arg.GetWorldMousePos(), out JunctionWireNode? jwn, out Wire? w))
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
            Vector2 clickedPos = arg.GetWorldMousePos().SnapToGrid();

            arg.OpenContextMenu("test", () =>
            {
                ImGui.Separator();
                if (ImGui.MenuItem("Create Wire"))
                {
                    arg.FirstClickedWireNode = node;
                    arg.FirstClickedWire = wire;
                    this.GoToState<ESCreateWireFromWireNode>(0);
                    return false;
                }

                if (ImGui.MenuItem("Delete Junction", "", false, wire?.Graph.AdjacentDegree(this.node!) == 2 || wire?.Graph.AdjacentDegree(this.node!) == 1))
                {
                    if (wire?.Graph.AdjacentDegree(this.node!) == 2)
                    {
                        CommandDeleteJunction cdj = new CommandDeleteJunction(clickedPos);
                        arg.Execute(cdj, arg);
                    }
                    else
                    {
                        Edge<WireNode> edge = wire.Graph.AdjacentEdges(this.node!).First();
                        CommandDeleteWireSegment cdws = new CommandDeleteWireSegment(edge.MiddleOfEdge());
                        arg.Execute(cdws, arg);
                    }


                    return false;
                }

                return true;
            });
        }
    }
}