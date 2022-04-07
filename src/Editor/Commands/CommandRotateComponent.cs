using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandRotateComponent : Command<Editor>
{
    public Component comp;
    public int rotation;

    public CommandRotateComponent(Component comp, int rotation)
    {
        this.comp = comp;
        this.rotation = rotation;
    }

    public override void Execute(Editor arg)
    {
        for (int i = 0; i < this.rotation; i++)
        {
            this.comp.RotateRight();
        }
    }

    public override void Undo(Editor arg)
    {
        for (int i = 0; i < this.rotation; i++)
        {
            this.comp.RotateLeft();
        }
    }
}
