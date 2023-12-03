using System.Text;
using NLua;

namespace LogiX.Scripting;

public interface ILuaFacingConstant
{
    void Register(Lua luaState);
    void WriteEmmyLua(StringBuilder sb);
}
