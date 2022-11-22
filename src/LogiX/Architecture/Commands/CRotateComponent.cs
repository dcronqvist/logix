using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Commands;

public class CRotateComponent : Command<Editor>
{
    public Component Component { get; set; }
    public int Rotation { get; set; }

    public CRotateComponent(Component comp, int rotation)
    {
        this.Component = comp;
        this.Rotation = rotation;
    }

    public override void Execute(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var rotationSign = Math.Sign(this.Rotation);
            if (rotationSign == 1)
            {
                Component.RotateClockwise(this.Rotation);
            }
            else
            {
                Component.RotateCounterClockwise(-this.Rotation);
            }
        });
    }

    public override void Undo(Editor arg)
    {
        arg.Sim.LockedAction(s =>
        {
            var rotationSign = Math.Sign(this.Rotation);
            if (rotationSign == 1)
            {
                Component.RotateCounterClockwise(this.Rotation);
            }
            else
            {
                Component.RotateClockwise(-this.Rotation);
            }
        });
    }
}