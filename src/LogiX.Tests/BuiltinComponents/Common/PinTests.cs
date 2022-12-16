using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Minimal;
using static LogiX.Utilities;

namespace LogiX.Tests.BuiltinComponents.Common;

[Collection("Tests Collection")]

public class PinTests
{
    [Fact]
    public void PinPropagateInSingleStep1()
    {
        var circuit = Utilities.GetEmptyCircuit();
        circuit.AddPin(Vector2i.Zero, 0, 3, out var pinID, true, LogicValue.HIGH, LogicValue.HIGH, LogicValue.LOW);

        var sim = Simulation.FromCircuit(circuit);

        // A pin must propagate its value to its output in a single step.
        sim.Step();

        var pinValues = sim.ReadPin(pinID, 3);

        Assert.Equal(Utilities.Arrayify(LogicValue.HIGH, LogicValue.HIGH, LogicValue.LOW), pinValues);
    }

    [Fact]
    public void PinPropagateVisibleAtOtherPin()
    {
        uint valueToPropagate = 0b1101;
        var asLVals = valueToPropagate.GetAsLogicValues(4);

        var circuit = Utilities.GetEmptyCircuit();
        circuit.AddPin(Vector2i.Zero, 0, 4, out var pin1ID, true, asLVals);
        circuit.AddPin(new Vector2i(0, 5), 0, 4, out var pin2ID, false, LogicValue.Z.Multiple(4));

        circuit = circuit.Connect(pin1ID, "Q", pin2ID, "Q");

        var sim = Simulation.FromCircuit(circuit);

        // Single step to propagate the value, MSUT be visible at the other pin after this step.
        sim.Step();

        var pin2 = sim.GetNodeFromID(pin2ID);
        var pin2Data = (pin2.GetNodeData() as PinData)!;

        Assert.Equal(pin2Data.Values, asLVals);
    }
}