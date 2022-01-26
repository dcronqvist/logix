namespace LogiX.Components;

public class NOTLogic : IGateLogic
{
    public LogicValue PerformLogic(List<ComponentInput> inputs)
    {
        return inputs[0].Values[0] == LogicValue.LOW ? LogicValue.HIGH : LogicValue.LOW;
    }

    public string GetLogicText() => "NOT";
    public int DefaultBits() => 1;
    public int MinBits() => 1;
    public int MaxBits() => 1;
    public bool AllowMultibit() => false;
}