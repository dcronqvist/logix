using System;
using System.Text;
using NLua;

namespace LogiX.Scripting;

public static class EmmyLuaHelpers
{
    public static string GetEmmyLuaType(Type type) => type switch
    {
        Type t when t == typeof(int) => "integer",
        Type t when t == typeof(float) => "number",
        Type t when t == typeof(double) => "number",
        Type t when t == typeof(string) => "string",
        Type t when t == typeof(bool) => "boolean",
        Type t when t == typeof(LuaTable) => "table",
        Type t when t == typeof(LuaFunction) => "fun()",
        Type t when t == typeof(object) => "any",
        _ => PascalToSnakeCase(type.Name)
    };

    public static string PascalToSnakeCase(string input)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (i > 0 && char.IsUpper(input[i]) && !char.IsUpper(input[i - 1]))
            {
                sb.Append('_');
            }
            sb.Append(char.ToLower(input[i]));
        }
        return sb.ToString();
    }
}
