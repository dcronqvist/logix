using System.Reflection;
using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CPaste : Command<Editor>
{
    public List<Guid> Components { get; set; }
    public List<(Vector2i, Vector2i)> Segments { get; set; }
    public Vector2i NewBasePosition { get; set; }

    public CPaste(List<Guid> components, List<(Vector2i, Vector2i)> segments, Vector2i newBasePosition)
    {
        this.Components = components;
        this.Segments = segments;
        this.NewBasePosition = newBasePosition;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var comps = this.Components.Select(c => s.GetComponentFromID(c)).ToList();
            var middleOfComps = comps.Select(c => c.Position).Average();

            var newComps = comps.Select(c =>
            {
                var compDesc = c.GetDescriptionOfInstance();
                var newComp = compDesc.CreateComponent();
                newComp.ID = Guid.NewGuid();
                newComp.Position = c.Position - middleOfComps + this.NewBasePosition;

                newComp.TriggerSizeRecalculation();
                return newComp;
            });

            foreach (var c in newComps)
            {
                s.AddComponent(c, c.Position);
            }

            foreach (var (s1, s2) in this.Segments)
            {
                s.ConnectPointsWithWire(s1 - middleOfComps + this.NewBasePosition, s2 - middleOfComps + this.NewBasePosition);
            }
        });
    }

    public override string GetDescription()
    {
        return $"Paste {this.Components.Count} components and {this.Segments.Count} wire segments";
    }
}