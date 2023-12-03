using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace LogiX.Scripting;

public class LuaFacingFunctionDefinedInLua(
    string luaFunctionName,
    string luaFunction,
    string returnType,
    string docs,
    IEnumerable<(string parameterName, string parameterType)> parameters,
    IEnumerable<string> genericTypes) : ILuaFacingFunction
{
    public void Register(Lua luaState, string scriptSourceIdentifier) => luaState.DoString(luaFunction);

    public void WriteEmmyLua(StringBuilder sb)
    {
        if (genericTypes.Any())
        {
            string genericTypesString = string.Join(", ", genericTypes);
            sb.AppendLine($"---@generic {genericTypesString}");
        }

        foreach (var (parameterName, parameterType) in parameters)
        {
            sb.AppendLine($"---@param {parameterName} {parameterType}");
        }

        if (returnType != null)
        {
            sb.AppendLine($"---@return {returnType}");
            sb.AppendLine("---@diagnostic disable-next-line: missing-return, duplicate-set-field");
        }
        else
        {
            sb.AppendLine("---@diagnostic disable-next-line: duplicate-set-field");
        }

        string paramString = string.Join(", ", parameters.Select(x => x.parameterName));
        sb.AppendLine($"function {luaFunctionName}({paramString}) end -- {docs}\n");
    }
}
