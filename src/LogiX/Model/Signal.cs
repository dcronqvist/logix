using System;
using System.Collections.Generic;
using System.Linq;
using LogiX.Model.NodeModel;

namespace LogiX.Model.Simulation;

public class Signal
{
    private readonly Dictionary<NodePin, IReadOnlyCollection<LogicValue>> _pinValues = [];

    internal void Apply(Guid originator, string originatorPinID, IReadOnlyCollection<LogicValue> newValues)
    {
        var nodePin = new NodePin(originator, originatorPinID);
        _pinValues[nodePin] = newValues;
    }

    public bool HasError(int expectedBitWidth) => !TryGetValuesAgree(expectedBitWidth, _pinValues.Values, out var _);

    public IReadOnlyCollection<LogicValue> GetValue(int expectedBitWidth)
    {
        TryGetValuesAgree(expectedBitWidth, _pinValues.Values, out var result);
        return result;
    }

    public bool TryGetBitWidth(out int bitWidth)
    {
        if (_pinValues.Count == 0)
        {
            bitWidth = 0;
            return true;
        }

        int firstBitWidth = _pinValues.First().Value.Count;
        bitWidth = firstBitWidth;
        return _pinValues.Values.All(x => x.Count == firstBitWidth);
    }

    private static bool TryGetValuesAgree(int expectedBitWidth, IEnumerable<IReadOnlyCollection<LogicValue>> values, out IReadOnlyCollection<LogicValue> result)
    {
        result = LogicValue.UNDEFINED.Repeat(expectedBitWidth);
        var finalValues = new LogicValue[expectedBitWidth];
        int length = values.Count();

        if (length == 0)
            return true;

        bool mismatch = false;

        for (int i = 0; i < expectedBitWidth; i++)
        {
            var bitValues = values.Select(x => x.ElementAt(i)).ToList();

            if (bitValues.All(x => x == LogicValue.UNDEFINED))
            {
                finalValues[i] = LogicValue.UNDEFINED;
                continue;
            }

            var defined = bitValues.Where(x => x != LogicValue.UNDEFINED).ToList();

            if (AllSame(defined))
            {
                finalValues[i] = defined.First();
                continue;
            }

            mismatch = true;
            finalValues[i] = LogicValue.UNDEFINED;
        }

        result = finalValues.ToList().AsReadOnly();
        return !mismatch;
    }

    private static bool AllSame(IReadOnlyCollection<LogicValue> values)
    {
        if (values.Count == 0)
            return true;

        var value = values.First();

        foreach (var v in values)
        {
            if (v != value)
                return false;
        }

        return true;
    }
}
