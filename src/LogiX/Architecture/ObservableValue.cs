namespace LogiX.Architecture;

public enum ObservableValueError
{
    NONE,
    PIN_WIDTHS_MISMATCH,
    VALUES_MISMATCH,
}

public class ObservableValue : Observable<IEnumerable<(ValueEvent, int)>>
{
    private int _bits = 0;

    public Dictionary<Node, LogicValue[]> _setValues = new();
    public ObservableValueError Error { get; set; } = ObservableValueError.NONE;

    public ObservableValue(int bits)
    {
        this._bits = bits;
    }

    private LogicValue[] _values = null;
    public LogicValue[] Read()
    {
        if (this._values is null)
        {
            this.Error = this.GetValuesAgree(this._setValues.Values.ToList(), out this._values);
        }
        return _values;
    }

    private ObservableValueError GetValuesAgree(List<LogicValue[]> values, out LogicValue[] result)
    {
        // Must consider that the values are not all the same and
        // There may be a conflict.
        // First assume that all values are Z, and then attempt
        // to work out the values from the values parameter
        // If there are values that are not Z, then they must be the same
        // and they will become the final value since Z does not contribute

        result = LogicValue.Z.Multiple(this._bits);

        if (values.Count == 0)
        {
            return ObservableValueError.NONE;
        }

        bool mismatchValue = false;

        for (int i = 0; i < this._bits; i++)
        {
            var bitValues = values.Select(x => x[i]);

            if (bitValues.All(x => x == LogicValue.Z))
            {
                result[i] = LogicValue.Z;
                continue;
            }

            var nonZ = bitValues.Where(x => x != LogicValue.Z);

            if (nonZ.AllSame())
            {
                result[i] = nonZ.First();
                continue;
            }

            result[i] = LogicValue.Z;
            mismatchValue = true;
        }

        if (mismatchValue)
            result = LogicValue.Z.Multiple(this._bits);

        return mismatchValue ? ObservableValueError.VALUES_MISMATCH : ObservableValueError.NONE;
    }

    private void PushValuesAsNode(Node node, LogicValue[] values)
    {

    }

    public IEnumerable<(ValueEvent, int)> Set(Node originator, LogicValue[] values)
    {
        if (values.Length != this._bits)
        {
            yield break;
        }

        var oldVal = this.Read();
        var oldError = this.Error;

        if (!this._setValues.ContainsKey(originator))
        {
            this._setValues.Add(originator, values);
            this._values = null;
        }
        else
        {
            if (!this._setValues[originator].SequenceEqual(values))
            {
                this._values = null;
            }

            this._setValues[originator] = values;
        }

        var newVal = this.Read();
        var newError = this.Error;

        if (!newVal.SequenceEqual(oldVal) || oldError != newError)
        {
            var b = this.NotifyObservers().SelectMany(x => x.ToArray());
            foreach (var (valueEvent, time) in b)
            {
                yield return (valueEvent, time);
            }
        }
    }
}