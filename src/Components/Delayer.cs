using LogiX.SaveSystem;

namespace LogiX.Components;

public class Delayer : Component
{

    private int Ticks { get; set; }
    private Queue<List<LogicValue>> Buffer { get; set; }
    public override string Text => "Delayer: " + Ticks;

    public override string? Documentation => @"
# Delayer Component

The delayer component delays the input by the specified number of ticks. The input is buffered and the output is the buffered input after the specified amount of ticks.

The delayer component is useful in situations where some kind of circuit delay is wanted.
";

    public Delayer(int ticks, int bits, bool multibit, Vector2 position) : base(multibit ? Util.Listify(bits) : Util.NValues(1, bits), multibit ? Util.Listify(bits) : Util.NValues(1, bits), position)
    {
        this.Ticks = ticks;
        this.Buffer = new Queue<List<LogicValue>>();
        Enumerable.Range(0, this.Ticks).ToList().ForEach(i => this.Buffer.Enqueue(Util.NValues(LogicValue.LOW, bits)));
    }

    public override void PerformLogic()
    {
        // Shift buffer down
        // Set output to index 0
        // set current input to length of buffer
        List<LogicValue> output = this.Buffer.Dequeue();
        this.Buffer.Enqueue(this.InputAt(0).Values);
        this.OutputAt(0).SetValues(output);
    }

    public override void OnSingleSelectedSubmitUI()
    {
        ImGui.Begin("Delayer", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoNav);
        int ticks = this.Ticks;
        ImGui.InputInt("Ticks", ref ticks, 1, 50);
        if (ticks != this.Ticks)
        {
            this.Ticks = ticks;
            this.Buffer.Clear();
            Enumerable.Range(0, this.Ticks).ToList().ForEach(i => this.Buffer.Enqueue(Util.NValues(LogicValue.LOW, this.InputAt(0).Values.Count)));
        }
        ImGui.End();
    }

    public override ComponentDescription ToDescription()
    {
        if (this.Inputs.Count == 1)
        {
            // multibit
            return new DelayerDescription(this.Position, this.Rotation, this.Ticks, this.Inputs[0].Bits, true);
        }
        else
        {
            // single bit
            return new DelayerDescription(this.Position, this.Rotation, this.Ticks, this.Inputs.Count, false);
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.GateAmount(("Delayer", 1));
    }
}