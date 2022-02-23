using LogiX.Components;
using LogiX.SaveSystem;

namespace LogiX.Editor;

public abstract class Command<TArg>
{
    public abstract void Execute(TArg arg);
    public abstract void Undo(TArg arg);
    public virtual void Redo(TArg arg) { Execute(arg); }

    public override abstract string ToString();
}

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

public class CommandMovedSelection : Command<Editor>
{
    List<ISelectable> selection;
    Vector2 delta;

    public CommandMovedSelection(List<ISelectable> selection, Vector2 delta)
    {
        this.selection = selection;
        this.delta = delta;
    }

    public override void Execute(Editor arg)
    {
        // DO NOTHING, IT ALREADY HAPPENS IN THE MOUSE MOVE EVENT
    }

    public override void Redo(Editor arg)
    {
        foreach (ISelectable sel in this.selection)
        {
            sel.Move(this.delta);
        }
    }

    public override string ToString()
    {
        return "Moved selection by " + this.delta.ToString();
    }

    public override void Undo(Editor arg)
    {
        foreach (ISelectable sel in this.selection)
        {
            sel.Move(-this.delta);
        }
    }
}