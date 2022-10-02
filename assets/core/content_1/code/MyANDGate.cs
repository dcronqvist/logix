using System.Collections.Generic;
using System.Linq;
using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace content_1;

[ScriptType("ANDGATE")]
public class MyANDGate : Component<BitData>
{
    public override string Name => "AND";
    public override bool DisplayIOGroupIdentifiers => false;

    public MyANDGate(IOMapping mapping) : base(mapping) { }

    public override void PerformLogic()
    {
        var inputs = this.GetIOsWithTag("in");
        var z = this.GetIOFromIdentifier("Z");

        var vals = inputs.Select(i => i.GetValue());

        if (vals.Any(v => v == LogicValue.UNDEFINED))
        {
            return; // DO NOTHING, WE CANNOT DETERMINE THE OUTPUT
        }
        else
        {
            if (vals.All(v => v == LogicValue.HIGH))
            {
                z.Push(LogicValue.HIGH);
            }
            else
            {
                z.Push(LogicValue.LOW);
            }
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
}