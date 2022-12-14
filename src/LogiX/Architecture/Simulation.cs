using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Graphics;
using LogiX.Rendering;
using QuikGraph;
using QuikGraph.Algorithms;

namespace LogiX;

public class Simulation : UndirectedGraph<Vector2i, Edge<Vector2i>>
{
    public Scheduler Scheduler { get; set; }
    public List<Node> Nodes { get; set; }
    public List<Node> SelectedNodes { get; set; }
    public List<Edge<Vector2i>> SelectedEdges { get; set; }

    public int TicksSinceStart { get; set; } = 0;

    public Simulation()
    {
        this.Scheduler = new Scheduler();
        this.Nodes = new();
        this.SelectedNodes = new();
        this.SelectedEdges = new();
    }

    public (int, int) Step()
    {
        this.TicksSinceStart++;
        return this.Scheduler.Step();
    }

    public IDictionary<Vector2i, int> GetConnectedComponents()
    {
        var comps = new QuikGraph.Algorithms.ConnectedComponents.ConnectedComponentsAlgorithm<Vector2i, Edge<Vector2i>>(this);
        comps.Compute();
        return comps.Components;
    }

    public IEnumerable<Edge<Vector2i>> GetEdgesForComponent(IDictionary<Vector2i, int> comps, int comp)
    {
        return this.Edges.Where(x => comps[x.Source] == comp).Distinct();
    }

    public Dictionary<int, (Node, string)[]> GetPinConnections()
    {
        var comps = this.GetConnectedComponents();
        var connections = new Dictionary<int, List<(Node, string)>>();
        comps.Values.Distinct().ToList().ForEach(x => connections.TryAdd(x, new()));

        foreach (var node in this.Nodes)
        {
            var pins = this.Scheduler.GetPinCollectionForNode(node);
            foreach (var (id, (conf, obser)) in pins)
            {
                var pos = node.GetPinPosition(pins, id);

                if (comps.ContainsKey(pos))
                {
                    var comp = comps[pos];
                    connections[comp].Add((node, id));
                }
            }
        }

        return connections.ToDictionary(x => x.Key, x => x.Value.ToArray());
    }

    public int GetWireComponentForWireVertex(Vector2i pos)
    {
        var comps = this.GetConnectedComponents();
        return comps[pos];
    }

    public void AddNode(Node node)
    {
        this.Nodes.Add(node);
        this.Scheduler.AddNode(node, true);
        RecalculateConnectionsInScheduler();
    }

    public void RemoveNode(Node node)
    {
        this.Nodes.Remove(node);
        if (this.SelectedNodes.Contains(node))
        {
            this.SelectedNodes.Remove(node);
        }
        this.Scheduler.RemoveNode(node);
        RecalculateConnectionsInScheduler();

    }

    public IEnumerable<T> GetNodesOfType<T>() where T : Node
    {
        return this.Nodes.Where(x => x is T).Cast<T>();
    }

    public void RecalculateConnectionsInScheduler()
    {
        this.TicksSinceStart = 0;
        this.Scheduler.ClearConnections();

        var l = this.Vertices.ToList();
        foreach (var v in l)
        {
            if (this.AdjacentDegree(v) == 0)
            {
                this.RemoveVertex(v);
            }
        }

        foreach (var node in this.Nodes)
        {
            node.Register(this.Scheduler);
        }

        var connections = this.GetPinConnections();

        foreach (var (comp, nodes) in connections)
        {
            if (nodes.Length > 1)
            {
                var first = nodes.First();
                foreach (var pin in nodes.Skip(1))
                {
                    this.Scheduler.AddConnection(first.Item1, first.Item2, pin.Item1, pin.Item2, false);
                }
            }
        }

        this.Scheduler.Prepare();
    }

