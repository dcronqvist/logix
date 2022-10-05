namespace LogiX.Architecture.Commands;

public class CAddWire : Command<EditorTab>
{
    private Vector2i _startPos;
    private Vector2i _endPos;

    public CAddWire(Vector2i startPos, Vector2i endPos)
    {
        _startPos = startPos;
        _endPos = endPos;
    }

    public override void Execute(EditorTab arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.ConnectPointsWithWire(_startPos, _endPos);
        });
    }

    public override void Undo(EditorTab arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.DisconnectPoints(_startPos, _endPos);
        });
    }
}
