namespace LogiX.Components;

public class ORLogic : IGateLogic
{
    public LogicValue PerformLogic(List<LogicValue> inputs)
    {
        foreach (LogicValue lv in inputs)
        {
            if (lv == LogicValue.HIGH)
            {
                return LogicValue.HIGH;
            }
        }

        return LogicValue.LOW;
    }

    public string GetLogicText() => "OR";
    public int DefaultBits() => 2;
    public int MinBits() => 2;
    public int MaxBits() => int.MaxValue;
    public bool AllowMultibit() => true;
}