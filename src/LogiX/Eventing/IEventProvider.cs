using System;
using System.Collections.Generic;

namespace LogiX.Eventing;

public interface IEventProvider<T>
{
    Guid Subscribe(Action<T> action);
    void NotifySubscribers(T value);
    void Unsubscribe(Guid subscriptionID);
}

public class EventProvider<T> : IEventProvider<T>
{
    private readonly List<(Guid, Action<T>)> _subscribers = [];

    public Guid Subscribe(Action<T> action)
    {
        var subscriptionID = Guid.NewGuid();
        _subscribers.Add((subscriptionID, action));
        return subscriptionID;
    }

    public void NotifySubscribers(T value) => _subscribers.ForEach(s => s.Item2(value));
    public void Unsubscribe(Guid subscriptionID) => _subscribers.RemoveAll(s => s.Item1 == subscriptionID);
}
