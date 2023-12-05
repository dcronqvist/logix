using NLua;

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

    [LuaMember(Name = "pin_id")]
    public string PinID { get; set; }

    [LuaMember(Name = "new_values")]
    public LogicValue[] NewValues { get; set; }

    [LuaMember(Name = "occurs_in_ticks")]
    public int OccursInTicks { get; set; }

    public override string ToString() => $"{PinID} [{string.Join(", ", NewValues)}] @ {OccursInTicks}";
}
