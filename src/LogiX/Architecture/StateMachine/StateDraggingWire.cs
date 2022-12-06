using System.Numerics;
using LogiX.Architecture.Commands;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.StateMachine;

public class StateDraggingWire : State<Editor, int>
{
    private Vector2i _startPos;
    private Vector2i _cornerPos;
    private Vector2i _endPos;

    private Vector2 _determinedDirection;

    private int _startBits = -1;

    public override void OnEnter(Editor updateArg, int arg)
    {
        var mouseWorldPos = Input.GetMousePosition(updateArg.Camera);
        _startPos = mouseWorldPos.ToVector2i(Constants.GRIDSIZE);
        _cornerPos = _startPos;
        _endPos = _startPos;
        _determinedDirection = Vector2.Zero;

        updateArg.Sim.LockedAction(s =>
        {
            if (s.TryGetPinAtPos(_startPos.ToVector2(Constants.GRIDSIZE), out var n, out var i))
            {
                var (config, v) = s.Scheduler.GetPinCollectionForNode(n)[i];
                _startBits = config.Bits;
            }
            else if (s.TryGetWireAtPos(_startPos, out var w))
            {
                var pins = s.GetPinsConnectedToWire(w);

                if (pins.Length > 0)
                {
                    var (node, ident) = pins.First();
                    _startBits = s.Scheduler.GetPinCollectionForNode(node)[ident].Item1.Bits;
                }
            }
        });
    }

