using LogiX.Model.Simulation;
using System;
using System.Collections.Generic;

namespace LogiX.Model.NodeModel;

public interface INodeUIEventProvider<T>
{
    void Subscribe(Func<T, IEnumerable<PinEvent>> action);
    IEnumerable<(Guid, PinEvent)> NotifySubscribers(T value);
}
