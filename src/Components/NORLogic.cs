namespace LogiX.Components;

public class NORLogic : IGateLogic
{
    public LogicValue PerformLogic(List<LogicValue> inputs)
    {
        foreach (LogicValue lv in inputs)
        {
            if (lv == LogicValue.HIGH)
            {
                return LogicValue.LOW;
            }
        }

        return LogicValue.HIGH;
    }

    public string GetLogicText() => "NOR";
    public int DefaultBits() => 2;
    public int MinBits() => 2;
    public int MaxBits() => int.MaxValue;
    public bool AllowMultibit() => true;
}