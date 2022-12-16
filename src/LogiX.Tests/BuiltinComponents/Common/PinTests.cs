using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using static LogiX.Utilities;

namespace LogiX.Tests.BuiltinComponents.Common;

public class PinTests : IClassFixture<TestsFixture>
{
    [Fact]
    public void PinPropagateInSingleStep1()
    {
        var circuit = Utilities.GetEmptyCircuit();
        circuit.AddPin(Vector2i.Zero, 0, 3, out var pinID, LogicValue.HIGH, LogicValue.HIGH, LogicValue.LOW);

        var sim = Simulation.FromCircuit(circuit);

        // A pin must propagate its value to its output in a single step.
        sim.Step();

        var pinValues = sim.ReadPin(pinID, 3);

        Assert.Equal(pinValues, Utilities.Arrayify(LogicValue.HIGH, LogicValue.HIGH, LogicValue.LOW));
    }

    [Fact]
    public void PinPropagateInSingleStep2()
    {
        var circuit = Utilities.GetEmptyCircuit();
        circuit.AddPin(Vector2i.Zero, 0, 3, out var pinID, LogicValue.HIGH, LogicValue.HIGH, LogicValue.LOW);

        var sim = Simulation.FromCircuit(circuit);

        // If no simulation step is performed, the pin must have a Z value.

        var pinValues = sim.ReadPin(pinID, 3);

        Assert.Equal(pinValues, LogicValue.Z.Multiple(3));
    }
}