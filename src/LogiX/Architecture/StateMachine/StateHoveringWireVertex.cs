using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateHoveringWireVertex : State<EditorTab, int>
{
    public override void Update(EditorTab arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);

        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetWireVertexAtPos(mouseWorldPos, out var pos, out var wire))
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    this.GoToState<StateDraggingWire>(0);
                }
            }
            else if (s.TryGetWireSegmentAtPos(mouseWorldPos, out var edge, out wire))
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

    public override void Render(EditorTab arg)
    {
        var mouseWorld = Input.GetMousePosition(arg.Camera);
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        arg.Sim.LockedAction(s =>
        {
            // if (s.TryGetWireVertexAtPos(mouseWorld, out var pos, out var wire))
            // {
            PrimitiveRenderer.RenderCircle(pShader, mouseWorld.ToVector2i(16).ToVector2(16), Constants.WIRE_POINT_RADIUS, 0, Constants.COLOR_SELECTED, arg.Camera);
            // }
        });
    }
}