namespace LogiX.Architecture.Commands;

public class CAddWire : Command<Editor>
{
    private Vector2i _startPos;
    private Vector2i _endPos;

    public CAddWire(Vector2i startPos, Vector2i endPos)
    {
        _startPos = startPos;
        _endPos = endPos;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.ConnectPointsWithWire(_startPos, _endPos);
        });
    }

    public override string ToString()
    {
        return $"Connect {_startPos} to {_endPos}";
    }
}
