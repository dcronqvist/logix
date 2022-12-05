namespace LogiX.Architecture;


public class ObservableValue : Observable
{
    private Node _lastNodeThatSet;
    private LogicValue[] _values = new LogicValue[0];
    private int _bits = 0;

    public ObservableValue(int bits, LogicValue[] initialValue = null)
    {
        this._bits = bits;
        this._lastNodeThatSet = null;
        this._values = initialValue ?? LogicValue.Z.Multiple(bits);
    }

    public LogicValue[] Read()
    {
        return this._values.ToArray();
    }

    public void Set(Node originator, LogicValue[] values)
    {
        if (values.Length != this._bits)
        {
            throw new ArgumentException("Value length does not match observable value length");
        }

        for (int i = 0; i < values.Length; i++)
        {
            var newValue = values[i];
            var currValue = this._values[i];

            if (originator != this._lastNodeThatSet)
            {
                if (currValue != LogicValue.Z && newValue != currValue)
                {
                    throw new ArgumentException("Cannot set observable value to a different value than the current value from a different node. This would cause a short circuit");
                }
            }
        }

        if (!values.SequenceEqual(this._values))
        {
            this._values = values;
            this._lastNodeThatSet = originator;
            this.NotifyObservers();
        }
    }
}