using LogiX.Model.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogiX.Model.NodeModel;

public class NodeUIEventProvider<T>(Guid nodeID) : INodeUIEventProvider<T>
{
    private readonly List<Func<T, IEnumerable<PinEvent>>> _subscribers = [];

    public IEnumerable<(Guid, PinEvent)> NotifySubscribers(T value) => _subscribers
        .SelectMany(s => (s(value) ?? []).Select(e => (nodeID, e)));

    public void Subscribe(Func<T, IEnumerable<PinEvent>> action) => _subscribers.Add(action);
}
