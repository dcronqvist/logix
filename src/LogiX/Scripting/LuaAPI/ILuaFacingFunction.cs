using System.Text;
using NLua;

namespace LogiX.Scripting;

public interface ILuaFacingFunction
{
    void Register(Lua luaState, string scriptSourceIdentifier, string scriptPath);
    void WriteEmmyLua(StringBuilder sb);
}
