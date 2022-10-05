using System.Numerics;
using LogiX.Architecture.Commands;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateDraggingWire : State<EditorTab, int>
{
    private Vector2i _startPos;
    private Vector2i _cornerPos;
    private Vector2i _endPos;

    private Vector2 _determinedDirection;

    public override void OnEnter(EditorTab updateArg, int arg)
    {
        var mouseWorldPos = Input.GetMousePosition(updateArg.Camera);
        _startPos = mouseWorldPos.ToVector2i(16);
        _cornerPos = _startPos;
        _endPos = _startPos;
        _determinedDirection = Vector2.Zero;
    }

    public override void Update(EditorTab arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);
        var mouseGridPos = mouseWorldPos.ToVector2i(16);

        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            if ((mouseGridPos - _startPos).Length() >= 1 && this._determinedDirection == Vector2.Zero)
            {
                this._determinedDirection = (mouseGridPos - _startPos).Normalized();
                this._determinedDirection = Utilities.GetClosestPoint(this._determinedDirection, new Vector2[] { Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX, -Vector2.UnitY });
            }

            _endPos = mouseGridPos;

            if (MathF.Abs(this._determinedDirection.X) == 1)
            {
                _cornerPos = new Vector2i(_endPos.X, _startPos.Y);
                if (this._determinedDirection.X == 1f)
                {
                    if (this._endPos.X <= _startPos.X)
                    {
                        this._determinedDirection = Vector2.Zero;
                    }
                }
                else
                {
                    if (this._endPos.X >= this._startPos.X)
                    {
                        this._determinedDirection = Vector2.Zero;
                    }
                }
            }
            else
            {
                _cornerPos = new Vector2i(_startPos.X, _endPos.Y);
                if (this._determinedDirection.Y == 1f)
                {
                    if (this._endPos.Y <= _startPos.Y)
                    {
                        this._determinedDirection = Vector2.Zero;
                    }
                }
                else
                {
                    if (this._endPos.Y >= this._startPos.Y)
                    {
                        this._determinedDirection = Vector2.Zero;
                    }
                }
            }
        }
        else
        {
            // Create the wire between these two positions
            Console.WriteLine($"Create wire between {_startPos.ToString()} and {_endPos.ToString()}");
            arg.Sim.LockedAction(s =>
            {
                Console.WriteLine($"Create wire between {_startPos.ToString()}, {_cornerPos.ToString()} and {_endPos.ToString()}, CORNER: {this.CornerNeeded()}");
                if (CornerNeeded())
                {
                    arg.Execute(new CAddWire(_startPos, _cornerPos), arg);
                    arg.Execute(new CAddWire(_cornerPos, _endPos), arg);
                }
                else
                {
                    arg.Execute(new CAddWire(_startPos, _endPos), arg);
                }
            });
            this.GoToState<StateIdle>(0);
        }
    }

    private bool CornerNeeded()
    {
        return _cornerPos.X != _endPos.X || _cornerPos.Y != _endPos.Y;
    }

    public override void Render(EditorTab arg)
    {
        var startWorld = _startPos.ToVector2(16);
        var cornerWorld = _cornerPos.ToVector2(16);
        var endWorld = _endPos.ToVector2(16);

        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        PrimitiveRenderer.RenderLine(pShader, startWorld, cornerWorld, 2, ColorF.Green, arg.Camera);
        PrimitiveRenderer.RenderLine(pShader, cornerWorld, endWorld, 2, ColorF.Green, arg.Camera);
    }
}