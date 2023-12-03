using System.Collections.Generic;

namespace LogiX.Model.NodeModel;

public class PinCollection : IPinCollection
{
    private readonly Dictionary<string, IReadOnlyCollection<LogicValue>> _pinValues = new();

    public void Write(string pinName, IReadOnlyCollection<LogicValue> values)
    {
        _pinValues[pinName] = values;
    }

    public IReadOnlyCollection<LogicValue> Read(string pinName)
    {
        if (!_pinValues.ContainsKey(pinName))
            return new List<LogicValue>();

        return _pinValues[pinName];
    }
}
