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

    public T GetEntryAs<T>() where T : new() => _entryData.ParseLuaTableAs<T>();
}
