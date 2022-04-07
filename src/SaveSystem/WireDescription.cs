using QuikGraph;
using LogiX.Components;

namespace LogiX.SaveSystem;

public class Node
{
    public Vector2 Position { get; set; }

    public string? Component { get; set; }
    public int IOIndex { get; set; }
}

public class WireDescription
{
    public List<Node> Vertices { get; set; }
    public List<(int, int)> Edges { get; set; }

    [JsonConstructor]
    public WireDescription(List<Node> vertices, List<(int, int)> edges)
    {
        this.Vertices = vertices;
        this.Edges = edges;
    }

    public WireDescription(UndirectedGraph<Node, Edge<Node>> graph)
    {
        this.Vertices = new List<Node>();
        this.Edges = new List<(int, int)>();

        foreach (Node vertex in graph.Vertices)
        {
            List<Node> adj = graph.AdjacentVertices(vertex).ToList();
            this.Vertices.Add(vertex);
        }

        foreach (Edge<Node> edge in graph.Edges)
        {
            this.Edges.Add((this.Vertices.IndexOf(edge.Source), this.Vertices.IndexOf(edge.Target)));
        }
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

        foreach (Node vertex in this.Vertices)
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

        foreach ((int u, int v) in this.Edges)
        {
            Node source = this.Vertices[u];
            Node target = this.Vertices[v];

            WireNode node1 = nodeToWireNode[source];
            WireNode node2 = nodeToWireNode[target];

            wire.Graph.AddEdge(new Edge<WireNode>(node1, node2));
        }

        wire.UpdateIOs();

        return wire;
    }
}