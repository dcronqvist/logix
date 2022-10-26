using System.Numerics;
using ImGuiNET;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateIdle : State<Editor, int>
{
    public override void Update(Editor arg)
    {
        // Pan around with mouse
        if (Input.IsMouseButtonDown(MouseButton.Middle))
        {
            arg.Camera.FocusPosition -= Input.GetMouseWindowDelta() / arg.Camera.Zoom;
        }

        var mouseWorldPosition = Input.GetMousePosition(arg.Camera);

        if (!ImGui.GetIO().WantCaptureMouse)
        {
            arg.Sim.LockedAction(s =>
            {
                s.Interact(arg.Camera);

                if (s.TryGetIOFromPosition(mouseWorldPosition, out var group, out var comp))
                {
                    this.GoToState<StateHoveringIOGroup>(0);
                    return;
                }

                if (s.TryGetWireVertexAtPos(mouseWorldPosition, out var pos, out var wire))
                {
                    this.GoToState<StateHoveringWireVertex>(0);
                    return;
                }

                if (s.TryGetWireSegmentAtPos(mouseWorldPosition, out var edge, out wire))
                {
                    this.GoToState<StateHoveringWireSegment>(0);
                    return;
                }

                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    // Check if we clicked on a component
                    if (s.TryGetComponentAtPos(mouseWorldPosition, out comp))
                    {
                        // If we clicked on a component and it isn't selected, select it.
                        if (!s.IsComponentSelected(comp))
                        {
                            if (!Input.IsKeyDown(Keys.LeftShift))
                            {
                                s.ClearSelection();
                            }
                            s.SelectComponent(comp);
                            // Should go to the state of moving selection.
                            // TODO:
                            this.GoToState<StateMovingSelection>(0);
                        }
                        else
                        {
                            if (Input.IsKeyDown(Keys.LeftShift))
                            {
                                s.DeselectComponent(comp);
                            }
                            else
                            {
                                // If it already is selected, immediately go to the state of moving selection.
                                // TODO: 
                                this.GoToState<StateMovingSelection>(0);
                            }
                        }
                    }
                    else
                    {
                        // If we didn't click on a component, then we are starting a selection box.
                        s.ClearSelection(); // Clear selection
                        this.GoToState<StateRectangleSelecting>(0);
                    }
                }
            });
        }
    }
}