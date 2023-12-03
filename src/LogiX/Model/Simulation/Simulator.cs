using System;
using System.Collections.Generic;
using System.Linq;
using LogiX.Model.Circuits;
using LogiX.Model.NodeModel;

namespace LogiX.Model.Simulation;

public class Simulator : ISimulator
{
    private record NodeEvaluationEvent(Guid Node, PinEvent PinEvent);

    private Dictionary<Guid, Signal> _signals = [];
    private ICircuitDefinition _circuitDefinition;
    private readonly INodeUIHandlerConfigurer _nodeUIHandlerConfigurer;

    private readonly Queue<Guid> _nodesToEvaluateNextStep = new();
    private readonly DiscreteTickBufferedQueue<NodeEvaluationEvent> _nodeEvaluationEventQueue = new();
    private readonly Dictionary<NodePin, Signal> _nodePinToSignal = [];
    private readonly Dictionary<NodePin, IReadOnlyCollection<LogicValue>> _selfGovernedPinValues = [];
    private readonly Dictionary<Signal, List<NodePin>> _signalToNodePins = [];

    public IReadOnlyDictionary<Guid, INode> GetNodes() => _circuitDefinition.GetNodes();
    public IReadOnlyDictionary<Guid, Signal> GetSignals() => _signals;

    public Simulator(ICircuitDefinition initialCircuit, INodeUIHandlerConfigurer nodeUIHandlerConfigurer)
    {
        _nodeUIHandlerConfigurer = nodeUIHandlerConfigurer;
        _circuitDefinition = initialCircuit;
        ResetSimulatedCircuit();
    }

    private void ResetSimulatedCircuit()
    {
        _nodesToEvaluateNextStep.Clear();
        _nodeEvaluationEventQueue.RemoveAll(x => true);
        _nodePinToSignal.Clear();
        _selfGovernedPinValues.Clear();
        _signalToNodePins.Clear();
        _signals = _circuitDefinition.GetSignals().ToDictionary(x => x.Key, x => new Signal());

        foreach (var signalDefinition in _circuitDefinition.GetSignals())
        {
            var signal = _signals[signalDefinition.Key];

            signalDefinition.Value.ConnectedPins
                .ToList()
                .ForEach(x => ConnectPinToSignal(x.NodeID, x.PinID, signal));
        }

        _nodeUIHandlerConfigurer.ClearAllHandlers();

        foreach (var (nodeID, node) in GetNodes())
        {
            var nodeState = _circuitDefinition.GetNodeState(nodeID);

            node.Initialize(nodeState).ToList().ForEach(x => EnqueueEvent(nodeID, x));

            _nodeUIHandlerConfigurer.SetCurrentConfiguringNode(nodeID);
            node.ConfigureUIHandlers(nodeState, _nodeUIHandlerConfigurer);
        }
    }

    public void EnqueueNodeForEvaluationNextStep(Guid nodeID) => _nodesToEvaluateNextStep.Enqueue(nodeID);

    public void EnqueueEvent(Guid originatingNode, PinEvent pinEvent)
    {
        _nodeEvaluationEventQueue.RemoveAll(x => x.Node == originatingNode && x.PinEvent.PinID == pinEvent.PinID);
        _nodeEvaluationEventQueue.Enqueue(new NodeEvaluationEvent(originatingNode, pinEvent), pinEvent.OccursInTicks);
    }

    public void PerformSimulationStep()
    {
        while (_nodeUIHandlerConfigurer.TryGetNextPinEventFrom(out var uiNodeID, out var uiPinEvent))
        {
            EnqueueEvent(uiNodeID, uiPinEvent);
        }

        _nodeEvaluationEventQueue.Pop();

        foreach (var nodeEvaluationEvent in _nodeEvaluationEventQueue.DequeueAll())
        {
            var newEventsAfterApplyingEvent = ApplyPinEvent(nodeEvaluationEvent.Node, nodeEvaluationEvent.PinEvent);

            foreach (var newNodeEvaluationEvent in newEventsAfterApplyingEvent)
                EnqueueEvent(newNodeEvaluationEvent.Node, newNodeEvaluationEvent.PinEvent);
        }

        while (_nodesToEvaluateNextStep.TryDequeue(out var nodeID))
        {
            var nodeInstance = GetNodes()[nodeID];
            var nodeState = _circuitDefinition.GetNodeState(nodeID);
            var pinCollectionForNode = GetPinCollectionForNode(nodeID);
            var newEventsAfterEvaluatingNode = nodeInstance.Evaluate(nodeState, pinCollectionForNode);

            foreach (var newEventAfterEvaluatingNode in newEventsAfterEvaluatingNode)
            {
                EnqueueEvent(nodeID, newEventAfterEvaluatingNode);
            }
        }
    }

