using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX;

public class Simulation
{
    public Scheduler Scheduler { get; set; }
    public List<Node> Nodes { get; set; }
    public List<Wire> Wires { get; set; }

    public List<Node> SelectedNodes { get; set; }
    public List<(Vector2i, Vector2i)> SelectedWireSegments { get; set; }
    public Dictionary<Vector2i, Wire> WirePositions { get; set; } = new();

    public Simulation()
    {
        this.Scheduler = new Scheduler();
        this.Nodes = new();
        this.Wires = new();
        this.SelectedNodes = new();
        this.SelectedWireSegments = new();
    }

    public void Step()
    {
        this.Scheduler.Step();
    }

    public void AddNode(Node node)
    {
        this.Nodes.Add(node);
        this.Scheduler.AddNode(node);
        node.Register(this.Scheduler);

        this.RecalculateWirePositions();
    }

    public void RemoveNode(Node node)
    {
        this.Nodes.Remove(node);
        if (this.SelectedNodes.Contains(node))
        {
            this.SelectedNodes.Remove(node);
        }
        this.Scheduler.RemoveNode(node);

        this.RecalculateWirePositions();
    }

    public void AddWire(Wire wire)
    {
        this.Wires.Add(wire);
        this.RecalculateWirePositions();
    }

    public (Node, string)[] GetPinsConnectedToWire(Wire wire)
    {
        var points = wire.GetLeafPoints();
        var pins = new List<(Node, string)>();

        foreach (var p in points)
        {
            if (this.TryGetPinAtPos(p.ToVector2(Constants.GRIDSIZE), out var node, out var identifier))
            {
                pins.Add((node, identifier));
            }
        }
        return pins.ToArray();
    }

