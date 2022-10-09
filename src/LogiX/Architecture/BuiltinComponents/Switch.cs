using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class SwitchData : IComponentDescriptionData
{
    public int Bits { get; set; }
    public LogicValue[] Values { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new SwitchData()
        {
            Bits = 1,
            Values = new LogicValue[] { LogicValue.UNDEFINED }
        };
    }

    public static IOMapping GetDefaultMapping(IComponentDescriptionData data)
    {
        var switchData = (SwitchData)data;
        return IOMapping.FromGroups(IOGroup.FromIndexList("io", ComponentSide.RIGHT, Enumerable.Range(0, switchData.Bits).ToArray()));
    }
}

[ScriptType("SWITCH")]
public class Switch : Component<SwitchData>
{
    public override string Name => this.CurrentValues.Select(x => x.ToString().Substring(0, 1)).Aggregate((x, y) => x + y);
    public override bool DisplayIOGroupIdentifiers => false;
    public override bool ShowPropertyWindow => true;

    public LogicValue[] CurrentValues { get; private set; }

    public Switch(IOMapping mapping) : base(mapping)
    {
    }

    public override IComponentDescriptionData GetDescriptionData()
    {
        return new SwitchData()
        {
            Bits = CurrentValues.Length,
            Values = CurrentValues
        };
    }

    public override void Initialize(SwitchData data)
    {
        this.CurrentValues = data.Values;

        for (int i = 0; i < data.Bits; i++)
        {
            this.RegisterIO($"io{i}", "io");
        }
    }

    public override void PerformLogic()
    {
        var ios = this.GetIOsWithTag("io");

        for (int i = 0; i < ios.Length; i++)
        {
            var val = this.CurrentValues[i];
            ios[i].Push(val);
        }
    }

    public override void SubmitUISelected()
    {
        int bits = this.CurrentValues.Length;
    }
}