using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

[ScriptType("ADDER"), NodeInfo("Adder", "Arithmetic", "core.markdown.adder")]
public class Adder : ArithmeticNode<ArithmeticData>
{
    public override string Text => "ADD";

    protected override (uint, bool) Calculate(uint a, uint b, bool carryIn)
    {
        var result = a + b + (carryIn ? 1u : 0u);
        var carryOut = result > (1u << this.Data.DataBits) - 1;
        return (result, carryOut);
    }
}