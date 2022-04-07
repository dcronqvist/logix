using LogiX.Components;
using LogiX.SaveSystem;

namespace LogiX.Editor.Commands;

public class CommandNewComponent : Command<Editor>
{
    Component c;

    public CommandNewComponent(Component c)
    {
        this.c = c;
    }

    public override void Execute(Editor arg)
    {
        arg.NewComponent(c);
    }

    public override void Redo(Editor arg)
    {
        arg.NewComponent(c, true);
    }

    public override string ToString()
    {
        return "Created new " + c.Text + " component";
    }

    public override void Undo(Editor arg)
    {
        arg.Simulator.RemoveComponent(c);
    }
}