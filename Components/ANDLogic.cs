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
}