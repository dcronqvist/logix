using System.Collections.Generic;
using System.Linq;
using NLua;

namespace LogiX.Model.NodeModel;

public class PinCollection : IPinCollection
{
    private readonly Dictionary<string, IReadOnlyCollection<LogicValue>> _pinValues = new();

    public void Write(string pinName, IReadOnlyCollection<LogicValue> values)
    {
        _pinValues[pinName] = values;
    }

    [LuaMember(Name = "read")]
    public IReadOnlyCollection<LogicValue> Read(string pinName)
    {
        if (!_pinValues.ContainsKey(pinName))
            return new List<LogicValue>();

        return _pinValues[pinName];
    }

    [LuaMember(Name = "read_at")]
    public LogicValue Read(string pinName, int index) => Read(pinName).ElementAt(index);
}
