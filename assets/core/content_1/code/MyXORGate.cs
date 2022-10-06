using System.Collections.Generic;
using System.Linq;
using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace content_1;

[ScriptType("XORGATE")]
public class MyXORGate : Component<BitData>
{
    public override string Name => "XOR";
    public override bool DisplayIOGroupIdentifiers => false;

    public MyXORGate(IOMapping mapping) : base(mapping) { }

    public override void PerformLogic()
    {
        var inputs = this.GetIOsWithTag("in");
        var z = this.GetIOFromIdentifier("Z");

        var vals = inputs.Select(i => i.GetValue());

        if (vals.Any(v => v == LogicValue.UNDEFINED))
        {
            // Cannot determine output
        }
        else
        {
            if (vals.Count(v => v == LogicValue.HIGH) % 2 == 1)
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