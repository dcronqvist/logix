using System.Numerics;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateRectangleSelecting : State<Editor, int>
{
    private Vector2 _startWorldPos;

    public override void OnEnter(Editor updateArg, int arg)
    {
        this._startWorldPos = Input.GetMousePosition(updateArg.Camera);
    }

    public override void Update(Editor arg)
    {
        var currentPos = Input.GetMousePosition(arg.Camera);

        var rect = Utilities.CreateRecFromTwoCorners(this._startWorldPos, currentPos);

        arg.Sim.LockedAction(s =>
        {
            s.ClearSelection();
            s.SelectNodesInRect(rect);
            s.SelectWireSegmentsInRectangle(rect);
        });

        if (Input.IsMouseButtonReleased(MouseButton.Left))
        {
            this.GoToState<StateIdle>(0);
        }
    }

    public override void PostSimRender(Editor arg)
    {
        var currentPos = Input.GetMousePosition(arg.Camera);
        var shader = LogiXWindow.ContentManager.GetContentItem<ShaderProgram>("logix_core:shaders/primitive/primitive.shader");

        var opacity = 0.3f;
        PrimitiveRenderer.RenderRectangle(Utilities.CreateRecFromTwoCorners(this._startWorldPos, currentPos), Vector2.Zero, 0f, (ColorF.LightSkyBlue * opacity));
    }
}