    public void Render(Camera2D camera)
    {
        foreach (var node in this.SelectedNodes)
        {
            node.RenderSelected(camera);
        }

        foreach (var node in this.Nodes)
        {
            node.Render(this.Scheduler.GetPinCollectionForNode(node), camera);
        }

        foreach (var edge in this.SelectedEdges)
        {
            Wire.RenderSegmentAsSelected(edge);
        }

        var connections = this.GetPinConnections();
        var comps = this.GetConnectedComponents();

        foreach (var (comp, nodes) in connections)
        {
            if (nodes.Length == 0)
            {
                Wire.Render(this.GetEdgesForComponent(comps, comp), Constants.COLOR_UNDEFINED, camera);
            }
            else
            {
                var (n, i) = nodes.First();
                var pins = this.Scheduler.GetPinCollectionForNode(n);
                var (conf, obser) = pins[i];
                var edges = this.GetEdgesForComponent(comps, comp);

                if (obser.Error != ObservableValueError.NONE)
                {
                    Wire.Render(edges, Constants.COLOR_ERROR, camera);
                }
                else
                {
                    var values = obser.Read();
                    Wire.Render(edges, Utilities.GetValueColor(values), camera);
                }
            }
        }
    }

    public bool Interact(Camera2D camera)
    {
        bool precedence = false;
        foreach (var node in this.Nodes)
        {
            precedence |= node.Interact(this.Scheduler.GetPinCollectionForNode(node), camera);
        }
        return precedence;
    }

    public static Simulation FromCircuit(Circuit circuit, params string[] excludeNodes)
    {
        var sim = new Simulation();
        sim.SetCircuitInSimulation(circuit, excludeNodes);
        return sim;
    }

    private List<Wire> GetWires()
    {
        var conns = this.GetConnectedComponents();
        var wires = new List<Wire>();

        foreach (var comp in conns.Values.Distinct())
        {
            var edges = this.GetEdgesForComponent(conns, comp);
            var wire = new Wire(edges.ToList());
            wires.Add(wire);
        }

        return wires;
    }

    public Circuit GetCircuitInSimulation(string name)
    {
        return new Circuit(name, this.Nodes, this.GetWires());
    }

    public void SetCircuitInSimulation(Circuit circuit, params string[] excludeNodes)
    {
        this.Nodes.Clear();
        this.RemoveEdgeIf(x => true);

        this.SelectedNodes.Clear();
        this.SelectedEdges.Clear();

        this.Scheduler = new Scheduler();

        foreach (var node in circuit.Nodes)
        {
            if (excludeNodes.Contains(node.NodeTypeID))
                continue;

            var c = node.CreateNode();
            this.AddNode(c);
        }

        foreach (var wire in circuit.Wires)
        {
            var w = wire.CreateWire();
            foreach (var s in w.Segments)
            {
                if (!this.ContainsVertex(s.Item1))
                    this.AddVertex(s.Item1);
                if (!this.ContainsVertex(s.Item2))
                    this.AddVertex(s.Item2);

                this.AddEdge(new Edge<Vector2i>(s.Item1, s.Item2));
            }
        }

        this.RecalculateConnectionsInScheduler();
    }

    #region SELECTION METHODS

    public void SelectNode(Node node)
    {
        if (!this.SelectedNodes.Contains(node))
            this.SelectedNodes.Add(node);
    }

    public void DeselectNode(Node node)
    {
        if (this.SelectedNodes.Contains(node))
            this.SelectedNodes.Remove(node);
    }

    public void ClearSelection()
    {
        this.SelectedNodes.Clear();
    }

    public void SelectNodesInRect(RectangleF rect)
    {
        foreach (var node in this.Nodes)
        {
            if (node.IsNodeInRect(rect))
                this.SelectNode(node);
        }
    }

    public bool IsNodeSelected(Node node)
    {
        return this.SelectedNodes.Contains(node);
    }

