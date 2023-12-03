using System.Text;

namespace LogiX.Scripting;

public interface ILuaFacingType
{
    void WriteEmmyLua(StringBuilder sb);
}
