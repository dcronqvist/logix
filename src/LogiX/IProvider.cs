using System;

namespace LogiX;

public interface IProvider<out T>
{
    T Get();
}

public class FixedValue<T>(T value) : IProvider<T>
{
    public T Get() => value;
}

public class ComputedValue<T>(Func<T> compute) : IProvider<T>
{
    public T Get() => compute();
}

public class SettableValue<T>(T initialValue) : IProvider<T>
{
    private T _value = initialValue;

    public T Get() => _value;

    public void Set(T value) => _value = value;
}
