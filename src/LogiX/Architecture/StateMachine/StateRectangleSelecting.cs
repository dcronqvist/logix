using System.Numerics;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateRectangleSelecting : State<EditorTab, int>
{
    private Vector2 _startWorldPos;

    public override void OnEnter(EditorTab updateArg, int arg)
    {
        this._startWorldPos = Input.GetMousePosition(updateArg.Camera);
    }

    public override void Update(EditorTab arg)
    {
        var currentPos = Input.GetMousePosition(arg.Camera);

        var rect = Utilities.CreateRecFromTwoCorners(this._startWorldPos, currentPos);

        arg.Sim.LockedAction(s =>
        {
            s.SelectComponentsInRectangle(rect);
        });

        if (Input.IsMouseButtonReleased(MouseButton.Left))
        {
            this.GoToState<StateIdle>(0);
        }
    }

    public override void Render(EditorTab arg)
    {
        var currentPos = Input.GetMousePosition(arg.Camera);
        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");

        PrimitiveRenderer.RenderRectangle(shader, Utilities.CreateRecFromTwoCorners(this._startWorldPos, currentPos), Vector2.Zero, 0f, Constants.COLOR_SELECTED * 0.5f, arg.Camera);
    }
}