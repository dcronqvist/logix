namespace LogiX.Editor.StateMachine;

public class ESRectangleSelecting : State<Editor, int>
{
    Vector2 startPos;

    public override bool ForcesSameTab => true;

    public override void OnEnter(Editor? updateArg, int arg)
    {
        this.startPos = updateArg!.GetWorldMousePos();
    }

    public override void Update(Editor arg)
    {
        arg.Simulator.Selection.Clear();
        arg.Simulator.SelectInRect(Util.CreateRecFromTwoCorners(startPos, arg.GetWorldMousePos()));
        if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
        {
            this.GoToState<ESNone>(0);
        }
    }

    public override void Render(Editor arg)
    {
        Raylib.DrawRectangleLinesEx(Util.CreateRecFromTwoCorners(startPos, arg.GetWorldMousePos()).Inflate(2), 2, Color.BLUE.Opacity(0.5f));
        Raylib.DrawRectangleRec(Util.CreateRecFromTwoCorners(startPos, arg.GetWorldMousePos()), Color.BLUE.Opacity(0.3f));
    }
}