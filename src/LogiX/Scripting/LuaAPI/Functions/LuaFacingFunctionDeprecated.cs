using System.Text;
using NLua;

namespace LogiX.Scripting;

public class LuaFacingFunctionDeprecated(ILuaFacingFunction wrappedFunction) : ILuaFacingFunction
{
    public void Register(Lua luaState, string scriptSourceIdentifier) => wrappedFunction.Register(luaState, scriptSourceIdentifier);
    public void WriteEmmyLua(StringBuilder sb)
    {
        sb.AppendLine("---@deprecated");
        wrappedFunction.WriteEmmyLua(sb);
    }
}
