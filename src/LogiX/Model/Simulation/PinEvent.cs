using System.Collections.Generic;

namespace LogiX.Model.Simulation;

public class PinEvent(string pinID, int occursInTicks, params LogicValue[] newValues)
{
    public string PinID { get; } = pinID;
    public IReadOnlyCollection<LogicValue> NewValues { get; } = newValues;
    public int OccursInTicks { get; } = occursInTicks;

    public override string ToString() => $"{PinID} [{string.Join(", ", NewValues)}] @ {OccursInTicks}";
}