    public IPinCollection GetPinCollectionForNode(Guid nodeID)
    {
        var pinCollection = new PinCollection();
        var nodeInstance = GetNodes()[nodeID];
        var nodeState = _circuitDefinition.GetNodeState(nodeID);

        foreach (var pinConfig in nodeInstance.GetPinConfigs(nodeState))
        {
            string pinID = pinConfig.ID;

            var pin = new NodePin(nodeID, pinID);

            if (!_nodePinToSignal.TryGetValue(pin, out var signal))
            {
                if (!_selfGovernedPinValues.TryGetValue(pin, out var selfValues))
                {
                    pinCollection.Write(pinID, LogicValue.UNDEFINED.Repeat(pinConfig.BitWidth));
                    continue;
                }
                else
                {
                    pinCollection.Write(pinID, selfValues);
                    continue;
                }
            }

            var signalValue = signal.GetValue(pinConfig.BitWidth);

            pinCollection.Write(pinID, signalValue);
        }
        return pinCollection;
    }

    public Signal GetSignal(Guid signalID) => _signals[signalID];

    private void ConnectPinToSignal(Guid node, string pinID, Signal signal)
    {
        var nodePin = new NodePin(node, pinID);
        _nodePinToSignal[nodePin] = signal;

        if (!_signalToNodePins.TryGetValue(signal, out var _))
            _signalToNodePins[signal] = [];

        _signalToNodePins[signal].Add(nodePin);
    }

    private List<NodeEvaluationEvent> ApplyPinEvent(Guid originatingNode, PinEvent pinEvent)
    {
        var eventsToReturn = new List<NodeEvaluationEvent>();

        var nodePin = new NodePin(originatingNode, pinEvent.PinID);

        if (!_nodePinToSignal.TryGetValue(nodePin, out var signal))
        {
            _selfGovernedPinValues[nodePin] = pinEvent.NewValues;
            return eventsToReturn;
        }

        signal.Apply(originatingNode, pinEvent.PinID, pinEvent.NewValues);

        var nodePinsConnectedToUpdatedSignal = _signalToNodePins[signal];

        foreach (var nodePinConnectedToUpdatedSignal in nodePinsConnectedToUpdatedSignal)
        {
            if (nodePinConnectedToUpdatedSignal.Equals(nodePin))
                continue;

            var nodeInstance = GetNodes()[nodePinConnectedToUpdatedSignal.NodeID];
            var nodeState = _circuitDefinition.GetNodeState(nodePinConnectedToUpdatedSignal.NodeID);

            var pinConfigForOriginatingPin = nodeInstance.GetPinConfigs(nodeState).First(x => x.ID == nodePinConnectedToUpdatedSignal.PinID);

            if (!pinConfigForOriginatingPin.UpdateCausesEvaluation)
                continue;

            var pinCollectionForNode = GetPinCollectionForNode(nodePinConnectedToUpdatedSignal.NodeID);
            var newEventsAfterEvaluatingNode = nodeInstance.Evaluate(nodeState, pinCollectionForNode);
            eventsToReturn.AddRange(newEventsAfterEvaluatingNode.Select(x => new NodeEvaluationEvent(nodePinConnectedToUpdatedSignal.NodeID, x)));
        }

        return eventsToReturn;
    }

    public void OnCompleted()
    {
        // Do nothing
    }

    public void OnError(Exception error)
    {
        // Do nothing
    }

    public void OnNext(ICircuitDefinition value)
    {
        _circuitDefinition = value;
        ResetSimulatedCircuit();
    }
}
