using System;
using System.Text;
using NLua;

namespace LogiX.Scripting;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class LuaFacingEnum<TEnum>(string rootTable) : ILuaFacingConstant
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    public void Register(Lua luaState)
    {
        string enumTypeNameSnake = EmmyLuaHelpers.PascalToSnakeCase(typeof(TEnum).Name);

        luaState.NewTable($"{rootTable}.{enumTypeNameSnake}");

        foreach (string valueName in Enum.GetNames(typeof(TEnum)))
        {
            string valueNameSnake = EmmyLuaHelpers.PascalToSnakeCase(valueName);
            luaState[$"{rootTable}.{enumTypeNameSnake}.{valueNameSnake}"] = Enum.Parse(typeof(TEnum), valueName);
        }
    }

    public void WriteEmmyLua(StringBuilder sb)
    {
        var enumType = Enum.GetUnderlyingType(typeof(TEnum));
        string enumTypeNameSnake = EmmyLuaHelpers.PascalToSnakeCase(typeof(TEnum).Name);

        foreach (string valueName in Enum.GetNames(typeof(TEnum)))
        {
            string valueNameSnake = EmmyLuaHelpers.PascalToSnakeCase(valueName);

            sb.AppendLine($"---@type {EmmyLuaHelpers.GetEmmyLuaType(enumType)}");
            sb.AppendLine($"local {enumTypeNameSnake}_{valueNameSnake}\n");
        }

        sb.AppendLine($"---@enum {enumTypeNameSnake}");
        sb.AppendLine($"{rootTable}.{enumTypeNameSnake} = {{");

        foreach (string valueName in Enum.GetNames(typeof(TEnum)))
        {
            string valueNameSnake = EmmyLuaHelpers.PascalToSnakeCase(valueName);

            sb.AppendLine($"  {valueNameSnake} = {enumTypeNameSnake}_{valueNameSnake},");
        }

        sb.AppendLine("}\n");
    }
}