    public override void Update(Editor arg)
    {
        var mouseWorldPos = Input.GetMousePosition(arg.Camera);
        var mouseGridPos = mouseWorldPos.ToVector2i(Constants.GRIDSIZE);

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
                    if (this._startBits != -1)
                    {
                        if (s.TryGetPinAtPos(_endPos.ToVector2(Constants.GRIDSIZE), out var n, out var i))
                        {
                            var (config, v) = s.Scheduler.GetPinCollectionForNode(n)[i];
                            if (config.Bits != this._startBits)
                            {
                                // Cannot do this
                            }
                            else
                            {
                                // This is allowed
                                var commands = new Command<Editor>[]
                                {
                                    new CAddWire(_startPos, _cornerPos),
                                    new CAddWire(_cornerPos, _endPos)
                                };
                                arg.Execute(new CMulti("Create wire with corner", commands), arg);
                            }
                        }
                        else if (s.TryGetWireAtPos(_endPos, out var wire))
                        {
                            var pins = s.GetPinsConnectedToWire(wire);

                            if (pins.Length > 0)
                            {
                                var (node, ident) = pins.First();
                                var (config, v) = s.Scheduler.GetPinCollectionForNode(node)[ident];
                                if (config.Bits != this._startBits)
                                {
                                    // Cannot do this
                                }
                                else
                                {
                                    // This is allowed
                                    var commands = new Command<Editor>[]
                                    {
                                        new CAddWire(_startPos, _cornerPos),
                                        new CAddWire(_cornerPos, _endPos)
                                    };
                                    arg.Execute(new CMulti("Create wire with corner", commands), arg);
                                }
                            }
                            else
                            {
                                // This is allowed
                                var commands = new Command<Editor>[]
                                {
                                    new CAddWire(_startPos, _cornerPos),
                                    new CAddWire(_cornerPos, _endPos)
                                };
                                arg.Execute(new CMulti("Create wire with corner", commands), arg);
                            }
                        }
                        else
                        {
                            // This is allowed
                            var commands = new Command<Editor>[]
                            {
                                new CAddWire(_startPos, _cornerPos),
                                new CAddWire(_cornerPos, _endPos)
                            };
                            arg.Execute(new CMulti("Create wire with corner", commands), arg);
                        }
                    }
                    else
                    {
                        var commands = new Command<Editor>[]
                        {
                            new CAddWire(_startPos, _cornerPos),
                            new CAddWire(_cornerPos, _endPos)
                        };
                        arg.Execute(new CMulti("Create wire with corner", commands), arg);
                    }
                }
                else
                {
                    if (this._startBits != -1)
                    {
                        if (s.TryGetPinAtPos(_endPos.ToVector2(Constants.GRIDSIZE), out var n, out var i))
                        {
                            var (config, v) = s.Scheduler.GetPinCollectionForNode(n)[i];
                            if (config.Bits != this._startBits)
                            {
                                // Cannot do this
                            }
                            else
                            {
                                // This is allowed
                                arg.Execute(new CAddWire(_startPos, _endPos), arg);
                            }
                        }
                        else if (s.TryGetWireAtPos(_endPos, out var w))
                        {
                            var pins = s.GetPinsConnectedToWire(w);

                            if (pins.Length > 0)
                            {
                                var (node, ident) = pins.First();
                                var (config, v) = s.Scheduler.GetPinCollectionForNode(node)[ident];
                                if (config.Bits != this._startBits)
                                {
                                    // Cannot do this
                                }
                                else
                                {
                                    // This is allowed
                                    arg.Execute(new CAddWire(_startPos, _endPos), arg);
                                }
                            }
                            else
                            {
                                // This is allowed
                                arg.Execute(new CAddWire(_startPos, _endPos), arg);
                            }
                        }
                        else
                        {
                            // This is allowed
                            arg.Execute(new CAddWire(_startPos, _endPos), arg);
                        }
                    }
                    else
                    {
                        arg.Execute(new CAddWire(_startPos, _endPos), arg);
                    }
                }
            });
            this.GoToState<StateIdle>(0);
        }
    }

    private bool CornerNeeded()
    {
        return _cornerPos.X != _endPos.X || _cornerPos.Y != _endPos.Y;
    }

    public override void Render(Editor arg)
    {
        var startWorld = _startPos.ToVector2(Constants.GRIDSIZE);
        var cornerWorld = _cornerPos.ToVector2(Constants.GRIDSIZE);
        var endWorld = _endPos.ToVector2(Constants.GRIDSIZE);

        var color = ColorF.Green;

        arg.Sim.LockedAction(s =>
        {
            if (this._startBits != -1)
            {
                if (s.TryGetPinAtPos(endWorld, out var n, out var i))
                {
                    var (config, v) = s.Scheduler.GetPinCollectionForNode(n)[i];
                    if (config.Bits != this._startBits)
                    {
                        // Cannot do this
                        color = ColorF.Red;
                    }
                }
                else if (s.TryGetWireAtPos(_endPos, out var wire))
                {
                    var pins = s.GetPinsConnectedToWire(wire);

                    if (pins.Length > 0)
                    {
                        var (node, ident) = pins.First();
                        var (config, v) = s.Scheduler.GetPinCollectionForNode(node)[ident];
                        if (config.Bits != this._startBits)
                        {
                            // Cannot do this
                            color = ColorF.Red;
                        }
                    }
                }
            }
        });

        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
        PrimitiveRenderer.RenderLine(startWorld, cornerWorld, Constants.WIRE_WIDTH, color);
        PrimitiveRenderer.RenderLine(cornerWorld, endWorld, Constants.WIRE_WIDTH, color);
    }

    public override void SubmitUI(Editor arg)
    {
        var startWorld = _startPos.ToVector2(Constants.GRIDSIZE);
        var cornerWorld = _cornerPos.ToVector2(Constants.GRIDSIZE);
        var endWorld = _endPos.ToVector2(Constants.GRIDSIZE);

        var color = ColorF.Green;

        arg.Sim.LockedAction(s =>
        {
            if (this._startBits != -1)
            {
                if (s.TryGetPinAtPos(endWorld, out var n, out var i))
                {
                    var (config, v) = s.Scheduler.GetPinCollectionForNode(n)[i];
                    if (config.Bits != this._startBits)
                    {
                        // Cannot do this
                        Utilities.MouseToolTip("Cannot connect pins with different bit widths");
                    }
                }
                else if (s.TryGetWireAtPos(_endPos, out var wire))
                {
                    var pins = s.GetPinsConnectedToWire(wire);

                    if (pins.Length > 0)
                    {
                        var (node, ident) = pins.First();
                        var (config, v) = s.Scheduler.GetPinCollectionForNode(node)[ident];
                        if (config.Bits != this._startBits)
                        {
                            // Cannot do this
                            Utilities.MouseToolTip("Cannot connect pins with different bit widths");
                        }
                    }
                }
            }
        });
    }
}