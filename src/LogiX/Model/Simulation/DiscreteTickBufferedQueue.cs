using System;
using System.Collections.Generic;
using System.Linq;

namespace LogiX.Model.Simulation;

public class DiscreteTickBufferedQueue<T>
{
    private readonly Queue<Queue<T>> _queues = new();

    public void Enqueue(T item, int offsetFromStart = 0)
    {
        while (_queues.Count <= offsetFromStart + 1)
        {
            _queues.Enqueue(new Queue<T>());
        }

        _queues.ElementAt(offsetFromStart + 1).Enqueue(item);
    }

    public void Pop()
    {
        if (_queues.Count == 0)
            return;

        _queues.Dequeue();
    }

    public IEnumerable<T> DequeueAll()
    {
        var q = _queues.TryPeek(out var queue) ? queue : new();

        while (q.Count > 0)
        {
            yield return q.Dequeue();
        }
    }

    public void RemoveAll(Predicate<T> predicate, int offsetFromStart = 0)
    {
        while (_queues.Count <= offsetFromStart + 1)
        {
            _queues.Enqueue(new Queue<T>());
        }

        var tempQueue = new Queue<T>();
        var queue = _queues.ElementAt(offsetFromStart + 1);

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            if (!predicate(item))
                tempQueue.Enqueue(item);
        }

        queue.Clear();

        while (tempQueue.Count > 0)
        {
            queue.Enqueue(tempQueue.Dequeue());
        }
    }
}
