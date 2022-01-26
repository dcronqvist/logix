using LogiX.Components;

namespace LogiX.SaveSystem;

public class ClockDescription : ComponentDescription
{
    [JsonProperty(PropertyName = "interval")]
    public float Interval { get; set; }

    public ClockDescription(Vector2 position, float interval) : base(position, Util.EmptyList<IODescription>(), Util.Listify(new IODescription(1)), ComponentType.Clock)
    {
        this.Interval = interval;
    }

    public override Component ToComponent(bool preserveID)
    {
        Clock c = new Clock(this.Interval, this.Position);
        if (preserveID)
        {
            c.SetUniqueID(this.ID);
        }
        return c;
    }
}