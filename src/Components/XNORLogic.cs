namespace LogiX.Components;

public class XNORLogic : IGateLogic
{
    public LogicValue PerformLogic(List<LogicValue> inputs)
    {
        int inputsHigh = 0;

        foreach (LogicValue lv in inputs)
        {
            if (lv == LogicValue.HIGH)
            {
                inputsHigh++;
            }
        }

        return inputsHigh % 2 == 1 ? LogicValue.LOW : LogicValue.HIGH;
    }

    public string GetLogicText() => "XNOR";
    public int DefaultBits() => 2;
    public int MinBits() => 2;
    public int MaxBits() => int.MaxValue;
    public bool AllowMultibit() => true;
}