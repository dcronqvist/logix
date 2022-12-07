namespace LogiX.Architecture;

public class ValueEvent
{
    public Node Originator { get; set; }
    public ObservableValue AffectedValue { get; set; }
    public LogicValue[] NewValues { get; set; }

    public ValueEvent(Node originator, ObservableValue affectedValue, LogicValue[] newValues)
    {
        this.Originator = originator;
        this.AffectedValue = affectedValue;
        this.NewValues = newValues.ToArray();
    }
}

public class Scheduler
{
    public Queue<LinkedList<ValueEvent>> EventQueue { get; set; } = new();
    public List<Node> Nodes { get; set; } = new();
    public List<(Node, string, Node, string)> NodePinConnections { get; set; } = new();
    public Dictionary<Node, PinCollection> NodePins { get; set; } = new();

    public void AddNode(Node node, bool prepare = true)
    {
        this.Nodes.Add(node);

        if (prepare)
            this.Prepare();
    }

    public void RemoveNode(Node node, bool prepare = true)
    {
        this.Nodes.Remove(node);

        if (prepare)
            this.Prepare();
    }

    public void ClearConnections()
    {
        this.NodePinConnections.Clear();
    }

    public void AddConnection(Node n1, string i1, Node n2, string i2, bool prepare = true)
    {
        this.NodePinConnections.Add((n1, i1, n2, i2));

        if (prepare)
            this.Prepare();
    }

    private List<List<(Node, string)>> GetConnectedPins(List<(Node, string, Node, string)> connections)
    {
        // Assume all values in connections to be edges of a graph. We seek to construct all connected components of this graph.
        // We do this by using a breadth-first search.

        var connectedPorts = new List<List<(Node, string)>>();

        var visited = new HashSet<(Node, string)>();
        var queue = new Queue<(Node, string)>();

        foreach (var (n, p, _, _) in connections)
        {
            if (visited.Contains((n, p)))
            {
                continue;
            }

            var connectedPortsComponent = new List<(Node, string)>();
            queue.Enqueue((n, p));

            while (queue.Count > 0)
            {
                var (node, port) = queue.Dequeue();
                if (visited.Contains((node, port)))
                {
                    continue;
                }

                visited.Add((node, port));
                connectedPortsComponent.Add((node, port));

                foreach (var (n1, p1, n2, p2) in connections)
                {
                    if (n1 == node && p1 == port)
                    {
                        queue.Enqueue((n2, p2));
                    }
                    else if (n2 == node && p2 == port)
                    {
                        queue.Enqueue((n1, p1));
                    }
                }
            }

            connectedPorts.Add(connectedPortsComponent);
        }

        return connectedPorts;
    }

    public void Prepare()
    {
        var currPins = this.NodePins.ToDictionary(x => x.Key, x => x.Value);
        this.NodePins.Clear();
        this.Nodes.ForEach(n => n.SetScheduler(this));

        foreach (var connections in this.GetConnectedPins(this.NodePinConnections))
        {
            var anyNodePin = connections.First();
            var anyConfig = anyNodePin.Item1.GetPinConfiguration().First(c => c.Identifier == anyNodePin.Item2);
            var value = new ObservableValue(anyConfig.Bits);

            foreach (var (node, port) in connections)
            {
                var nodePortConfig = node.GetPinConfiguration().ToArray();
                var config = nodePortConfig.First(c => c.Identifier == port);

                if (!this.NodePins.ContainsKey(node))
                {
                    this.NodePins.Add(node, new PinCollection(nodePortConfig));
                }

                this.NodePins[node].SetObservableValue(port, value);

                if (config.EvaluateOnValueChange)
                {
                    value.AddObserver(node);
                }
            }

            // TODO: This should make sure that all connected pins have the same bit width, if not, then throw an exception.
            foreach (var (node, port) in connections)
            {
                var nodePortConfig = node.GetPinConfiguration().ToArray();
                var config = nodePortConfig.First(c => c.Identifier == port);

                if (config.Bits != anyConfig.Bits)
                {
                    var v = this.NodePins[node].Get(port);
                    v.Error = ObservableValueError.PIN_WIDTHS_MISMATCH;
                }
            }
        }

        foreach (var node in this.Nodes)
        {
            var nodePortConfig = node.GetPinConfiguration().ToArray();

            if (!this.NodePins.ContainsKey(node))
            {
                this.NodePins.Add(node, new PinCollection(nodePortConfig));
            }

            for (int i = 0; i < nodePortConfig.Length; i++)
            {
                var config = nodePortConfig[i];

                if (this.NodePins[node].Get(config.Identifier) is null)
                {
                    this.NodePins[node].SetObservableValue(config.Identifier, new ObservableValue(config.Bits));

                    if (config.EvaluateOnValueChange)
                    {
                        this.NodePins[node].Get(config.Identifier).AddObserver(node);
                    }
                }
            }

            node.Prepare();
        }
    }

    public void Schedule(Node originator, ObservableValue value, LogicValue[] newValues, int time)
    {
        var valueEvent = new ValueEvent(originator, value, newValues);

        while (this.EventQueue.Count < time)
        {
            this.EventQueue.Enqueue(new LinkedList<ValueEvent>());
        }

        if (this.EventQueue.ElementAt(time - 1).Any(x => x.AffectedValue == value))
        {
            // Remove the old event and replace it with the new one.
            this.EventQueue.ElementAt(time - 1).Remove(this.EventQueue.ElementAt(time - 1).First(x => x.AffectedValue == value));
        }

        this.EventQueue.ElementAt(time - 1).AddLast(valueEvent);
    }

    public int Step()
    {
        int eventsExecuted = 0;
        if (this.EventQueue.TryDequeue(out var events))
        {
            foreach (var e in events)
            {
                e.AffectedValue.Set(e.Originator, e.NewValues);
                eventsExecuted++;
            }
        }
        return eventsExecuted;
    }

    public PinCollection GetPinCollectionForNode(Node node)
    {
        if (this.NodePins.ContainsKey(node))
        {
            return this.NodePins[node];
        }

        return null;
    }
}