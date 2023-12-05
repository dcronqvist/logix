using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using LogiX.Graphics;
using NLua;

namespace LogiX.Scripting;

public static class LuaExtensions
{
    public static LuaTable ToLuaTable<T>(this IReadOnlyCollection<T> array, Lua lua)
    {
        lua.State.CreateTable(0, array.Count);

        int i = 1;
        foreach (var item in array)
        {
            lua.State.PushInteger(i++);
            lua.Push(item);
            lua.State.SetTable(-3);
        }

        return lua.Pop() as LuaTable;
    }

    public static T[] ParseLuaTableAsArrayOf<T>(this LuaTable table) where T : new()
    {
        var type = typeof(T);
        return (T[])ParseLuaTableAs(typeof(T[]), table);
    }

    public static T ParseLuaTableAs<T>(this LuaTable table) where T : new()
    {
        var type = typeof(T);
        return (T)ParseLuaTableAs(type, table);
    }

    private static object ParseLuaTableAs(Type targetType, LuaTable obj)
    {
        if (targetType == typeof(Vector2i) && obj.Keys.Cast<object>().All(x => x is long) && obj.Keys.Count == 2)
        {
            var x = FixValue(typeof(int), obj[1]);
            var y = FixValue(typeof(int), obj[2]);

            return new Vector2i((int)x, (int)y);
        }
        if (targetType == typeof(Vector2) && obj.Keys.Cast<object>().All(x => x is long) && obj.Keys.Count == 2)
        {
            var x = FixValue(typeof(float), obj[1]);
            var y = FixValue(typeof(float), obj[2]);

            return new Vector2((float)x, (float)y);
        }

        if (targetType.IsArray)
        {
            // Get the array type
            var arrayType = targetType.GetElementType();

            // Get the array length
            int length = obj.Keys.Count;

            // Create the array
            var array = Array.CreateInstance(arrayType, length);

            // Loop through the array
            for (int i = 0; i < length; i++)
            {
                // Get the value
                object value = obj[i + 1];

                // Check if the value is a dictionary
                if (value is LuaTable subTable && arrayType != typeof(LuaTable))
                {
                    // Parse the dictionary
                    value = ParseLuaTableAs(arrayType, subTable);
                }

                // Set the value
                array.SetValue(value, i);
            }

            return array;
        }

        // Only works with properties
        var properties = targetType.GetProperties();

        // Create an instance of the target type
        object instance = Activator.CreateInstance(targetType);

        // Local function to get the property name
        static string getPropName(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<LuaMemberAttribute>();
            return attr != null ? attr.Name : prop.Name;
        }

        foreach (var property in properties)
        {
            // Get the property name
            string name = getPropName(property);

            if (!obj.Keys.Cast<string>().Contains(name))
                continue;

            // Get the property type
            var type = property.PropertyType;

            // Get the value from the data
            object value = FixValue(type, obj[name]);

            // Check if the value is a dictionary
            if (value is LuaTable subTable && type != typeof(LuaTable))
            {
                // Parse the dictionary
                value = ParseLuaTableAs(type, subTable);
            }

            // Set the value
            property.SetValue(instance, value);
        }

        return instance;
    }

    private static object FixValue(Type targetType, object value)
    {
        if (targetType == typeof(int)) // value will be Int64
        {
            return (int)(long)value;
        }
        if (targetType == typeof(float)) // value will be Double
        {
            if (value is double d)
                return (float)d;
            else if (value is long l)
                return (float)l;

            return (float)(double)value;
        }

        return value;
    }
}
