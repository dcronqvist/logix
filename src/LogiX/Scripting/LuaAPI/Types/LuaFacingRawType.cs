using System.Text;

namespace LogiX.Scripting;

public class LuaFacingRawType(string emmyLua) : ILuaFacingType
{
    public void WriteEmmyLua(StringBuilder sb) => sb.AppendLine(emmyLua);
}
