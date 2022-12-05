namespace LogiX.Architecture;

public struct PinConfig
{
    public string Identifier { get; set; }
    public int Bits { get; set; }
    public bool EvaluateOnValueChange { get; set; }
    public Vector2i Offset { get; set; }

    public PinConfig(string identifier, int bits, bool evaluateOnValueChange, Vector2i offset)
    {
        this.Identifier = identifier;
        this.Bits = bits;
        this.EvaluateOnValueChange = evaluateOnValueChange;
        this.Offset = offset;
    }
}

public class PinCollection : Dictionary<string, (PinConfig, ObservableValue)>
{
    public PinCollection(IEnumerable<PinConfig> pinConfigs)
    {
        foreach (var pinConfig in pinConfigs)
        {
            this.Add(pinConfig.Identifier, (pinConfig, null));
        }
    }

    public IEnumerable<ObservableValue> GetObservableValues()
    {
        return this.Values.Select(v => v.Item2);
    }

    public ObservableValue Get(string identifier)
    {
        return this[identifier].Item2;
    }

    public void SetObservableValue(string identifier, ObservableValue value)
    {
        this[identifier] = (this[identifier].Item1, value);
    }

    public PinConfig GetConfig(string identifier)
    {
        return this[identifier].Item1;
    }
}