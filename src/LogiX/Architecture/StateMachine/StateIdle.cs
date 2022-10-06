using System.Numerics;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateIdle : State<EditorTab, int>
{
    public override void Update(EditorTab arg)
    {
        // Pan around with mouse
        if (Input.IsMouseButtonDown(MouseButton.Middle))
        {
            arg.Camera.FocusPosition -= Input.GetMouseWindowDelta() / arg.Camera.Zoom;
        }

        var mouseWorldPosition = Input.GetMousePosition(arg.Camera);

        if (!NewGUI.AnyWindowHovered())
        {
            arg.Sim.LockedAction(s =>
            {
                s.Interact(arg.Camera);

                if (s.TryGetIOGroupFromPosition(mouseWorldPosition, out var group, out var comp))
                {
                    this.GoToState<StateHoveringIOGroup>(0);
                }

                if (s.TryGetWireVertexAtPos(mouseWorldPosition.ToVector2i(16), out var wire) && Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    this.GoToState<StateDraggingWire>(0);
                    return;
                }

                if (s.TryGetWireSegmentAtPos(mouseWorldPosition.ToVector2i(16), out var edge, out wire) && Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    this.GoToState<StateDraggingWire>(0);
                    return;
                }
            });
        }
    }

    public override void Render(EditorTab arg)
    {
        var mouseWorld = Input.GetMousePosition(arg.Camera);
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetWireVertexAtPos(mouseWorld, out var pos, out Wire wire))
            {
                PrimitiveRenderer.RenderCircle(pShader, pos.ToVector2(16), Constants.WIRE_POINT_RADIUS, 0, Constants.COLOR_SELECTED, arg.Camera);
                return;
            }

            if (s.TryGetWireSegmentAtPos(mouseWorld, out var edge, out wire))
            {
                PrimitiveRenderer.RenderLine(pShader, edge.Item1.ToVector2(16), edge.Item2.ToVector2(16), Constants.WIRE_WIDTH, Constants.COLOR_SELECTED, arg.Camera);
                return;
            }
            // if (s.TryGetWireVertexAtPos(mouseWorld, out var wire) && Input.IsMouseButtonPressed(MouseButton.Left))
            // {
            //     this.GoToState<StateDraggingWire>(0);
            // }

            // if (s.TryGetWireSegmentAtPos(mouseWorld, out var edge, out wire) && Input.IsMouseButtonPressed(MouseButton.Left))
            // {
            //     this.GoToState<StateDraggingWire>(0);
            // }

            // if (s.TryGetWireSegmentAtPos(mousePos, out var segment, out var wire))
            // {
            //     var points = Utilities.GetAllGridPointsBetween(segment.Item1, segment.Item2);

            //     foreach (var point in points)
            //     {
            //         PrimitiveRenderer.RenderCircle(pShader, point.ToVector2(16), 4, 0, ColorF.Green, this.Camera);
            //     }
            // }
        });
    }
}