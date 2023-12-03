using System;
using System.Collections.Generic;
using System.Linq;
using LogiX.Graphics;

namespace LogiX.Model;

public enum LogicValue : int
{
    UNDEFINED = -1,
    LOW = 0,
    HIGH = 1
}

public static class LogicValueExtensions
{
    public static bool IsZ(this LogicValue value) => value == LogicValue.UNDEFINED;

    public static ColorF GetValueColor(this LogicValue value) => value switch
    {
        LogicValue.LOW => ColorF.Darken(ColorF.SoftGreen, 0.3f),
        LogicValue.HIGH => ColorF.SoftGreen,
        LogicValue.UNDEFINED => ColorF.BlueGray,
        _ => throw new NotImplementedException($"There is no color for {value}")
    };

    public static ColorF GetValueColor(this IReadOnlyCollection<LogicValue> values)
    {
        if (values.All(v => v == LogicValue.UNDEFINED))
        {
            return ColorF.BlueGray;
        }

        int highs = values.Count(v => v == LogicValue.HIGH);
        int total = values.Count;

        return ColorF.Lerp(ColorF.Darken(ColorF.SoftGreen, 0.3f), ColorF.SoftGreen, highs / (float)total);
    }

    public static IReadOnlyCollection<LogicValue> Repeat(this LogicValue value, int count)
    {
        var result = new List<LogicValue>(count);

        for (int i = 0; i < count; i++)
        {
            result.Add(value);
        }

        return result;
    }

    public static bool GetAsBool(this LogicValue value) => value switch
    {
        LogicValue.LOW => false,
        LogicValue.HIGH => true,
        LogicValue.UNDEFINED => throw new ArgumentException("Value is not a valid LogicValue for converting to a bool", nameof(value)),
        _ => throw new ArgumentException("Value is not a valid LogicValue for converting to a bool", nameof(value))
    };

    public static LogicValue GetAsLogicValue(this bool value) => value switch
    {
        false => LogicValue.LOW,
        true => LogicValue.HIGH
    };

    public static byte GetAsByte(this LogicValue value) => value switch
    {
        LogicValue.LOW => 0,
        LogicValue.HIGH => 1,
        LogicValue.UNDEFINED => throw new ArgumentException("Value is not a valid LogicValue for converting to a byte", nameof(value)),
        _ => throw new ArgumentException("Value is not a valid LogicValue for converting to a byte", nameof(value))
    };

    public static LogicValue GetAsLogicValue(this byte value) => value switch
    {
        0 => LogicValue.LOW,
        1 => LogicValue.HIGH,
        _ => throw new ArgumentException("Value is not a valid byte for converting to a LogicValue", nameof(value))
    };

    public static int GetAsInt(this LogicValue value) => value switch
    {
        LogicValue.LOW => 0,
        LogicValue.HIGH => 1,
        LogicValue.UNDEFINED => throw new ArgumentException("Value is not a valid LogicValue for converting to an int", nameof(value)),
        _ => throw new ArgumentException("Value is not a valid LogicValue for converting to an int", nameof(value))
    };

    public static LogicValue GetAsLogicValue(this int value) => value switch
    {
        0 => LogicValue.LOW,
        1 => LogicValue.HIGH,
        _ => throw new ArgumentException("Value is not a valid int for converting to a LogicValue", nameof(value))
    };

    /// <summary>
    /// Converts a collection of LogicValues to a byte. The first element in the collection will be the least significant bit.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static byte GetAsByte(this IReadOnlyCollection<LogicValue> values)
    {
        byte result = 0;

        for (int i = 0; i < values.Count; i++)
        {
            result |= (byte)(values.ElementAt(i).GetAsByte() << i);
        }

        return result;
    }

    /// <summary>
    /// Converts a byte to a collection of LogicValues. The least significant bit will be the first element in the collection.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IReadOnlyCollection<LogicValue> GetAsLogicValues(this byte value)
    {
        var result = new List<LogicValue>(8);

        for (int i = 0; i < 8; i++)
        {
            result.Add(((value >> i) & 1).GetAsLogicValue());
        }

        return result;
    }

    /// <summary>
    /// Converts a collection of LogicValues to an int. The first element in the collection will be the least significant bit.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static int GetAsInt(this IReadOnlyCollection<LogicValue> values)
    {
        int result = 0;

        for (int i = 0; i < values.Count; i++)
        {
            result |= values.ElementAt(i).GetAsInt() << i;
        }

        return result;
    }

    /// <summary>
    /// Converts an int to a collection of LogicValues. The least significant bit will be the first element in the collection.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IReadOnlyCollection<LogicValue> GetAsLogicValues(this int value)
    {
        var result = new List<LogicValue>(32);

        for (int i = 0; i < 32; i++)
        {
            result.Add(((value >> i) & 1).GetAsLogicValue());
        }

        return result;
    }
}
