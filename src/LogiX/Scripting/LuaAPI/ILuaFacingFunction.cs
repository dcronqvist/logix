using System.Text;
using NLua;

namespace LogiX.Scripting;

public interface ILuaFacingFunction
{
    void Register(Lua luaState, string scriptSourceIdentifier);
    void WriteEmmyLua(StringBuilder sb);
}
