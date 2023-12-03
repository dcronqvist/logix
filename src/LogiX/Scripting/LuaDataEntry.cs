using System;
using System.Linq;
using System.Reflection;
using NLua;

namespace LogiX.Scripting;

public enum ScriptingDataType
{
    Node
}

public class LuaDataEntry(LuaTable entryData)
{
    private readonly LuaTable _entryData = entryData;

    [LuaMember(Name = "datatype")]
    public ScriptingDataType DataType => (ScriptingDataType)_entryData["datatype"];

    [LuaMember(Name = "id")]
    public string Identifier => _entryData["id"].ToString();

    public LuaTable GetLuaTable() => _entryData;

    public string GetString(string key) => _entryData[key].ToString();

    public int GetInt(string key) => int.Parse(_entryData[key].ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);

    public float GetFloat(string key) => float.Parse(_entryData[key].ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);

    public bool GetBool(string key) => bool.Parse(_entryData[key].ToString());

    public T Get<T>(string key) => (T)_entryData[key];

    public LuaDataEntry GetLuaDataEntry(string key) => new((LuaTable)_entryData[key]);

    public T GetEntryAs<T>()
    {
        var type = typeof(T);
        return (T)ParseData(type, _entryData);
    }

    public static object ParseData(Type targetType, LuaTable obj)
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
                    value = ParseData(arrayType, subTable);
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
                value = ParseData(type, subTable);
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
