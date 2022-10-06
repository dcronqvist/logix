using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateHoveringIOGroup : State<EditorTab, int>
{
    public override void Update(EditorTab arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);

        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetIOGroupFromPosition(mouseWorldPos, out var ioGroup, out var component))
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    this.GoToState<StateDraggingWire>(0);
                }
            }
            else
            {
                // Not hovering IOgroup anymore, go back to idle
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
            if (s.TryGetIOGroupFromPosition(mouseWorld, out var group, out var comp))
            {
                PrimitiveRenderer.RenderCircle(pShader, comp.GetPositionForGroup(group, out var le).ToVector2(16), Constants.IO_GROUP_RADIUS, 0, Constants.COLOR_SELECTED, arg.Camera);
            }
        });
    }
}