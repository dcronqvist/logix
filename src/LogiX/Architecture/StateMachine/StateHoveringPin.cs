using ImGuiNET;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateHoveringPin : State<Editor, int>
{
    public override void Update(Editor arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);

        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetPinAtPos(mouseWorldPos, out var node, out var identifier))
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
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetPinAtPos(mouseWorld, out var node, out var identifier))
            {
                PrimitiveRenderer.RenderCircle(mouseWorld.ToVector2i(Constants.GRIDSIZE).ToVector2(Constants.GRIDSIZE), Constants.PIN_RADIUS, 0, Constants.COLOR_SELECTED);
            }
        });
    }

    public override void SubmitUI(Editor arg)
    {
        var mouseWorld = Input.GetMousePosition(arg.Camera);
        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetPinAtPos(mouseWorld, out var node, out var ident))
            {
                Utilities.MouseToolTip($"{ident} : {s.Scheduler.GetPinCollectionForNode(node).GetConfig(ident).Bits}");
            }
        });
    }
}