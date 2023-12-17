using System.Collections.Generic;
using System.Linq;

namespace LogiX.Model.NodeModel;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class PinCollection : IPinCollection
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    private readonly Dictionary<string, IReadOnlyCollection<LogicValue>> _pinValues = [];

    public void Write(string pinName, IReadOnlyCollection<LogicValue> values) => _pinValues[pinName] = values;

    public IReadOnlyCollection<LogicValue> Read(string pinID)
    {
        if (!_pinValues.TryGetValue(pinID, out var value))
            return new List<LogicValue>();

        return value;
    }

    public LogicValue Read(string pinID, int index) => Read(pinID).ElementAt(index);
}
