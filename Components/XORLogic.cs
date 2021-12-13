namespace LogiX.Components;

public class XORLogic : IGateLogic
{
    public LogicValue PerformLogic(List<ComponentInput> inputs)
    {
        int inputsHigh = 0;

        for (int i = 0; i < inputs.Count; i++)
        {
            ComponentInput ci = inputs[i];

            for (int j = 0; j < ci.Bits; j++)
            {
                if (ci.Values[j] == LogicValue.HIGH)
                {
                    inputsHigh += 1;
                }
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