using LogiX.Components;
using QuikGraph;

namespace LogiX.Editor.Commands;

public class CommandAddJunction : Command<Editor>
{
    public Vector2 position;
    public Vector2 sourcePos;
    public Vector2 targetPos;

    public CommandAddJunction(Vector2 sourcePos, Vector2 targetPos, Vector2 position)
    {
        this.position = position;
        this.sourcePos = sourcePos;
        this.targetPos = targetPos;
    }

    public override void Execute(Editor arg)
    {
        Edge<WireNode> edge = Util.GetEdgeFromPos(arg!.Simulator!, this.position, out Wire wire);
        this.sourcePos = edge.Source.GetPosition();
        this.targetPos = edge.Target.GetPosition();

        wire.InsertNodeBetween(edge.Source, edge.Target, wire.CreateJunctionWireNode(this.position));
    }

    public override string ToString()
    {
        return $"Added junction node";
    }

    public override void Undo(Editor arg)
    {
        WireNode source = Util.GetWireNodeFromPos(arg!.Simulator!, this.sourcePos, out Wire sourceWire);
        WireNode target = Util.GetWireNodeFromPos(arg!.Simulator!, this.targetPos, out Wire targetWire);

        WireNode newJunc = Util.GetWireNodeFromPos(arg!.Simulator!, this.position, out Wire newJuncWire);

        newJuncWire.RemoveNode(newJunc);

        sourceWire.ConnectNodes(source, targetWire, target);
    }
}