namespace LogiX.Architecture.Commands;

public class CSplitWire : Command<Editor>
{
    public Vector2i Point { get; set; }

    public CSplitWire(Vector2i point)
    {
        this.Point = point;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.SplitAtPoint(this.Point);
        });
    }

    public override string GetDescription()
    {
        return $"Splt Wire at {this.Point}";
    }
}
