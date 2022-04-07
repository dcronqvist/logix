using LogiX.Components;
using LogiX.SaveSystem;

namespace LogiX.Editor.Commands;


public class CommandMovedSelection : Command<Editor>
{
    List<Component> selectedComponents;
    List<Vector2> selectedWireNodes;
    Vector2 delta;

    public CommandMovedSelection(List<ISelectable> selection, Vector2 delta)
    {
        this.selectedComponents = selection.Where(x => x is Component).Cast<Component>().ToList();
        this.selectedWireNodes = selection.Where(x => x is WireNode).Cast<WireNode>().Select(x => x.GetPosition() - delta).ToList();
        this.delta = delta;
    }

    public override void Execute(Editor arg)
    {
        // DO NOTHING, IT ALREADY HAPPENS IN THE MOUSE MOVE EVENT
    }

    public override void Redo(Editor arg)
    {
        foreach (Component component in this.selectedComponents)
        {
            component.Move(this.delta);
        }

        foreach (Vector2 wireNodePos in this.selectedWireNodes)
        {
            WireNode wn = Util.GetWireNodeFromPos(arg.Simulator, wireNodePos, out Wire wire);
            wn.Move(this.delta);
        }
    }

    public override string ToString()
    {
        return "Moved selection by " + this.delta.ToString();
    }

    public override void Undo(Editor arg)
    {
        foreach (Component component in this.selectedComponents)
        {
            component.Move(-this.delta);
        }

        foreach (Vector2 wireNodePos in this.selectedWireNodes)
        {
            WireNode wn = Util.GetWireNodeFromPos(arg.Simulator, wireNodePos + delta, out Wire wire);
            wn.Move(-this.delta);
        }
    }
}