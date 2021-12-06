namespace LogiX.Components;

public interface IGateLogic
{
    LogicValue PerformLogic(List<ComponentInput> inputs);
    string GetLogicText();
}