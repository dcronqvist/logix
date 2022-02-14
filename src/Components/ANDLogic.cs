namespace LogiX.Components;

public class ANDLogic : IGateLogic
{
    public LogicValue PerformLogic(List<LogicValue> inputs)
    {
        foreach (LogicValue lv in inputs)
        {
            if (lv == LogicValue.LOW)
            {
                return LogicValue.LOW;
            }
        }

        return LogicValue.HIGH;
    }

    public string GetLogicText() => "AND";
    public int DefaultBits() => 2;
    public int MinBits() => 2;
    public int MaxBits() => int.MaxValue;
    public bool AllowMultibit() => true;
}