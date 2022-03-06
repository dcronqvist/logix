using LogiX.Editor.Commands;

namespace LogiX.Editor.StateMachine;

public class ESMovingSelection : State<Editor, int>
{
    Vector2 startPos;
    Vector2 originalStartPos;
    bool willDoCommand;

    public override bool ForcesSameTab => true;

    public override void OnEnter(Editor? updateArg, int arg)
    {
        this.startPos = updateArg!.GetWorldMousePos().SnapToGrid();
        this.originalStartPos = this.startPos;
        this.willDoCommand = arg == 1;
    }

    public override void Update(Editor arg)
    {
        if (MathF.Abs((arg.GetWorldMousePos().SnapToGrid() - startPos).X) > 0 || MathF.Abs((arg.GetWorldMousePos().SnapToGrid() - startPos).Y) > 0)
        {
            arg.Simulator.MoveSelection(arg.GetWorldMousePos().SnapToGrid() - startPos);
            startPos = arg.GetWorldMousePos().SnapToGrid();
        }

        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
        {
            Vector2 endPos = arg.GetWorldMousePos().SnapToGrid();

            if (this.willDoCommand)
            {
                CommandMovedSelection cms = new CommandMovedSelection(arg.Simulator.Selection.Copy(), endPos - this.originalStartPos);
                arg.Execute(cms, arg);
            }

            this.GoToState<ESNone>(0);
        }
    }
}