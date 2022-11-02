using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics.UI;

namespace LogiX.Architecture.BuiltinComponents;

public class RamData : IComponentDescriptionData
{
    public ByteAddressableMemory Memory { get; set; }
    public int AddressBits { get; set; }
    public int DataBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new RamData()
        {
            AddressBits = 8,
            DataBits = 8,
            Memory = new ByteAddressableMemory(256, false),
        };
    }
}

[ScriptType("RAM"), ComponentInfo("RAM", "Memory")]
public class RAM : Component<RamData>
{
    public override string Name => "RAM";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private RamData _data;
    private int bytesPerAddress;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(RamData data)
    {
        this.ClearIOs();
        this._data = data;

        this.bytesPerAddress = (int)Math.Ceiling(data.DataBits / 8f);
        this._data.Memory = new ByteAddressableMemory((int)Math.Pow(2, this._data.AddressBits) * bytesPerAddress, false);

        this.RegisterIO("A", data.AddressBits, ComponentSide.LEFT, "address");
        this.RegisterIO("EN", 1, ComponentSide.BOTTOM, "enable");
        this.RegisterIO("CLK", 1, ComponentSide.BOTTOM, "clock");
        this.RegisterIO("LD", 1, ComponentSide.BOTTOM, "load");
        this.RegisterIO("CLR", 1, ComponentSide.BOTTOM, "clear");
        this.RegisterIO("D", data.DataBits, ComponentSide.RIGHT);

        this.TriggerSizeRecalculation();
    }

    private bool previousClk = false;
    private int currentlySelectedAddress = -1;
    public override void PerformLogic()
    {
        var a = this.GetIOFromIdentifier("A").GetValues();
        var en = this.GetIOFromIdentifier("EN").GetValues().First();
        var clk = this.GetIOFromIdentifier("CLK").GetValues().First();
        var ld = this.GetIOFromIdentifier("LD").GetValues().First();
        var clr = this.GetIOFromIdentifier("CLR").GetValues().First();
        var d = this.GetIOFromIdentifier("D");

        if (a.AnyUndefined() || en.IsUndefined() || clk.IsUndefined() || ld.IsUndefined() || clr.IsUndefined() || en != LogicValue.HIGH)
        {
            return; // Do nothing
        }

        bool clockHigh = clk == LogicValue.HIGH;
        bool load = ld == LogicValue.HIGH;
        bool reset = clr == LogicValue.HIGH;

        var address = a.Reverse().GetAsUInt();
        var realAddress = (int)(address * bytesPerAddress);
        this.currentlySelectedAddress = realAddress;

        var values = new LogicValue[_data.DataBits];

        for (int i = 0; i < this.bytesPerAddress; i++)
        {
            var value = this._data.Memory[realAddress + i];

            for (int k = 0; k < 8; k++)
            {
                if (i * 8 + k >= _data.DataBits)
                {
                    break;
                }
                values[i * 8 + k] = (value & (1 << k)) != 0 ? LogicValue.HIGH : LogicValue.LOW;
            }
        }

        if (clockHigh && !previousClk)
        {
            if (load)
            {
                // Load from D into memory
                var dValues = d.GetValues();
                var dval = dValues.GetAsUInt();

                for (int i = 0; i < this.bytesPerAddress; i++)
                {
                    var value = (byte)((dval >> (i * 8)) & 0xFF);
                    this._data.Memory[realAddress + i] = value;
                }
            }
        }

        if (reset)
        {
            for (int i = 0; i < this.bytesPerAddress; i++)
            {
                this._data.Memory[realAddress + i] = 0;
            }
        }

        if (!load)
        {
            d.Push(values);
        }
        previousClk = clockHigh;
    }

    private MemoryEditor memoryEditor = new MemoryEditor(false);
    public override void SubmitUISelected(Editor editor, int componentIndex)
    {

    }

    public override void CompleteSubmitUISelected(Editor editor, int componentIndex)
    {
        var id = this.GetUniqueIdentifier();
        this.memoryEditor.DrawWindow("Read Only Memory Editor", this._data.Memory, this.bytesPerAddress, this.currentlySelectedAddress, () =>
        {
            var currAddressBits = this._data.AddressBits;
            if (ImGui.InputInt($"Address Bits##{id}", ref currAddressBits, 1, 1))
            {
                this._data.AddressBits = currAddressBits;
                this.Initialize(this._data);
            }
            var currDataBits = this._data.DataBits;
            if (ImGui.InputInt($"Data Bits##{id}", ref currDataBits, 1, 1))
            {
                this._data.DataBits = currDataBits;
                this.Initialize(this._data);
            }
            ImGui.PushFont(ImGui.GetIO().FontDefault);
        });
        ImGui.PopFont();
    }
}