using System.Collections.Generic;
using System.Linq;
using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace content_1;

public class BitData : IComponentDescriptionData
{
    public int InputBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new BitData()
        {
            InputBits = 2
        };
    }

    public static IOMapping GetDefaultMapping(IComponentDescriptionData data)
    {
        // Can assume data to be BitData
        var bitData = (BitData)data;

        var groups = new List<IOGroup>();

        for (int i = 0; i < bitData.InputBits; i++)
        {
            groups.Add(IOGroup.FromIndexList($"A{i}", ComponentSide.LEFT, i));
        }

        groups.Add(IOGroup.FromIndexList("Z", ComponentSide.RIGHT, bitData.InputBits));

        // Default mapping is 2 1 bit inputs on the left, and output on the right
        return IOMapping.FromGroups(groups.ToArray());
    }
}

[ScriptType("ORGATE")]
public class MyORGate : Component<BitData>
{
    public override string Name => "OR";
    public override bool DisplayIOGroupIdentifiers => false;
    public override bool ShowPropertyWindow => false;

    public MyORGate(IOMapping mapping) : base(mapping) { }

    public override void PerformLogic()
    {
        var inputs = this.GetIOsWithTag("in");
        var z = this.GetIOFromIdentifier("Z");

        var vals = inputs.Select(i => i.GetValue());

        if (vals.Any(v => v == LogicValue.HIGH))
        {
            z.Push(LogicValue.HIGH);
        }
        else
        {
            if (vals.Any(v => v == LogicValue.UNDEFINED) && vals.Any(v => v == LogicValue.LOW))
            {
                return; // PUSH NOTHING, WE CANNOT DETERMINE THE OUTPUT
            }
            else if (vals.All(v => v == LogicValue.LOW))
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

    public override void SubmitUISelected()
    {

    }
}