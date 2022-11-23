using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CDeleteWireSegment : Command<Editor>
{
    public (Vector2i, Vector2i) Segment { get; set; }

    public CDeleteWireSegment((Vector2i, Vector2i) segment)
    {
        this.Segment = segment;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            s.DisconnectPoints(this.Segment.Item1, this.Segment.Item2);
            if (s.SelectedWireSegments.Contains(this.Segment))
            {
                s.SelectedWireSegments.Remove(this.Segment);
            }
        });
    }
}