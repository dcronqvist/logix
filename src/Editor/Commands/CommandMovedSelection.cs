using LogiX.Components;
using LogiX.SaveSystem;

namespace LogiX.Editor.Commands;


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