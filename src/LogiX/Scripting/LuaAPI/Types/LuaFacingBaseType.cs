using System.Text;

namespace LogiX.Scripting;

public class LuaFacingBaseType<T>(params ILuaFacingType[] derivedTypes) : ILuaFacingType
{
    private readonly ILuaFacingType _root = new LuaFacingType<T>();

    public void WriteEmmyLua(StringBuilder sb)
    {
        _root.WriteEmmyLua(sb);
        foreach (var derivedType in derivedTypes)
        {
            derivedType.WriteEmmyLua(sb);
        }
    }
}
