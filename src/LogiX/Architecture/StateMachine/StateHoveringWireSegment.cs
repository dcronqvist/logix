using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateHoveringWireSegment : State<EditorTab, int>
{
    public override void Update(EditorTab arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);

        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetIOGroupFromPosition(mouseWorldPos, out var group, out var comp))
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
                        if (NewGUI.MenuItem("Delete"))
                        {
                            s.DisconnectPoints(edge.Item1, edge.Item2);
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

    public override void Render(EditorTab arg)
    {
        var mouseWorld = Input.GetMousePosition(arg.Camera);
        var mouseGrid = Input.GetMousePosition(arg.Camera).ToVector2i(16);
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        arg.Sim.LockedAction(s =>
        {
            // if (s.TryGetWireSegmentAtPos(mouseWorld, out var edge, out var wire))
            // {
            //     PrimitiveRenderer.RenderLine(pShader, edge.Item1.ToVector2(16), edge.Item2.ToVector2(16), Constants.WIRE_WIDTH, Constants.COLOR_SELECTED, arg.Camera);
            // }

            PrimitiveRenderer.RenderCircle(pShader, mouseGrid.ToVector2(16), Constants.WIRE_POINT_RADIUS, 0f, Constants.COLOR_SELECTED, arg.Camera);
        });
    }
}