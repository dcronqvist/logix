using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using LogiX.Graphics;
using LogiX.Model.NodeModel;
using LogiX.Observables;

namespace LogiX.Model.Circuits;

public record CircuitSignalDefinition(IEnumerable<NodePin> ConnectedPins);

public record SignalSegment(Vector2i Start, Vector2i End);

[JsonDerivedType(typeof(CircuitDefinition), "circuit-def")]
public interface ICircuitDefinition : IObservable<ICircuitDefinition>
{
    IReadOnlyDictionary<Guid, INode> GetNodes();
    IReadOnlyDictionary<Guid, CircuitSignalDefinition> GetSignals();
    IReadOnlyCollection<SignalSegment> GetSignalSegments(Guid signalID);

    Vector2i GetNodePosition(Guid nodeID);
    int GetNodeRotation(Guid nodeID);
    INodeState GetNodeState(Guid nodeID);
    TState GetNodeState<TState>(Guid nodeID) where TState : INodeState => (TState)GetNodeState(nodeID);

    void SetNodes(IReadOnlyDictionary<Guid, INode> nodes);
    void SetSignals(IReadOnlyDictionary<Guid, CircuitSignalDefinition> signals);
    void ClearSignalSegmentsForAllSignals();
    void SetSignalSegments(Guid signalID, IReadOnlyCollection<SignalSegment> segments);

    void SetNodePosition(Guid nodeID, Vector2i position);
    void SetNodeRotation(Guid nodeID, int rotation);
    void SetNodeState(Guid nodeID, INodeState state);

    ICircuitDefinition Clone();
    void Set(ICircuitDefinition circuitDefinition);

    void NotifyObservers();
}

public class CircuitDefinition : Observable<ICircuitDefinition>, ICircuitDefinition
{
    public CircuitDefinition() { }
    public CircuitDefinition(ICircuitDefinition initialCircuitDefinition) => Set(initialCircuitDefinition);

    private Dictionary<Guid, INode> _nodes = [];
    public IReadOnlyDictionary<Guid, INode> Nodes { get => _nodes; init => _nodes = value.ToDictionary(); }

    private Dictionary<Guid, CircuitSignalDefinition> _signals = [];
    public IReadOnlyDictionary<Guid, CircuitSignalDefinition> Signals { get => _signals; init => _signals = value.ToDictionary(); }

    private Dictionary<Guid, IReadOnlyCollection<SignalSegment>> _signalSegments = [];
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<SignalSegment>> SignalSegments { get => _signalSegments; init => _signalSegments = value.ToDictionary(); }

    private Dictionary<Guid, Vector2i> _nodePositions = [];
    public IReadOnlyDictionary<Guid, Vector2i> NodePositions { get => _nodePositions; init => _nodePositions = value.ToDictionary(); }

    private Dictionary<Guid, int> _nodeRotations = [];
    public IReadOnlyDictionary<Guid, int> NodeRotations { get => _nodeRotations; init => _nodeRotations = value.ToDictionary(); }

    private Dictionary<Guid, INodeState> _nodeStates = [];
    public IReadOnlyDictionary<Guid, INodeState> NodeStates { get => _nodeStates; init => _nodeStates = value.ToDictionary(); }

    public IReadOnlyDictionary<Guid, INode> GetNodes() => _nodes;
    public IReadOnlyDictionary<Guid, CircuitSignalDefinition> GetSignals() => _signals;
    public IReadOnlyCollection<SignalSegment> GetSignalSegments(Guid signalID) => _signalSegments.TryGetValue(signalID, out var segments) ? segments : [];

    public Vector2i GetNodePosition(Guid nodeID) => _nodePositions[nodeID];
    public int GetNodeRotation(Guid nodeID) => _nodeRotations[nodeID];
    public INodeState GetNodeState(Guid nodeID) => _nodeStates[nodeID];

    public void SetNodes(IReadOnlyDictionary<Guid, INode> nodes) => _nodes = nodes.ToDictionary();
    public void SetSignals(IReadOnlyDictionary<Guid, CircuitSignalDefinition> signals) => _signals = signals.ToDictionary();
    public void SetSignalSegments(Guid signalID, IReadOnlyCollection<SignalSegment> segments) => _signalSegments[signalID] = segments;
    public void ClearSignalSegmentsForAllSignals() => _signalSegments.Clear();

    public void SetNodePosition(Guid nodeID, Vector2i position) => _nodePositions[nodeID] = position;
    public void SetNodeRotation(Guid nodeID, int rotation) => _nodeRotations[nodeID] = rotation;
    public void SetNodeState(Guid nodeID, INodeState state) => _nodeStates[nodeID] = state;

    public ICircuitDefinition Clone() => new CircuitDefinition(this);
    public void Set(ICircuitDefinition circuitDefinition)
    {
        SetNodes(circuitDefinition.GetNodes());
        SetSignals(circuitDefinition.GetSignals());

        foreach (var (nodeID, _) in circuitDefinition.GetNodes().ToDictionary())
            SetNodePosition(nodeID, circuitDefinition.GetNodePosition(nodeID));

        foreach (var (nodeID, _) in circuitDefinition.GetNodes().ToDictionary())
            SetNodeRotation(nodeID, circuitDefinition.GetNodeRotation(nodeID));

        foreach (var (nodeID, _) in circuitDefinition.GetNodes().ToDictionary())
            SetNodeState(nodeID, circuitDefinition.GetNodeState(nodeID).Clone());
    }

    public void NotifyObservers() => NotifyObservers(this);
}
