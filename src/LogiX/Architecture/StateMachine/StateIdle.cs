using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateIdle : State<Editor, int>
{
    Vector2 clickDownPos;

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
            arg.Sim.LockedAction(s => s.Interact(arg.Camera));

            var done = arg.Sim.LockedAction(s =>
            {
                if (s.TryGetIOFromPosition(mouseWorldPosition, out var group, out var comp))
                {
                    this.GoToState<StateHoveringIOGroup>(0);
                    return true;
                }

                return false;
            });
            if (done) return;

            done = arg.Sim.LockedAction(s =>
            {
                if (s.TryGetWireVertexAtPos(mouseWorldPosition, out var pos, out var wire))
                {
                    this.GoToState<StateHoveringWireVertex>(0);
                    return true;
                }
                return false;
            });
            if (done) return;

            done = arg.Sim.LockedAction(s =>
            {
                if (s.TryGetWireSegmentAtPos(mouseWorldPosition, out var edge, out var wire))
                {
                    this.GoToState<StateHoveringWireSegment>(0);
                    return true;
                }
                return false;
            });
            if (done) return;

            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                this.clickDownPos = mouseWorldPosition;

                arg.Sim.LockedAction(s =>
                {
                    // Check if we clicked on a component
                    if (s.TryGetComponentAtPos(mouseWorldPosition, out var comp))
                    {
                        // If we clicked on a component and it isn't selected, select it.
                        if (!s.IsComponentSelected(comp))
                        {
                            if (!Input.IsKeyDown(Keys.LeftShift))
                            {
                                s.ClearSelection();
                            }
                            s.SelectComponent(comp);
                        }
                        else
                        {
                            if (Input.IsKeyDown(Keys.LeftShift))
                            {
                                s.DeselectComponent(comp);
                            }
                        }
                    }
                    else
                    {
                        // If we didn't click on a component, then we are starting a selection box.
                        s.ClearSelection(); // Clear selection
                        this.GoToState<StateRectangleSelecting>(0);
                    }
                });
            }

            if (Input.IsMouseButtonDown(MouseButton.Left))
            {
                var delta = (mouseWorldPosition - this.clickDownPos).ToVector2i(Constants.GRIDSIZE);

                if (Math.Abs(delta.X) > 0 || Math.Abs(delta.Y) > 0)
                {
                    this.GoToState<StateMovingSelection>(0);
                }
            }
        }
    }
}