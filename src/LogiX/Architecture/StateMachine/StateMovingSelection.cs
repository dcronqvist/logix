using System.Numerics;
using LogiX.Architecture.Commands;
using LogiX.GLFW;

namespace LogiX.Architecture.StateMachine;

public class StateMovingSelection : State<Editor, int>
{
    private Vector2 startWorldPos;
    private List<Component> components;
    private List<(Vector2i, Vector2i)> segments;

    public override void OnEnter(Editor updateArg, int arg)
    {
        this.startWorldPos = Input.GetMousePosition(updateArg.Camera);
        updateArg.Sim.LockedAction(s =>
        {
            this.components = s.SelectedComponents.ToList();
            this.segments = s.SelectedWireSegments.ToList();
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
            arg.Sim.LockedAction(s =>
            {
                s.CommitMovedPickedUpSelection(currentSnap - this.startWorldPos.ToVector2i(Constants.GRIDSIZE));
                arg.Execute(new CMoveSelection(components.ToList(), segments.ToList(), currentSnap - this.startWorldPos.ToVector2i(Constants.GRIDSIZE)), arg, false);
            });
            this.GoToState<StateIdle>(0);
        }
    }
}