    private void RecalculateConnectionsInScheduler()
    {
        var wires = this.Wires;

        this.Scheduler.ClearConnections();

        foreach (var node in this.Nodes)
        {
            node.Register(this.Scheduler);
        }

        foreach (var wire in wires)
        {
            var pins = this.GetPinsConnectedToWire(wire);

            if (pins.Length > 1)
            {
                var first = pins.First();
                foreach (var pin in pins.Skip(1))
                {
                    this.Scheduler.AddConnection(first.Item1, first.Item2, pin.Item1, pin.Item2);
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

        foreach (var segment in this.SelectedWireSegments)
        {
            Wire.RenderSegmentAsSelected(segment);
        }

        foreach (var wire in this.Wires)
        {
            var pins = this.GetPinsConnectedToWire(wire);

            if (pins.Length > 0)
            {
                var (node, ident) = pins.First();
                var nodePinCollection = this.Scheduler.GetPinCollectionForNode(node);
                var (config, observableValue) = nodePinCollection[ident];

                if (observableValue.Error != ObservableValueError.NONE)
                {
                    wire.Render(Constants.COLOR_ERROR, camera);
                }
                else
                {
                    var values = observableValue.Read();
                    wire.Render(Utilities.GetValueColor(values), camera);
                }
            }
            else
            {
                wire.Render(Constants.COLOR_UNDEFINED, camera);
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
        foreach (var node in circuit.Nodes)
        {
            if (excludeNodes.Contains(node.NodeTypeID))
                continue;

            var c = node.CreateNode();
            sim.AddNode(c);
        }

        foreach (var wire in circuit.Wires)
        {
            sim.AddWire(wire.CreateWire());
        }

        sim.RecalculateWirePositions();
        return sim;
    }

    public Circuit GetCircuitInSimulation(string name)
    {
        return new Circuit(name, this.Nodes, this.Wires);
    }

    public void SetCircuitInSimulation(Circuit circuit)
    {
        this.Nodes.Clear();
        this.Wires.Clear();

        this.SelectedNodes.Clear();
        this.SelectedWireSegments.Clear();

        foreach (var node in circuit.Nodes)
        {
            var c = node.CreateNode();
            this.AddNode(c);
        }

        foreach (var wire in circuit.Wires)
        {
            this.AddWire(wire.CreateWire());
        }

        this.RecalculateWirePositions();
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
        this.SelectedWireSegments.Clear();
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
        this._pickedSegments = this.SelectedWireSegments.ToList();

        this.SelectedNodes.ForEach(s => { this.Nodes.Remove(s); this.Scheduler.RemoveNode(s); });
        this.SelectedWireSegments.ForEach(s => this.DisconnectPoints(s.Item1, s.Item2));

        this.SelectedNodes.Clear();
        this.SelectedWireSegments.Clear();

        this.RecalculateWirePositions();
    }

    public void CommitMovedPickedUpSelection(Vector2i delta)
    {
        this._pickedNodes.ForEach(s => s.Move(delta));
        this._pickedSegments.ForEach(s => this.ConnectPointsWithWire(s.Item1 + delta, s.Item2 + delta));

        this.Nodes.AddRange(this._pickedNodes);
        this._pickedNodes.ForEach(s => this.Scheduler.AddNode(s));

        this.SelectedNodes = this._pickedNodes.ToList();
        this.SelectedWireSegments = this._pickedSegments.Select(s => (s.Item1 + delta, s.Item2 + delta)).ToList();

        this.RecalculateWirePositions();
    }

    public bool HasSelection()
    {
        return this.SelectedNodes.Count > 0 || this.SelectedWireSegments.Count > 0;
    }

    public void SelectWireSegment((Vector2i, Vector2i) segment)
    {
        if (!this.SelectedWireSegments.Contains(segment))
        {
            this.SelectedWireSegments.Add(segment);
        }
    }

    public void DeselectWireSegment((Vector2i, Vector2i) segment)
    {
        if (this.SelectedWireSegments.Contains(segment))
        {
            this.SelectedWireSegments.Remove(segment);
        }
    }

    public void ToggleSelection((Vector2i, Vector2i) segment)
    {
        if (this.SelectedWireSegments.Contains(segment))
        {
            this.SelectedWireSegments.Remove(segment);
        }
        else
        {
            this.SelectedWireSegments.Add(segment);
        }
    }

    public void SelectWireSegmentsInRectangle(RectangleF rectangle)
    {
        this.SelectedWireSegments.Clear();

        foreach (var w in this.Wires)
        {
            foreach (var s in w.Segments)
            {
                if (Utilities.GetSegmentBoundingBox(s).IntersectsWith(rectangle))
                {
                    this.SelectedWireSegments.Add(s);
                }
            }
        }
    }

    public bool IsWireSegmentSelected((Vector2i, Vector2i) segment)
    {
        return this.SelectedWireSegments.Contains(segment);
    }

    public void MoveSelectedWireSegments(Vector2i delta)
    {
        foreach (var s in this.SelectedWireSegments)
        {
            this.DisconnectPoints(s.Item1, s.Item2);
        }

        var selected = this.SelectedWireSegments.ToList();
        this.SelectedWireSegments.Clear();

        foreach (var s in selected)
        {
            this.ConnectPointsWithWire(s.Item1 + delta, s.Item2 + delta);
            this.SelectedWireSegments.Add((s.Item1 + delta, s.Item2 + delta));
        }
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

    public void ConnectPointsWithWire(Vector2i point1, Vector2i point2, bool recalculate = true)
    {
        if (point1 == point2)
        {
            return;
        }

        if (this.TryGetWireAtPos(point1, out var w1))
        {
            if (this.TryGetWireAtPos(point2, out var w2))
            {
                if (w1 == w2)
                {
                    // Same wire? Just add the segment
                    w1.AddSegment(point1, point2);
                }
                else
                {
                    // Different wires? Merge them
                    w1.MergeWith(w2);
                    w1.AddSegment(point1, point2);
                    this.Wires.Remove(w2);
                }
            }
            else
            {
                // Add segment to wire
                w1.AddSegment(point1, point2);
            }
        }
        else
        {
            if (this.TryGetWireAtPos(point2, out var w2))
            {
                // Add segment to wire
                w2.AddSegment(point2, point1);
            }
            else
            {
                // Create new wire
                var wire = new Wire(point1, point2);
                this.AddWire(wire);
            }
        }

        if (recalculate)
            this.RecalculateWirePositions();
    }

    public void DisconnectPoints(Vector2i point1, Vector2i point2)
    {
        if (this.TryGetWireAtPos(point1, out var w1))
        {
            if (this.TryGetWireAtPos(point2, out var w2))
            {
                if (w1 == w2)
                {
                    // Same wire? Remove the segment
                    Wire[] newWires = Wire.RemoveSegmentFromWire(w1, (point1, point2));
                    this.Wires.Remove(w1);
                    this.Wires.AddRange(newWires);
                    this.RecalculateWirePositions();

                    // if (this.SelectedWireSegments.Contains((point1, point2)))
                    // {
                    //     this.SelectedWireSegments.Remove((point1, point2));
                    // }
                }
                else
                {
                    // Should never be different wires? Two points are only connected if they are on the same wire
                    throw new Exception("Two points are only connected if they are on the same wire");
                }
            }
            else
            {
                throw new Exception("No wire at point 2");
            }
        }
        else
        {
            throw new Exception("No wire at point 1");
        }
    }

    public void RecalculateWirePositions()
    {
        this.WirePositions.Clear();
        foreach (var wire in this.Wires)
        {
            foreach (var point in wire.GetPoints())
            {
                this.WirePositions[point] = wire;
            }
        }

        RecalculateConnectionsInScheduler();
    }

    public bool TryGetWireAtPos(Vector2i position, [NotNullWhen(true)] out Wire wire)
    {
        if (this.WirePositions.TryGetValue(position, out var w))
        {
            wire = w;
            return true;
        }

        wire = null;
        return false;
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

    public bool TryGetWireSegmentAtPos(Vector2 worldPosition, out (Vector2i, Vector2i) edge, out Wire wire)
    {
        foreach (var w in this.Wires)
        {
            foreach (var seg in w.Segments)
            {
                var start = seg.Item1;
                var end = seg.Item2;
                var rect = Utilities.GetWireRectangle(start, end);

                if (rect.Contains(worldPosition))
                {
                    edge = seg;
                    wire = w;
                    return true;
                }
            }
        }

        edge = default;
        wire = null;
        return false;
    }
}