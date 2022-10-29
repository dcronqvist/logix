namespace LogiX.Architecture.Commands;

public class CDisconnectPoints : Command<Editor>
{
    private Vector2i _startPos;
    private Vector2i _endPos;

    public CDisconnectPoints(Vector2i startPos, Vector2i endPos)
    {
        _startPos = startPos;
        _endPos = endPos;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.DisconnectPoints(_startPos, _endPos);
        });
    }

    public override void Undo(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.ConnectPointsWithWire(_startPos, _endPos);
        });
    }

    public override string ToString()
    {
        return $"Disconnect {_startPos} and {_endPos}";
    }
}
