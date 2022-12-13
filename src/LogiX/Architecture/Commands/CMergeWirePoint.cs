namespace LogiX.Architecture.Commands;

public class CMergeWirePoint : Command<Editor>
{
    public Vector2i Point { get; set; }

    public CMergeWirePoint(Vector2i point)
    {
        this.Point = point;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.MergeAtPoint(this.Point);
        });
    }

    public override string GetDescription()
    {
        return $"Merge Wire Point at {this.Point}";
    }
}
