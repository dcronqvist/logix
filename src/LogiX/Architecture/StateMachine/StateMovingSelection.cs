using System.Numerics;
using LogiX.Architecture.Commands;
using LogiX.GLFW;

namespace LogiX.Architecture.StateMachine;

public class StateMovingSelection : State<EditorTab, int>
{
    private Vector2 startWorldPos;
    private Vector2 originalStartWorldPos;

    public override void OnEnter(EditorTab updateArg, int arg)
    {
        this.startWorldPos = Input.GetMousePosition(updateArg.Camera);
        this.originalStartWorldPos = this.startWorldPos;
    }

    public override void Update(EditorTab arg)
    {
        var currentMouse = Input.GetMousePosition(arg.Camera);

        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            var startSnap = this.startWorldPos.ToVector2i(16);
            var currentSnap = currentMouse.ToVector2i(16);

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
            var currentSnap = currentMouse.ToVector2i(16);
            arg.Sim.LockedAction(s =>
            {
                arg.Execute(new CMoveSelection(s.SelectedComponents, currentSnap - this.originalStartWorldPos.ToVector2i(16)), arg, false);
            });
            this.GoToState<StateIdle>(0);
        }
    }
}