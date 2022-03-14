using QuikGraph;
using LogiX.Components;

namespace LogiX.SaveSystem;

public struct Node
{
    public Vector2 Position { get; set; }

    public string? Component { get; set; }
    public int IOIndex { get; set; }
}

public class WireDescription
{

    public UndirectedGraph<Node, Edge<Node>> Graph { get; set; }

    public WireDescription(UndirectedGraph<Node, Edge<Node>> graph)
    {
        this.Graph = graph;
    }

    public Component GetComponentByID(List<Component> comps, string id)
    {
        foreach (Component comp in comps)
        {
            if (comp.UniqueID == id)
            {
                return comp;
            }
        }
        return null;
    }

    public Wire ToWire(List<Component> components)
    {
        Wire wire = new Wire();

        Dictionary<Node, WireNode> nodeToWireNode = new Dictionary<Node, WireNode>();

        foreach (Node vertex in this.Graph.Vertices)
        {
            if (vertex.Component is null)
            {
                // This is a junction
                WireNode junc = wire.CreateJunctionWireNode(vertex.Position);
                nodeToWireNode.Add(vertex, junc);
            }
            else
            {
                // This is an IO node
                Component comp = this.GetComponentByID(components, vertex.Component);
                WireNode ioNode = wire.CreateIOWireNode(comp.GetIO(vertex.IOIndex));
                nodeToWireNode.Add(vertex, ioNode);
            }
        }

        foreach (Edge<Node> edge in this.Graph.Edges)
        {
            WireNode source = nodeToWireNode[edge.Source];
            WireNode target = nodeToWireNode[edge.Target];

            wire.Graph.AddEdge(new Edge<WireNode>(source, target));
        }

        wire.UpdateIOs();

        return wire;
    }
}