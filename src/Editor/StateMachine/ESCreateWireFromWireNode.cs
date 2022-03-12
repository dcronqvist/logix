using LogiX.Components;
using LogiX.Editor.Commands;
using QuikGraph;

namespace LogiX.Editor.StateMachine;

public class ESCreateWireFromWireNode : State<Editor, int>
{
    public override bool ForcesSameTab => true;

    public Vector2 determinedDirection;
    Vector2 corner;
    Vector2 endPoint;

    public override void OnEnter(Editor? updateArg, int arg)
    {
        this.determinedDirection = Vector2.Zero;
        base.OnEnter(updateArg, arg);
    }

    public bool IsCornerNeeded()
    {
        return corner.X != endPoint.X || corner.Y != endPoint.Y;
    }

    public override void Render(Editor arg)
    {
        Vector2 ioPos = arg.FirstClickedWireNode.GetPosition();
        Vector2 mousePos = arg.GetWorldMousePos().SnapToGrid();

        if ((mousePos - ioPos).Length() > 3 && this.determinedDirection == Vector2.Zero)
        {
            this.determinedDirection = Vector2.Normalize(mousePos - ioPos);
            this.determinedDirection = Util.GetClosestPoint(this.determinedDirection, new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1));
        }

        endPoint = mousePos;

        if (arg.Simulator.TryGetJunctionFromPosition(mousePos, out JunctionWireNode? node, out Wire? w))
        {
            endPoint = node.GetPosition();
        }

        if (MathF.Abs(this.determinedDirection.X) == 1)
        {
            corner = new Vector2(endPoint.X, ioPos.Y);

            if (this.determinedDirection.X == 1f)
            {
                if (endPoint.X <= ioPos.X)
                {
                    this.determinedDirection = Vector2.Zero;
                }
            }
            else
            {
                if (endPoint.X >= ioPos.X)
                {
                    this.determinedDirection = Vector2.Zero;
                }
            }
        }
        else
        {
            corner = new Vector2(ioPos.X, endPoint.Y);

            if (this.determinedDirection.Y == 1f)
            {
                if (endPoint.Y <= ioPos.Y)
                {
                    this.determinedDirection = Vector2.Zero;
                }
            }
            else
            {
                if (endPoint.Y >= ioPos.Y)
                {
                    this.determinedDirection = Vector2.Zero;
                }
            }
        }

        if (this.determinedDirection != Vector2.Zero)
        {
            this.RenderNewWire(Color.GRAY, arg.FirstClickedWireNode.GetPosition(), corner, endPoint);
        }

        base.Render(arg);
    }

    public void RenderNewWire(Color color, params Vector2[] positions)
    {
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector2 start = positions[i];
            Vector2 end = positions[i + 1];
            Raylib.DrawLineEx(start, end, 5f, Color.BLACK);
            Raylib.DrawLineEx(start, end, 3f, color);
        }

        for (int i = 0; i < positions.Length; i++)
        {
            Vector2 pos = positions[i];
            float rad = 2f;
            Raylib.DrawCircleV(pos, rad + 1, Color.BLACK);
            Raylib.DrawCircleV(pos, rad, color);
        }
    }

    public override void SubmitUI(Editor arg)
    {
        base.SubmitUI(arg);
    }

    public override void Update(Editor arg)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
        {
            this.GoToState<ESNone>(0);
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            // CHECK IF WE ARE PRESSING ON SOMETHING (IO OR OTHER WIRE)
            if (arg.Simulator.TryGetJunctionFromPosition(arg.GetWorldMousePos().SnapToGrid(), out JunctionWireNode? junc, out Wire? wjunc))
            {
                // CONNECTING TO JUNCTION
                CommandConnectJunctions ccj = new CommandConnectJunctions(arg.FirstClickedWireNode.GetPosition(), junc.GetPosition(), this.corner);
                arg.Execute(ccj, arg);

                this.GoToState<ESNone>(0);

                return;
            }
            else if (arg.Simulator.TryGetIOFromWorldPosition(arg.GetWorldMousePos().SnapToGrid(), out (IO, int)? io))
            {
                // CONNECTING TO OTHER IO
                CommandConnectIOToJunction ciojunc = new CommandConnectIOToJunction(io.Value.Item1, arg.FirstClickedWireNode.GetPosition(), this.corner);
                arg.Execute(ciojunc, arg);

                this.GoToState<ESNone>(0);
                return;
            }
            // else if (arg.Simulator.TryGetChildWireNodeFromPosition(arg.GetWorldMousePos().SnapToGrid(), out WireNode? node))
            // {
            //     // CONNECTING TO WIRE
            //     // if (node.Wire != arg.FirstClickedWireNode.Wire)
            //     // {
            //     //     WireNode start = arg.FirstClickedWireNode;
            //     //     JunctionWireNode jwn = new JunctionWireNode(node.Wire, null, arg.GetWorldMousePos().SnapToGrid());
            //     //     node!.Parent!.InsertBetween(jwn, node);

            //     //     start.ConnectTo(jwn, out Wire? wireToDelete);
            //     //     if (wireToDelete != null)
            //     //     {
            //     //         arg.Simulator.RemoveWire(wireToDelete);
            //     //     }

            //     //     if (this.IsCornerNeeded())
            //     //     {
            //     //         JunctionWireNode newJunctionWireNode = new JunctionWireNode(node.Wire, null, this.corner);
            //     //         start.InsertBetween(newJunctionWireNode, jwn);
            //     //     }

            //     //     this.GoToState<ESNone>(0);
            //     //     return;
            //     // }
            //     // else
            //     // {
            //     //     return;
            //     // }
            // }
            // else
            // {
            //     // HERE WE ARE PRESSING ON NOTHING
            //     // CommandConnectWireToNothing connectWireToNothing = new CommandConnectWireToNothing(arg.FirstClickedWireNode, this.corner, arg.GetWorldMousePos().SnapToGrid());
            //     // arg.Execute(connectWireToNothing, arg);

            //     // WireNode start = arg.FirstClickedWireNode;
            //     // WireNode end = new JunctionWireNode(start.Wire, null, arg.GetWorldMousePos().SnapToGrid());

            //     // start.ConnectTo(end, out Wire? wireToDelete);
            //     // if (wireToDelete != null)
            //     // {
            //     //     arg.Simulator.RemoveWire(wireToDelete);
            //     // }

            //     // if (this.IsCornerNeeded())
            //     // {
            //     //     WireNode newJunctionWireNode = new JunctionWireNode(start.Wire, null, this.corner);
            //     //     start.InsertBetween(newJunctionWireNode, end);
            //     // }

            //     this.GoToState<ESNone>(0);
            // }
        }
    }
}