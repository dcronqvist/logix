using LogiX.Components;
using QuikGraph;

namespace LogiX.Editor.Commands;

public class CommandDeleteJunction : Command<Editor>
{
    public Vector2 position;
    public Vector2 sourcePos;
    public Vector2 targetPos;

    public CommandDeleteJunction(Vector2 position)
    {
        this.position = position;
    }

    public override void Execute(Editor arg)
    {
        WireNode node = Util.GetWireNodeFromPos(arg.Simulator, this.position, out Wire wire);

        List<Edge<WireNode>> edges = wire.Graph.AdjacentEdges(node).ToList();

        this.sourcePos = edges[0].GetOtherVertex(node).GetPosition();
        this.targetPos = edges[1].GetOtherVertex(node).GetPosition();

        wire.RemoveNode(node);

        WireNode source = Util.GetWireNodeFromPos(arg.Simulator, this.sourcePos, out Wire sourceWire);
        WireNode target = Util.GetWireNodeFromPos(arg.Simulator, this.targetPos, out Wire targetWire);

        sourceWire.ConnectNodes(source, targetWire, target);
    }

    public override string ToString()
    {
        return $"Added junction node";
    }

    public override void Undo(Editor arg)
    {
        WireNode source = Util.GetWireNodeFromPos(arg.Simulator, this.sourcePos, out Wire sourceWire);
        WireNode target = Util.GetWireNodeFromPos(arg.Simulator, this.targetPos, out Wire targetWire);

        WireNode newJunc = sourceWire.CreateJunctionWireNode(this.position);

        sourceWire.Graph.RemoveEdgeIf(e => e.Source == source && e.Target == target);

        sourceWire.Graph.AddEdge(new Edge<WireNode>(source, newJunc));
        targetWire.Graph.AddEdge(new Edge<WireNode>(newJunc, target));
    }
}