using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateHoveringWireVertex : State<Editor, int>
{
    public override void Update(Editor arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);

        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetWireVertexAtPos(mouseWorldPos, out var pos, out var degree, out var parallel))
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    this.GoToState<StateDraggingWire>(0);
                }
                if (Input.IsMouseButtonPressed(MouseButton.Right))
                {
                    arg.OpenContextMenu(() =>
                    {
                        if (ImGui.MenuItem("Merge adjacent", null, false, degree == 2 && parallel))
                        {
                            CMergeWirePoint merge = new CMergeWirePoint(pos);
                            arg.Execute(merge, arg);
                            ImGui.CloseCurrentPopup();
                        }
                    });
                }
            }
            else if (s.TryGetWireSegmentAtPos(mouseWorldPos, out var edge))
            {
                this.GoToState<StateHoveringWireSegment>(0);
            }
            else
            {
                // Not hovering wire vertex anymore, go back to idle
                this.GoToState<StateIdle>(0);
            }
        });
    }

    public override void PostSimRender(Editor arg)
    {
        var mouseWorld = Input.GetMousePosition(arg.Camera);
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
        arg.Sim.LockedAction(s =>
        {
            // if (s.TryGetWireVertexAtPos(mouseWorld, out var pos, out var wire))
            // {
            PrimitiveRenderer.RenderCircle(mouseWorld.ToVector2i(Constants.GRIDSIZE).ToVector2(Constants.GRIDSIZE), Constants.WIRE_POINT_RADIUS, 0, Constants.COLOR_SELECTED);
            // }
        });
    }
}