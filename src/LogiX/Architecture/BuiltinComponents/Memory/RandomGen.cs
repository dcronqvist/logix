using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class RandomGenData : IComponentDescriptionData
{
    public int DataBits { get; set; }
    public bool ExposeSeedPin { get; set; }
    public int Seed { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new RandomGenData()
        {
            DataBits = 4,
            ExposeSeedPin = false,
            Seed = 0
        };
    }
}

[ScriptType("RANDOM"), ComponentInfo("Random Generator", "Memory")]
public class RandomGen : Component<RandomGenData>
{
    public override string Name => "RNG";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private RandomGenData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(RandomGenData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("CLK", 1, ComponentSide.LEFT); // Update value
        this.RegisterIO("EN", 1, ComponentSide.LEFT); // Only if enabled
        this.RegisterIO("R", 1, ComponentSide.LEFT); // reset value to initial value from seed
        this.RegisterIO("Q", data.DataBits, ComponentSide.RIGHT); // random output

        if (data.ExposeSeedPin)
        {
            this.RegisterIO("SEED", data.DataBits, ComponentSide.TOP); // seed value
        }
        else
        {
            this._random = new Random(data.Seed);
        }
    }

    private bool TryGetSeed(out int seed)
    {
        if (this._data.ExposeSeedPin)
        {
            var seedPin = this.GetIOFromIdentifier("SEED");
            var seedValues = seedPin.GetValues();

            if (seedValues.AnyUndefined())
            {
                seed = 0;
                return false;
            }
            else
            {
                seed = (int)seedValues.Reverse().GetAsUInt();
                return true;
            }
        }
        else
        {
            seed = this._data.Seed;
            return true;
        }
    }

    private int _previousSeed = -1;
    private bool _previousClock;
    private Random _random;
    private uint _randomValue;

    public override void PerformLogic()
    {
        var clk = this.GetIOFromIdentifier("CLK");
        var en = this.GetIOFromIdentifier("EN");
        var r = this.GetIOFromIdentifier("R");
        var q = this.GetIOFromIdentifier("Q");

        if (en.GetValues().AnyUndefined() || clk.GetValues().AnyUndefined() || r.GetValues().AnyUndefined())
        {
            return; // Can't do anything if we don't have all the values
        }

        var clockHigh = clk.GetValues().First() == LogicValue.HIGH;
        var enabled = en.GetValues().First() == LogicValue.HIGH;
        var reset = r.GetValues().First() == LogicValue.HIGH;

        if (clockHigh && enabled && !_previousClock)
        {
            if (this.TryGetSeed(out var seed))
            {
                if (seed != _previousSeed)
                {
                    _random = new Random(seed);
                    _previousSeed = seed;
                }

                _randomValue = (uint)_random.Next(0, (int)Math.Pow(2, this._data.DataBits));
            }
        }

        if (reset)
        {
            if (this.TryGetSeed(out var seed))
            {
                _random = new Random(seed);
                _randomValue = (uint)_random.Next(0, (int)Math.Pow(2, this._data.DataBits));
            }
        }

        q.Push(_randomValue.GetAsLogicValues(this._data.DataBits));
        this._previousClock = clockHigh;
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        var id = this.GetUniqueIdentifier();
        var databits = this._data.DataBits;
        if (ImGui.InputInt($"Data Bits##{id}", ref databits, 1, 1))
        {
            this._data.DataBits = databits;
            this.Initialize(this._data);
        }
        var seed = this._data.Seed;
        if (ImGui.InputInt($"Seed##{id}", ref seed, 1, 1))
        {
            this._data.Seed = seed;
            this.Initialize(this._data);
        }
        var exposeSeed = this._data.ExposeSeedPin;
        if (ImGui.Checkbox($"Expose Seed Pin##{id}", ref exposeSeed))
        {
            this._data.ExposeSeedPin = exposeSeed;
            this.Initialize(this._data);
        }
    }
}