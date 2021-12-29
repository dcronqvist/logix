namespace LogiX.Components;

public class ANDLogic : IGateLogic
{
    public LogicValue PerformLogic(List<ComponentInput> inputs)
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            ComponentInput ci = inputs[i];

            for (int j = 0; j < ci.Bits; j++)
            {
                if (ci.Values[j] == LogicValue.LOW)
                {
                    return LogicValue.LOW;
                }
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