using System;
using System.Linq;
using System.Reflection;
using System.Text;
using NLua;

namespace LogiX.Scripting;

public class LuaFacingBaseType<TBase>(params Type[] derivedTypes) : ILuaFacingType
{
    LuaFacingType<TBase> _root = new();

    public void WriteEmmyLua(StringBuilder sb)
    {
        _root.WriteEmmyLua(sb);

        foreach (var derivedType in derivedTypes)
        {
            WriteEmmyLua(sb, derivedType, typeof(TBase));
        }
    }

    private void WriteEmmyLua(StringBuilder sb, Type type, Type baseType = null)
    {
        string typeNameSnake = EmmyLuaHelpers.PascalToSnakeCase(type.Name);
        string baseTypeNameSnake = baseType != null ? EmmyLuaHelpers.PascalToSnakeCase(baseType.Name) : null;

        if (baseTypeNameSnake != null)
        {
            sb.AppendLine($"---@class {typeNameSnake} : {baseTypeNameSnake}");
        }
        else
        {
            sb.AppendLine($"---@class {typeNameSnake}");
        }

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in props.Where(prop => prop.GetCustomAttribute<LuaMemberAttribute>(true) != null))
        {
            var attrib = property.GetCustomAttribute<LuaMemberAttribute>();
            string typeName = EmmyLuaHelpers.GetEmmyLuaType(property.PropertyType);

            if (property.GetCustomAttribute<LuaTypeHintAttribute>() is LuaTypeHintAttribute hint)
            {
                sb.AppendLine($"---@field {attrib.Name} {hint.TypeHint}");
                continue;
            }

            sb.AppendLine($"---@field {attrib.Name} {typeName}");
        }

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in methods.Where(method => method.GetCustomAttribute<LuaMemberAttribute>(true) != null))
        {
            var attrib = method.GetCustomAttribute<LuaMemberAttribute>();
            string returnTypeName = EmmyLuaHelpers.GetEmmyLuaType(method.ReturnType);

            string argsString = string.Join(", ", [
                "self: " + typeNameSnake,
                ..method.GetParameters().Select(x => $"{x.Name}: {EmmyLuaHelpers.GetEmmyLuaType(x.ParameterType)}")
            ]);

            sb.AppendLine($"---@field {attrib.Name} fun({argsString}): {returnTypeName}");
        }

        sb.AppendLine();
    }
}
