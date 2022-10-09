using System.Collections.Generic;
using System.Linq;
using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace content_1;

[ScriptType("NORGATE")]
public class MyNORGate : Component<BitData>
{
    public override string Name => "NOR";
    public override bool DisplayIOGroupIdentifiers => false;
    public override bool ShowPropertyWindow => false;

    public MyNORGate(IOMapping mapping) : base(mapping) { }

    public override void PerformLogic()
    {
        var inputs = this.GetIOsWithTag("in");
        var z = this.GetIOFromIdentifier("Z");

        var vals = inputs.Select(i => i.GetValue());
        var highs = vals.Count(v => v == LogicValue.HIGH);

        if (highs == 0 && vals.Any(v => v == LogicValue.UNDEFINED))
        {
            return;
        }

        if (highs > 0)
        {
            z.Push(LogicValue.LOW);
        }
        else
        {
            z.Push(LogicValue.HIGH);
        }
    }

    public override IComponentDescriptionData GetDescriptionData()
    {
        return new BitData()
        {
            InputBits = this.IOs.Length - 1
        };
    }

    public override void Initialize(BitData data)
    {
        for (int i = 0; i < data.InputBits; i++)
        {
            this.RegisterIO($"A{i}", "in");
        }

        this.RegisterIO("Z", "out");
    }

    public override void SubmitUISelected()
    {

    }
}