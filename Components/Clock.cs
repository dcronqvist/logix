using LogiX.SaveSystem;

namespace LogiX.Components;

public class Clock : Component
{
    private float interval;
    public float Interval
    {
        get
        {
            return interval;
        }
        set
        {
            interval = value;
            if (interval < 1)
            {
                interval = 1;
            }
        }
    }
    private float internalCounter;
    public override string Text => "Clock: " + Interval;

    public Clock(float interval, Vector2 position) : base(Util.EmptyList<int>(), Util.Listify(1), position)
    {
        this.Interval = interval;
    }

    public override void PerformLogic()
    {
        if (internalCounter > this.Interval)
        {
            this.OutputAt(0).SetValues(this.OutputAt(0).Values[0] == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH);
            internalCounter = 0;
        }
        else
        {
            internalCounter += 1;
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new ClockDescription(this.Position, this.Interval);
    }

    public override void OnSingleSelectedSubmitUI()
    {
        ImGui.Begin("Clock", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoNav);

        ImGui.InputFloat("Interval", ref this.interval, 1, 100, "%.0f");

        ImGui.End();


    }
}