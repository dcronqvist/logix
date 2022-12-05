using System.Numerics;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.GLFW;

namespace LogiX.Architecture.StateMachine;

public class StateMovingSelection : State<Editor, int>
{
    private Vector2 startWorldPos;
    private List<(Node, PinCollection)> nodes;
    private List<(Vector2i, Vector2i)> segments;
    private Circuit stateBefore;

    public override void OnEnter(Editor updateArg, int arg)
    {
        this.startWorldPos = Input.GetMousePosition(updateArg.Camera);
        updateArg.Sim.LockedAction(s =>
        {
            this.nodes = s.SelectedNodes.Select(n => (n, s.Scheduler.GetPinCollectionForNode(n))).ToList();
            this.segments = s.SelectedWireSegments.ToList();
            this.stateBefore = updateArg.GetCurrentInvokerState();
            s.PickUpSelection();
        });
    }

    public override void Update(Editor arg)
    {
        var currentMouse = Input.GetMousePosition(arg.Camera);

        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            // var startSnap = this.startWorldPos.ToVector2i(Constants.GRIDSIZE);
            // var currentSnap = currentMouse.ToVector2i(Constants.GRIDSIZE);

            // var delta = currentSnap - startSnap;

            // if (Math.Abs(delta.X) > 0 || Math.Abs(delta.Y) > 0)
            // {
            //     arg.Sim.LockedAction(s =>
            //     {
            //         s.MoveSelection(delta);
            //         s.MoveSelectedWireSegments(delta);
            //     });
            //     this.startWorldPos = currentMouse;
            // }
        }
        else
        {
            var currentSnap = currentMouse.ToVector2i(Constants.GRIDSIZE);
            var delta = currentSnap - this.startWorldPos.ToVector2i(Constants.GRIDSIZE);
            arg.Sim.LockedAction(s =>
            {
                s.CommitMovedPickedUpSelection(delta);
                if (delta != Vector2i.Zero)
                {
                    arg.Execute(stateBefore, new CMoveSelection(nodes.Select(c => c.Item1.ID).ToList(), delta), arg, false);
                }
            });
            this.GoToState<StateIdle>(0);
        }
    }

    public override void Render(Editor arg)
    {
        var currentMouse = Input.GetMousePosition(arg.Camera);
        var currentSnap = currentMouse.ToVector2i(Constants.GRIDSIZE);
        var delta = currentSnap - this.startWorldPos.ToVector2i(Constants.GRIDSIZE);

        foreach (var (node, pins) in this.nodes)
        {
            var realPos = node.Position;
            var pos = node.Position + delta;

            node.Position = pos;
            node.RenderSelected(arg.Camera);
            node.Render(pins, arg.Camera);
            node.Position = realPos;
        }

        foreach (var segment in this.segments)
        {
            Wire.RenderSegmentAsSelected((segment.Item1 + delta, segment.Item2 + delta));
            Wire.RenderSegment((segment.Item1 + delta, segment.Item2 + delta), Constants.COLOR_UNDEFINED);
        }
    }
}