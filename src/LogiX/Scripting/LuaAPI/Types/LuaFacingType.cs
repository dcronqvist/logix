using System;
using System.Linq;
using System.Reflection;
using System.Text;
using NLua;

namespace LogiX.Scripting;

public class LuaFacingType<T> : ILuaFacingType
{
    public void WriteEmmyLua(StringBuilder sb) => WriteEmmyLua(sb, typeof(T));

    private void WriteEmmyLua(StringBuilder sb, Type type)
    {
        string typeNameSnake = EmmyLuaHelpers.PascalToSnakeCase(type.Name);
        sb.AppendLine($"---@class {typeNameSnake}");

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in props.Where(prop => prop.GetCustomAttribute<LuaMemberAttribute>(true) != null))
        {
            var attrib = property.GetCustomAttribute<LuaMemberAttribute>();
            string typeName = EmmyLuaHelpers.GetEmmyLuaType(property.PropertyType);

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