    private List<Node> _pickedNodes = new();
    private List<(Vector2i, Vector2i)> _pickedSegments = new();
    public void PickUpSelection()
    {
        this._pickedNodes = this.SelectedNodes.ToList();
        // this._pickedSegments = this.SelectedWireSegments.ToList();

        this.SelectedNodes.ForEach(s => { this.Nodes.Remove(s); this.Scheduler.RemoveNode(s, false); });
        // this.SelectedWireSegments.ForEach(s => this.DisconnectPoints(s.Item1, s.Item2, false));

        this.SelectedNodes.Clear();
        // this.SelectedWireSegments.Clear();

        this.Scheduler.Prepare();
        this.RecalculateConnectionsInScheduler();
    }

    public void CommitMovedPickedUpSelection(Vector2i delta)
    {
        this._pickedNodes.ForEach(s => s.Move(delta));
        this._pickedSegments.ForEach(s => this.ConnectPointsWithWire(s.Item1 + delta, s.Item2 + delta));

        this.Nodes.AddRange(this._pickedNodes);
        this._pickedNodes.ForEach(s => this.Scheduler.AddNode(s, false));

        this.SelectedNodes = this._pickedNodes.ToList();
        // this.SelectedWireSegments = this._pickedSegments.Select(s => (s.Item1 + delta, s.Item2 + delta)).ToList();

        this.Scheduler.Prepare();
        this.RecalculateConnectionsInScheduler();
    }

    public bool HasSelection()
    {
        return this.SelectedNodes.Count > 0; // || this.SelectedWireSegments.Count > 0;
    }

    public void SelectWireSegment((Vector2i, Vector2i) segment)
    {
        // if (!this.SelectedWireSegments.Contains(segment))
        // {
        //     this.SelectedWireSegments.Add(segment);
        // }
    }

    public void DeselectWireSegment((Vector2i, Vector2i) segment)
    {
        // if (this.SelectedWireSegments.Contains(segment))
        // {
        //     this.SelectedWireSegments.Remove(segment);
        // }
    }

    public void ToggleSelection((Vector2i, Vector2i) segment)
    {
        // if (this.SelectedWireSegments.Contains(segment))
        // {
        //     this.SelectedWireSegments.Remove(segment);
        // }
        // else
        // {
        //     this.SelectedWireSegments.Add(segment);
        // }
    }

    public void SelectWireSegmentsInRectangle(RectangleF rectangle)
    {
        // this.SelectedWireSegments.Clear();

        // foreach (var w in this.Wires)
        // {
        //     foreach (var s in w.Segments)
        //     {
        //         if (Utilities.GetSegmentBoundingBox(s).IntersectsWith(rectangle))
        //         {
        //             this.SelectedWireSegments.Add(s);
        //         }
        //     }
        // }
    }

    public bool IsWireSegmentSelected((Vector2i, Vector2i) segment)
    {
        // return this.SelectedWireSegments.Contains(segment);
        return false;
    }

    #endregion

    public bool TryGetNodeFromPos(Vector2 worldPosition, out Node node)
    {
        foreach (var n in this.Nodes)
        {
            if (n.IsNodeInRect(worldPosition.CreateRect(new Vector2(1, 1))))
            {
                node = n;
                return true;
            }
        }

        node = null;
        return false;
    }

    public Node GetNodeFromID(Guid id)
    {
        return this.Nodes.FirstOrDefault(n => n.ID == id);
    }

    public void ConnectPointsWithWire(Vector2i point1, Vector2i point2)
    {
        if (point1 == point2)
        {
            return;
        }

        if (!this.TryGetWireVertexAtPos(point1.ToVector2(Constants.GRIDSIZE), out var v1, out var d1, out var p1))
        {
            if (this.TryGetWireSegmentAtPos(point1.ToVector2(Constants.GRIDSIZE), out var segment))
            {
                this.SplitAtPoint(point1);
            }
            else
            {
                this.AddVertex(point1);
            }
        }

        if (!this.TryGetWireVertexAtPos(point2.ToVector2(Constants.GRIDSIZE), out var v2, out var d2, out var p2))
        {
            if (this.TryGetWireSegmentAtPos(point2.ToVector2(Constants.GRIDSIZE), out var segment))
            {
                this.SplitAtPoint(point2);
            }
            else
            {
                this.AddVertex(point2);
            }
        }

        this.AddEdge(new Edge<Vector2i>(point1, point2));
        this.RecalculateConnectionsInScheduler();
    }

