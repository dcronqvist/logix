using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Minimal;
using static LogiX.Utilities;

namespace LogiX.Tests.BuiltinComponents.Arithmetic;

[Collection("Tests Collection")]
public class AdderTests
{
    public (uint, bool) GetArithmeticResult(string type, int bits, uint a, uint b, bool carryIn)
    {
        var circ = Utilities.GetEmptyCircuit();
        var adder = NodeDescription.CreateDefaultNodeDescription(type);
        var data = (NodeDescription.CreateDefaultNodeDescriptionData(type) as ArithmeticData)!;

        data.DataBits = bits;
        data.PinMode = PinModeMulti.Combined;

        circ = circ.AddNode(adder, data, Vector2i.Zero, 0);

        circ = circ.AddPin(new Vector2i(-10, -5), 0, bits, out var aID, true, ((uint)a).GetAsLogicValues(bits));
        circ = circ.AddPin(new Vector2i(0, 5), 0, bits, out var bID, true, ((uint)b).GetAsLogicValues(bits));
        circ = circ.AddPin(new Vector2i(1, -10), 0, 1, out var carryInID, true, carryIn ? LogicValue.HIGH : LogicValue.LOW);

        circ = circ.Connect(aID, "Q", adder.ID, "A");
        circ = circ.Connect(bID, "Q", adder.ID, "B");
        circ = circ.Connect(carryInID, "Q", adder.ID, "CarryIn");

        circ = circ.AddPin(new Vector2i(10, 0), 0, bits, out var resultID, false, LogicValue.Z.Multiple(bits));

        circ = circ.Connect(resultID, "Q", adder.ID, "Result");

        circ = circ.AddPin(new Vector2i(10, 5), 0, 1, out var carryOutID, false, LogicValue.Z);

        circ = circ.Connect(adder.ID, "CarryOut", carryOutID, "Q");

        circ.SaveToFile("testadder.circ");

        var sim = Simulation.FromCircuit(circ);

        sim.Step();
        sim.Step();

        var result = sim.ReadPin(resultID, bits).Reverse().GetAsUInt();
        var carryOut = sim.ReadPin(carryOutID, 1).First() == LogicValue.HIGH;

        return ((ushort)result, carryOut);
    }

    [Fact]
    public void TestSimpleAdd()
    {
        var (result, carryOut) = GetArithmeticResult("logix_core:script/ADDER", 4, 0b1010, 0b0101, false);

        Assert.Equal(0b1111u, result);
        Assert.False(carryOut);
    }

    [Fact]
    public void TestSimpleAddWithCarry()
    {
        var (result, carryOut) = GetArithmeticResult("logix_core:script/ADDER", 4, 0b1010, 0b0101, true);

        Assert.Equal(0b0000u, result);
        Assert.True(carryOut);
    }
}