using System;
using System.Linq;
using System.Reflection;
using NLua;

namespace LogiX.Scripting;

public static class LuaExtensions
{
    public static T ParseLuaTableAs<T>(this LuaTable table) where T : new()
    {
        var type = typeof(T);
        return (T)ParseLuaTableAs(type, table);
    }

    private static object ParseLuaTableAs(Type targetType, LuaTable obj)
    {
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
            return (float)(double)value;
        }

        return value;
    }
}
