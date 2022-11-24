using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CRotateComponent : Command<Editor>
{
    public Guid Component { get; set; }
    public int Rotation { get; set; }

    public CRotateComponent(Guid comp, int rotation)
    {
        this.Component = comp;
        this.Rotation = rotation;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var comp = s.GetComponentFromID(this.Component);
            var rotationSign = Math.Sign(this.Rotation);
            if (rotationSign == 1)
            {
                comp.RotateClockwise(this.Rotation);
            }
            else
            {
                comp.RotateCounterClockwise(-this.Rotation);
            }
        });
    }

    public override string GetDescription()
    {
        return $"Rotate {this.Component} {this.Rotation} times";
    }
}