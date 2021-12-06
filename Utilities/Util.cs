using LogiX.Components;

namespace LogiX;

public static class Util
{
    public static List<LogicValue> NValues(LogicValue value, int n)
    {
        return Enumerable.Repeat(value, n).ToList();
    }

    public static List<T> Listify<T>(params T[] args)
    {
        return args.ToList();
    }
}