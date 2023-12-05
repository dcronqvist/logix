using System.Collections;
using System.Collections.Generic;
using System.Text;
using NLua;

namespace LogiX.Scripting;

public class LuaFacingConstDict<T>(string rootTable, string nextLevel, IDictionary<string, T> dictionary) : ILuaFacingConstant
{
    public void Register(Lua luaState)
    {
        luaState.NewTable($"{rootTable}.{nextLevel}");

        foreach (var (key, value) in dictionary)
        {
            luaState[$"{rootTable}.{nextLevel}.{key}"] = value;
        }
    }

    public void WriteEmmyLua(StringBuilder sb)
    {
        var valueType = typeof(T);

        foreach (var (key, value) in dictionary)
        {
            sb.AppendLine($"---@type {EmmyLuaHelpers.GetEmmyLuaType(valueType)}");
            sb.AppendLine($"local {nextLevel}_{key}\n");
        }

        sb.AppendLine($"---@enum {nextLevel}");
        sb.AppendLine($"{rootTable}.{nextLevel} = {{");

        foreach (var (key, value) in dictionary)
        {
            sb.AppendLine($"  {key} = {nextLevel}_{key},");
        }

        sb.AppendLine("}\n");
    }
}
