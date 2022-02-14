namespace LogiX.Components;

public interface IGateLogic
{
    LogicValue PerformLogic(List<LogicValue> inputs);
    string GetLogicText();
    int DefaultBits();
    int MinBits();
    int MaxBits();
    bool AllowMultibit();
}