    public void DisconnectPoints(Vector2i point1, Vector2i point2)
    {
        if (point1 == point2)
        {
            return;
        }

        this.RemoveEdgeIf(e => e.Source == point1 && e.Target == point2);

        if (this.AdjacentDegree(point1) == 0)
        {
            this.RemoveVertex(point1);
        }

        if (this.AdjacentDegree(point2) == 0)
        {
            this.RemoveVertex(point2);
        }

        this.RecalculateConnectionsInScheduler();
    }

    public void SplitAtPoint(Vector2i point)
    {
        if (this.ContainsVertex(point))
        {
            return;
        }

        this.AddVertex(point);

        if (this.TryGetWireSegmentAtPos(point.ToVector2(Constants.GRIDSIZE), out var segment))
        {
            this.RemoveEdgeIf(e => e.Source == segment.Item1 && e.Target == segment.Item2);
            this.AddEdge(new Edge<Vector2i>(segment.Item1, point));
            this.AddEdge(new Edge<Vector2i>(segment.Item2, point));
        }

        this.RecalculateConnectionsInScheduler();
    }

    public void MergeAtPoint(Vector2i point)
    {
        if (!this.ContainsVertex(point))
        {
            return;
        }

        var edges = this.Edges.Where(e => e.Source == point || e.Target == point).ToList();
        var points = edges.SelectMany(e => new[] { e.Source, e.Target }).Distinct().ToArray();
        var (a, b) = Utilities.GetPointsFurthestApart(points);
        foreach (var e in edges)
        {
            this.RemoveEdge(e);
        }

        this.AddEdge(new Edge<Vector2i>(a, b));
        this.RemoveVertex(point);

        this.RecalculateConnectionsInScheduler();
    }

    public bool TryGetPinAtPos(Vector2 position, [NotNullWhen(true)] out Node node, out string identifier)
    {
        foreach (var n in this.Nodes)
        {
            var pins = this.Scheduler.GetPinCollectionForNode(n);
            foreach (var (ident, _) in pins)
            {
                var pinPos = n.GetPinPosition(pins, ident).ToVector2(Constants.GRIDSIZE);
                var maxDist = Constants.PIN_RADIUS;

                if ((pinPos - position).Length() < maxDist)
                {
                    identifier = ident;
                    node = n;
                    return true;
                }
            }
        }

        identifier = null;
        node = null;
        return false;
    }

    public bool TryGetWireSegmentAtPos(Vector2 position, out (Vector2i, Vector2i) segment)
    {
        foreach (var e in this.Edges)
        {
            var p1 = e.Source.ToVector2(Constants.GRIDSIZE);
            var p2 = e.Target.ToVector2(Constants.GRIDSIZE);

            var wrec = Utilities.GetWireRectangle(e.Source, e.Target);

            var dist = Utilities.DistanceToLine(p1, p2, position);
            if (dist < Constants.WIRE_WIDTH && wrec.Contains(position))
            {
                segment = (e.Source, e.Target);
                return true;
            }
        }

        segment = default;
        return false;
    }

    public bool TryGetWireVertexAtPos(Vector2 position, out Vector2i vertex, out int degree, out bool parallel)
    {
        foreach (var v in this.Vertices)
        {
            var vpos = v.ToVector2(Constants.GRIDSIZE);
            var maxDist = Constants.WIRE_POINT_RADIUS;

            if ((vpos - position).Length() < maxDist)
            {
                vertex = v;
                degree = this.AdjacentDegree(v);
                parallel = false;
                var edgesToThisVertex = this.AdjacentEdges(v).Select(e => (e.Source, e.Target)).ToList();
                parallel = Utilities.AreEdgesParallel(edgesToThisVertex.First(), edgesToThisVertex.Last());
                return true;
            }
        }

        degree = 0;
        parallel = false;
        vertex = default;
        return false;
    }
}