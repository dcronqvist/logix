using LogiX.Components;

namespace LogiX.Editor.Commands;

public class CommandAddJunction : Command<Editor>
{
    public WireNode childNode;
    public WireNode createdNode;

    public CommandAddJunction(WireNode childNode, Vector2 position)
    {
        this.childNode = childNode;
        this.createdNode = new JunctionWireNode(childNode.Wire, null, position);
    }

    public override void Execute(Editor arg)
    {
        this.childNode.Parent!.InsertBetween(createdNode, this.childNode);
    }

    public override string ToString()
    {
        return $"Added junction node";
    }

    public override void Undo(Editor arg)
    {
        this.createdNode.RemoveOnlyNode(out Wire? wireToDelete);

        // Wont have to delete the wire.
    }
}