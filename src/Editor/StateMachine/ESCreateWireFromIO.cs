using LogiX.Components;
using LogiX.Editor.Commands;

namespace LogiX.Editor.StateMachine;

public class ESCreateWireFromIO : State<Editor, int>
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
        Vector2 ioPos = arg.FirstClickedIO.OnComponent.GetIOPosition(arg.FirstClickedIO);
        Vector2 mousePos = arg.GetWorldMousePos().SnapToGrid();

        if ((mousePos - ioPos).Length() > arg.FirstClickedIO.OnComponent.IORadius && this.determinedDirection == Vector2.Zero)
        {
            this.determinedDirection = Vector2.Normalize(mousePos - ioPos);
            this.determinedDirection = Util.GetClosestPoint(this.determinedDirection, new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1));
        }

        endPoint = mousePos;

        if (arg.Simulator.TryGetJunctionWireNodeFromPosition(mousePos, out JunctionWireNode? node))
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
            Raylib.DrawLineEx(ioPos, corner, 2f, Color.BLACK);
            Raylib.DrawLineEx(corner, endPoint, 2f, Color.BLACK);
        }

        base.Render(arg);
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
            Vector2 mousePos = arg.GetWorldMousePos().SnapToGrid();
            if (arg.Simulator.TryGetJunctionWireNodeFromPosition(mousePos, out JunctionWireNode? junc))
            {
                // CONNECTING TO JUNCTION
                Wire wire = new Wire();
                WireNode start = new IOWireNode(wire, null, arg.FirstClickedIO);
                WireNode end = junc;

                start.ConnectTo(end, out Wire? wireToDelete);
                if (wireToDelete != null)
                {
                    arg.Simulator.RemoveWire(wireToDelete);
                }

                if (this.IsCornerNeeded())
                {
                    start.InsertBetween(new JunctionWireNode(wire, null, this.corner), end);
                }

                arg.Simulator.AddWire(wire);

                this.GoToState<ESNone>(0);
                return;
            }
            else if (arg.Simulator.TryGetIOFromWorldPosition(mousePos, out (IO, int)? io))
            {
                // CONNECTING TO OTHER IO
                // CommandConnectIOToIO connectIOToIO = new CommandConnectIOToIO(arg.FirstClickedIO, io.Value.Item1, this.corner);
                // arg.Execute(connectIOToIO, arg);

                // Wire wire = new Wire();
                // WireNode start = new IOWireNode(wire, null, arg.FirstClickedIO);
                // WireNode end = new IOWireNode(wire, null, io.Value.Item1);

                // start.ConnectTo(end, out Wire? wireToDelete);
                // if (wireToDelete != null)
                // {
                //     arg.Simulator.RemoveWire(wireToDelete);
                // }

                // if (this.IsCornerNeeded())
                // {
                //     WireNode newJunctionWireNode = new JunctionWireNode(wire, null, this.corner);
                //     start.InsertBetween(newJunctionWireNode, end);
                // }

                // arg.Simulator.AddWire(wire);

                CommandConnectIOToIO connectIOToIO = new CommandConnectIOToIO(arg.FirstClickedIO, io.Value.Item1, this.corner);
                arg.Execute(connectIOToIO, arg);

                this.GoToState<ESNone>(0);
                return;
            }
            else if (arg.Simulator.TryGetChildWireNodeFromPosition(mousePos, out WireNode? node))
            {
                // CONNECTING TO WIRE
                JunctionWireNode jwn = new JunctionWireNode(node.Wire, null, mousePos);
                node!.Parent!.InsertBetween(jwn, node);

                Wire wire = new Wire();
                WireNode start = new IOWireNode(wire, null, arg.FirstClickedIO);
                WireNode end = jwn;

                start.ConnectTo(end, out Wire? wireToDelete);
                if (wireToDelete != null)
                {
                    arg.Simulator.RemoveWire(wireToDelete);
                }

                if (this.IsCornerNeeded())
                {
                    JunctionWireNode newJunctionWireNode = new JunctionWireNode(wire, null, this.corner);
                    start.InsertBetween(newJunctionWireNode, end);
                }

                arg.Simulator.AddWire(wire);

                this.GoToState<ESNone>(0);
                return;
            }
            else
            {
                // HERE WE ARE PRESSING ON NOTHING
                // CommandConnectIOToNothing connectIOToNothing = new CommandConnectIOToNothing(arg.FirstClickedIO, this.corner, mousePos);
                // arg.Execute(connectIOToNothing, arg);

                Wire wire = new Wire();
                WireNode start = new IOWireNode(wire, null, arg.FirstClickedIO);
                WireNode end = new JunctionWireNode(wire, null, mousePos);

                start.ConnectTo(end, out Wire? wireToDelete);
                if (wireToDelete != null)
                {
                    arg.Simulator.RemoveWire(wireToDelete);
                }

                if (this.IsCornerNeeded())
                {
                    start.InsertBetween(new JunctionWireNode(wire, null, this.corner), end);
                }

                arg.Simulator.AddWire(wire);

                this.GoToState<ESNone>(0);
            }
        }
    }
}