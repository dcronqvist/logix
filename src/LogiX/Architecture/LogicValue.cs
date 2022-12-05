namespace LogiX;

public enum LogicValue
{
    Z = -1,
    LOW = 0,
    HIGH = 1,
}

public static class LogicValueExtension
{
    public static int ToInt(this LogicValue value)
    {
        return (int)value;
    }

    public static LogicValue ToLogicValue(this bool b)
    {
        return b ? LogicValue.HIGH : LogicValue.LOW;
    }
}