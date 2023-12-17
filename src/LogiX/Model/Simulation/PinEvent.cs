namespace LogiX.Model.Simulation;

public class PinEvent
{
    public PinEvent() { }

    public PinEvent(string pinID, int occursInTicks, params LogicValue[] newValues)
    {
        PinID = pinID;
        OccursInTicks = occursInTicks;
        NewValues = newValues;
    }

    public string PinID { get; set; }

    public LogicValue[] NewValues { get; set; }

    public int OccursInTicks { get; set; }

    public override string ToString() => $"{PinID} [{string.Join(", ", NewValues)}] @ {OccursInTicks}";
}
