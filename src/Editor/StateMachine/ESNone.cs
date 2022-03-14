using LogiX.Components;
using LogiX.Editor.Commands;
using QuikGraph;

namespace LogiX.Editor.StateMachine;

public class ESNone : State<Editor, int>
{
    public override bool ForcesSameTab => false;

    public override void Update(Editor arg)
    {
        if (arg.IsMouseInWorld && !ImGui.GetIO().WantCaptureKeyboard)
        {
            if (arg.Simulator.TryGetIOFromWorldPosition(arg.GetWorldMousePos(), out (IO, int)? io))
            {
                this.GoToState<ESHoveringIO>(0);
            }
            else if (arg.Simulator.TryGetJunctionFromPosition(arg.GetWorldMousePos(), out JunctionWireNode? jwn, out Wire? nodeOnWire))
            {
                this.GoToState<ESHoveringJunctionNode>(0);
            }
            else if (arg.Simulator.TryGetEdgeFromPosition(arg.GetWorldMousePos(), out Edge<WireNode>? edge, out Wire? edgeOnWire))
            {
                this.GoToState<ESHoveringWire>(0);
            }
        }

        if (arg.IsMouseInWorld)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                // PRESSING DOWN LEFT MOUSE BUTTON
                if (arg.Simulator.IsPositionOnSelected(arg.GetWorldMousePos()))
                {
                    // PRESSED DOWN ON SOMETHING THAT IS ALREADY SELECTED
                    this.GoToState<ESMovingSelection>(1);
                }
                else if (arg.Simulator.TryGetComponentFromWorldPosition(arg.GetWorldMousePos(), out Component? comp))
                {
                    // IF NOT SELECTED, SELECT THIS AND CLEAR THE PREVIOUS SELECTION, GO TO MOVESELECTION STATE
                    arg.Simulator.Selection.Clear();
                    arg.Simulator.Select(comp);
                    this.GoToState<ESMovingSelection>(1);
                }
                else
                {
                    // NOT PRESSING DOWN ON A COMPONENT - TODO: MIGHT BE PRESSING DOWN ON WIRES
                    // GO TO RECTANGLE SELECTING
                    this.GoToState<ESRectangleSelecting>(0);
                }
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
            {

            }
        }

        if (arg.Simulator.Selection.Where(x => x is Component).Count() > 0)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
            {
                List<Component> comps = arg.Simulator.Selection.Where(x => x is Component).Cast<Component>().ToList();

                List<Command<Editor>> commands = new List<Command<Editor>>();
                foreach (Component c in comps)
                {
                    commands.Add(new CommandDeleteComponent(c));
                }
                MultiCommand<Editor> multiCommand = new MultiCommand<Editor>($"Deleted {comps.Count} Components", commands.ToArray());
                arg.Execute(multiCommand);
            }
        }
    }
}