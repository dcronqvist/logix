namespace LogiX.Components;

public class XORLogic : IGateLogic
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

        return inputsHigh % 2 == 1 ? LogicValue.HIGH : LogicValue.LOW;
    }

    public string GetLogicText() => "XOR";
    public int DefaultBits() => 2;
    public int MinBits() => 2;
    public int MaxBits() => int.MaxValue;
    public bool AllowMultibit() => true;
}