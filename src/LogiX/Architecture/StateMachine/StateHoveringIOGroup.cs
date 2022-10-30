using ImGuiNET;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateHoveringIOGroup : State<Editor, int>
{
    public override void Update(Editor arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);

        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetIOFromPosition(mouseWorldPos, out var ioGroup, out var component))
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

    public override void Render(Editor arg)
    {
        var mouseWorld = Input.GetMousePosition(arg.Camera);
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetIOFromPosition(mouseWorld, out var io, out var comp))
            {
                PrimitiveRenderer.RenderCircle(comp.GetPositionForIO(io, out var le).ToVector2(Constants.GRIDSIZE), Constants.IO_GROUP_RADIUS, 0, Constants.COLOR_SELECTED);
            }
        });
    }

    public override void SubmitUI(Editor arg)
    {
        var mouseWorld = Input.GetMousePosition(arg.Camera);
        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetIOFromPosition(mouseWorld, out var io, out var comp))
            {
                if (comp.DisplayIOGroupIdentifiers)
                    Utilities.MouseToolTip(io.Identifier);
            }
        });
    }
}