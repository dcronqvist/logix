using System.Reflection;
using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CPaste : Command<Editor>
{
    public List<Guid> Node { get; set; }
    public List<(Vector2i, Vector2i)> Segments { get; set; }
    public Vector2i NewBasePosition { get; set; }

    public CPaste(List<Guid> nodes, List<(Vector2i, Vector2i)> segments, Vector2i newBasePosition)
    {
        this.Node = nodes;
        this.Segments = segments;
        this.NewBasePosition = newBasePosition;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var comps = this.Node.Select(c => s.GetNodeFromID(c)).ToList();
            var middleOfComps = comps.Select(c => c.Position).Average();

            s.ClearSelection();

            var newComps = comps.Select(c =>
            {
                var compDesc = c.GetDescriptionOfInstance();
                var newComp = compDesc.CreateNode();
                newComp.ID = Guid.NewGuid();
                newComp.Position = c.Position - middleOfComps + this.NewBasePosition;
                newComp.Rotation = c.Rotation;
                return newComp;
            });

            foreach (var c in newComps)
            {
                s.AddNode(c);
                s.SelectNode(c);
            }

            foreach (var (s1, s2) in this.Segments)
            {
                s.ConnectPointsWithWire(s1 - middleOfComps + this.NewBasePosition, s2 - middleOfComps + this.NewBasePosition, false);
                s.SelectWireSegment((s1 - middleOfComps + this.NewBasePosition, s2 - middleOfComps + this.NewBasePosition));
            }

            s.RecalculateWirePositions();
        }, (e) => { throw e; });
    }

    public override string GetDescription()
    {
        return $"Paste {this.Node.Count} components and {this.Segments.Count} wire segments";
    }
}