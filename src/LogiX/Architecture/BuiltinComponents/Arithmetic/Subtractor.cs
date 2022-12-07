using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

[ScriptType("SUBTRACTOR"), NodeInfo("Subtractor", "Arithmetic", "core.markdown.subtractor")]
public class Subtractor : ArithmeticNode<ArithmeticData>
{
    public override string Text => "SUB";

    protected override (uint, bool) Calculate(uint a, uint b, bool carryIn)
    {
        // Return the result and the borrow
        var result = a - b - (carryIn ? 1u : 0u);
        var borrow = result > (1u << this.Data.DataBits) - 1;
        return (result, borrow);
    }
}