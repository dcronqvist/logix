using LogiX.Components;
using LogiX.Editor.Commands;
using QuikGraph;

namespace LogiX.Editor.StateMachine;

public class ESHoveringWire : State<Editor, int>
{
    public override bool ForcesSameTab => true;
    public Edge<WireNode>? edge;
    public Wire? wire;

    public override void OnEnter(Editor? updateArg, int arg)
    {
        updateArg!.Simulator.TryGetEdgeFromPosition(updateArg.GetWorldMousePos(), out edge, out wire);
    }

    public override void Update(Editor arg)
    {
        if (arg.Simulator.TryGetJunctionFromPosition(arg.GetWorldMousePos(), out JunctionWireNode? jwn, out Wire? nodeOnWire))
        {
            this.GoToState<ESHoveringJunctionNode>(0);
        }

        if (!arg.Simulator.TryGetEdgeFromPosition(arg.GetWorldMousePos(), out Edge<WireNode>? e, out Wire? w))
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
                    if (ImGui.MenuItem("Delete Segment"))
                    {
                        CommandDeleteWireSegment cdws = new CommandDeleteWireSegment(clickedMousePos);
                        arg.Execute(cdws, arg);

                        return false;
                    }
                    if (ImGui.MenuItem("Add Junction"))
                    {
                        CommandAddJunction caj = new CommandAddJunction(edge.Source.GetPosition(), edge.Target.GetPosition(), clickedMousePos);
                        arg.Execute(caj, arg);

                        return false;
                    }
                    if (ImGui.MenuItem("Delete Wire"))
                    {
                        CommandDeleteWire cdw = new CommandDeleteWire(clickedMousePos);
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
        Raylib.DrawLineV(this.edge!.Source.GetPosition(), this.edge!.Target.GetPosition(), Color.BLUE);
    }
}