using System;
using System.Linq;
using System.Reflection;
using System.Text;
using NLua;

namespace LogiX.Scripting;

[AttributeUsage(AttributeTargets.All)]
public class LuaTypeHintAttribute : Attribute
{
    public string TypeHint { get; set; }
}

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class LuaFacingFunctionDelegate(
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    string luaFunctionName,
    string docs,
    Func<string, string, Delegate> delegateFactory
) : ILuaFacingFunction
{
    public void Register(Lua luaState, string scriptSourceIdentifier, string scriptPath)
    {
        var @delegate = delegateFactory(scriptSourceIdentifier, scriptPath);
        luaState.RegisterFunction(luaFunctionName, @delegate.Target, @delegate.Method);
    }

    public void WriteEmmyLua(StringBuilder sb)
    {
        var @delegate = delegateFactory("", "");
        var parameters = @delegate.GetMethodInfo().GetParameters();

        foreach (var parameter in parameters)
        {
            if (parameter.GetCustomAttribute<LuaTypeHintAttribute>() is LuaTypeHintAttribute hint)
            {
                sb.AppendLine($"---@param {parameter.Name} {hint.TypeHint}");
                continue;
            }

            string paramEmmyLuaType = EmmyLuaHelpers.GetEmmyLuaType(parameter.ParameterType);
            sb.AppendLine($"---@param {parameter.Name} {paramEmmyLuaType}");
        }

        if (@delegate.GetMethodInfo().ReturnType != typeof(void))
        {
            if (@delegate.GetMethodInfo().GetCustomAttribute<LuaTypeHintAttribute>() is LuaTypeHintAttribute hint)
            {
                sb.AppendLine($"---@return {hint.TypeHint}");
                sb.AppendLine("---@diagnostic disable-next-line: missing-return, duplicate-set-field");
            }
            else
            {
                string returnEmmyLuaType = EmmyLuaHelpers.GetEmmyLuaType(@delegate.GetMethodInfo().ReturnType);
                sb.AppendLine($"---@return {returnEmmyLuaType}");
                sb.AppendLine("---@diagnostic disable-next-line: missing-return, duplicate-set-field");
            }
        }
        else
        {
            sb.AppendLine("---@diagnostic disable-next-line: duplicate-set-field");
        }

        string paramString = string.Join(", ", parameters.Select(x => x.Name));
        sb.AppendLine($"function {luaFunctionName}({paramString}) end -- {docs}\n");
    }
}
