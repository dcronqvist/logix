using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;
using QuikGraph;

namespace LogiX.Architecture.StateMachine;

public class StateHoveringWireSegment : State<Editor, int>
{
    public override void Update(Editor arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);

        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetPinAtPos(mouseWorldPos, out var node, out var ident))
            {
                this.GoToState<StateHoveringPin>(0);
            }
            else if (s.TryGetWireVertexAtPos(mouseWorldPos, out var pos, out var degree, out var parallel))
            {
                this.GoToState<StateHoveringWireVertex>(0);
            }
            else if (s.TryGetWireSegmentAtPos(mouseWorldPos, out var edge))
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    this.GoToState<StateDraggingWire>(0);
                }
                else if (Input.IsMouseButtonPressed(MouseButton.Right))
                {
                    var gridPos = mouseWorldPos.ToVector2i(Constants.GRIDSIZE);

                    arg.OpenContextMenu(() =>
                    {
                        if (ImGui.MenuItem("Add point"))
                        {
                            var addPoint = new CSplitWire(gridPos);
                            arg.Execute(addPoint, arg);
                            ImGui.CloseCurrentPopup();
                        }
                        if (ImGui.MenuItem("Remove segment"))
                        {
                            var disconnect = new CDeleteWireSegment(edge);
                            arg.Execute(disconnect, arg);
                            ImGui.CloseCurrentPopup();
                        }
                        if (ImGui.MenuItem("Delete Wire"))
                        {
                            var deleteSegments = s.GetWireSegmentsForComponent(s.GetWireComponentForWireSegment(edge)).Select(w => new CDeleteWireSegment((w.Source, w.Target)));
                            arg.Execute(new CMulti("Deleted wire", deleteSegments.ToArray()), arg);
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

    public override void PreSimRender(Editor arg)
    {
        var mouseWorld = Input.GetMousePosition(arg.Camera);
        var mouseGrid = Input.GetMousePosition(arg.Camera).ToVector2i(Constants.GRIDSIZE);
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
        arg.Sim.LockedAction(s =>
        {
            if (s.TryGetWireSegmentAtPos(mouseWorld, out var edge))
            {
                Wire.RenderSegmentAsSelected(new Edge<Vector2i>(edge.Item1, edge.Item2));
                //PrimitiveRenderer.RenderCircle(mouseGrid.ToVector2(Constants.GRIDSIZE), Constants.WIRE_POINT_RADIUS, 0f, Constants.COLOR_SELECTED);
            }
        });
    }
}