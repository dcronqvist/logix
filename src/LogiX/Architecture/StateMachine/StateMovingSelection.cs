using System.Numerics;
using LogiX.Architecture.Commands;
using LogiX.GLFW;

namespace LogiX.Architecture.StateMachine;

public class StateMovingSelection : State<Editor, int>
{
    private Vector2 startWorldPos;
    private Vector2 originalStartWorldPos;

    public override void OnEnter(Editor updateArg, int arg)
    {
        this.startWorldPos = Input.GetMousePosition(updateArg.Camera);
        this.originalStartWorldPos = this.startWorldPos;
    }

    public override void Update(Editor arg)
    {
        var currentMouse = Input.GetMousePosition(arg.Camera);

        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            var startSnap = this.startWorldPos.ToVector2i(Constants.GRIDSIZE);
            var currentSnap = currentMouse.ToVector2i(Constants.GRIDSIZE);

            var delta = currentSnap - startSnap;

            if (Math.Abs(delta.X) > 0 || Math.Abs(delta.Y) > 0)
            {
                arg.Sim.LockedAction(s =>
                {
                    s.MoveSelection(delta);
                });
                this.startWorldPos = currentMouse;
            }
        }
        else
        {
            var currentSnap = currentMouse.ToVector2i(Constants.GRIDSIZE);
            arg.Sim.LockedAction(s =>
            {
                arg.Execute(new CMoveSelection(s.SelectedComponents, currentSnap - this.originalStartWorldPos.ToVector2i(Constants.GRIDSIZE)), arg, false);
            });
            this.GoToState<StateIdle>(0);
        }
    }
}