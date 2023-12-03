using System;
using System.Collections.Generic;
using LogiX.Model.Circuits;
using LogiX.Model.NodeModel;

namespace LogiX.Model.Simulation;

public interface ISimulator : IObserver<ICircuitDefinition>
{
    IReadOnlyDictionary<Guid, INode> GetNodes();
    IReadOnlyDictionary<Guid, Signal> GetSignals();

    void PerformSimulationStep();
    void EnqueueEvent(Guid originatingNode, PinEvent pinEvent);
    void EnqueueNodeForEvaluationNextStep(Guid nodeID);
    IPinCollection GetPinCollectionForNode(Guid nodeID);
    Signal GetSignal(Guid signalID);
}
