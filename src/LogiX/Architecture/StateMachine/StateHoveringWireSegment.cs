using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateHoveringWireSegment : State<Editor, int>
{
    public override void Update(Editor arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);

        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetIOFromPosition(mouseWorldPos, out var group, out var comp))
            {
                this.GoToState<StateHoveringIOGroup>(0);
            }
            else if (s.TryGetWireVertexAtPos(mouseWorldPos, out var pos, out var wire))
            {
                this.GoToState<StateHoveringWireVertex>(0);
            }
            else if (s.TryGetWireSegmentAtPos(mouseWorldPos, out var edge, out wire))
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    this.GoToState<StateDraggingWire>(0);
                }
                else if (Input.IsMouseButtonPressed(MouseButton.Right))
                {
                    arg.OpenContextMenu(() =>
                    {
                        if (ImGui.MenuItem("Delete"))
                        {
                            var disconnect = new CDisconnectPoints(edge.Item1, edge.Item2);
                            arg.Execute(disconnect, arg);
                            ImGui.CloseCurrentPopup();
                        }
                    });
                }
            }
            else
            {
                // Not hovering wire vertex anymore, go back to idle
                this.GoToState<StateIdle>(0);
            }
        });
    }

    public override void Render(Editor arg)
    {
        var mouseWorld = Input.GetMousePosition(arg.Camera);
        var mouseGrid = Input.GetMousePosition(arg.Camera).ToVector2i(Constants.GRIDSIZE);
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        arg.Sim.LockedAction(s =>
        {
            PrimitiveRenderer.RenderCircle(mouseGrid.ToVector2(Constants.GRIDSIZE), Constants.WIRE_POINT_RADIUS, 0f, Constants.COLOR_SELECTED);
        });
    }
}