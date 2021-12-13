namespace LogiX.Components;

public class NANDLogic : IGateLogic
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
                    return LogicValue.HIGH;
                }
            }
        }

        return LogicValue.LOW;
    }

    public string GetLogicText() => "NAND";
    public int DefaultBits() => 2;
    public int MinBits() => 2;
    public int MaxBits() => int.MaxValue;
    public bool AllowMultibit() => true;
}