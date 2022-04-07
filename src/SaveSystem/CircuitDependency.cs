using System.Diagnostics.CodeAnalysis;

namespace LogiX.SaveSystem;

public class CircuitDependency
{
    public string CircuitID { get; set; }
    public string CircuitUpdateID { get; set; }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is CircuitDependency dependency &&
               CircuitID == dependency.CircuitID &&
               CircuitUpdateID == dependency.CircuitUpdateID;
    }
}