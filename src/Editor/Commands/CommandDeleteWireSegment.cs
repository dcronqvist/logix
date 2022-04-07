using LogiX.Components;
using QuikGraph;

namespace LogiX.Editor.Commands;

public class CommandDeleteWireSegment : Command<Editor>
{
    enum DeleteWireSegmentType
    {
        DeleteWire,
        DeleteTarget,
        DeleteSource,
        DeleteEdge
    }

    public Vector2 edgePosition;

    public Vector2 sourcePos;
    public Vector2 targetPos;

    public WireNode deletedSource;
    public WireNode deletedTarget;

    private DeleteWireSegmentType type;
    private Wire deletedWire;

    public CommandDeleteWireSegment(Vector2 edgePos)
    {
        this.edgePosition = edgePos;
    }

    private DeleteWireSegmentType GetDeleteWireSegmentType(Editor arg, out Wire wire, out Edge<WireNode> edge)
    {
        edge = Util.GetEdgeFromPos(arg.Simulator, this.edgePosition, out wire);

        int sourceDeg = wire.Graph.AdjacentDegree(edge.Source);
        int targetDeg = wire.Graph.AdjacentDegree(edge.Target);

        if (sourceDeg == 1 && targetDeg == 1)
        {
            // SHOULD REMOVE WIRE
            return DeleteWireSegmentType.DeleteWire;
        }
        else if (sourceDeg > 1 && targetDeg == 1)
        {
            // SHOULD REMOVE TARGET NODE
            return DeleteWireSegmentType.DeleteTarget;
        }
        else if (sourceDeg == 1 && targetDeg > 1)
        {
            // SHOULD REMOVE SOURCE NODE
            return DeleteWireSegmentType.DeleteSource;
        }
        else
        {
            // SHOULD ONLY REMOVE EDGE
            return DeleteWireSegmentType.DeleteEdge;
        }
    }

    public override void Execute(Editor arg)
    {
        this.type = this.GetDeleteWireSegmentType(arg, out Wire wire, out Edge<WireNode> edge);
        this.sourcePos = edge.Source.GetPosition();
        this.targetPos = edge.Target.GetPosition();

        switch (this.type)
        {
            case DeleteWireSegmentType.DeleteWire:
                wire.DisconnectAllIOs();
                arg.Simulator.RemoveWire(wire);
                this.deletedWire = wire;
                break;

            case DeleteWireSegmentType.DeleteTarget:
                wire.RemoveNode(edge.Target);
                this.deletedTarget = edge.Target;
                break;

            case DeleteWireSegmentType.DeleteSource:
                wire.RemoveNode(edge.Source);
                this.deletedSource = edge.Source;
                break;

            case DeleteWireSegmentType.DeleteEdge:
                if (wire.DisconnectNodes(edge.Source, edge.Target, out Wire? newWire))
                {
                    arg.Simulator.AddWire(newWire);
                }
                break;
        }
    }

    public override string ToString()
    {
        return $"Deleted wire segment";
    }

    public override void Undo(Editor arg)
    {
        switch (this.type)
        {
            case DeleteWireSegmentType.DeleteWire:
                arg.Simulator.AddWire(this.deletedWire);
                this.deletedWire.UpdateIOs();
                break;

            case DeleteWireSegmentType.DeleteTarget:
                WireNode source = Util.GetWireNodeFromPos(arg.Simulator, this.sourcePos, out Wire sourceWire);
                sourceWire.AddNode(this.deletedTarget);
                sourceWire.ConnectNodes(source, sourceWire, this.deletedTarget);
                break;

            case DeleteWireSegmentType.DeleteSource:
                WireNode target = Util.GetWireNodeFromPos(arg.Simulator, this.targetPos, out Wire targetWire);
                targetWire.AddNode(this.deletedSource);
                targetWire.ConnectNodes(this.deletedSource, targetWire, target);
                break;

            case DeleteWireSegmentType.DeleteEdge:
                source = Util.GetWireNodeFromPos(arg.Simulator, this.sourcePos, out sourceWire);
                target = Util.GetWireNodeFromPos(arg.Simulator, this.targetPos, out targetWire);
                if (sourceWire.ConnectNodes(source, targetWire, target))
                {
                    arg.Simulator.RemoveWire(targetWire);
                }
                break;
        }
    }
}