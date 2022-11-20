using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics.UI;

namespace LogiX.Architecture.BuiltinComponents;

public class RamData : IComponentDescriptionData
{
    public ByteAddressableMemory Memory { get; set; }
    public int AddressBits { get; set; }
    public string Label { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new RamData()
        {
            AddressBits = 8,
            Memory = new ByteAddressableMemory(256, false),
            Label = ""
        };
    }
}

[ScriptType("RAM"), ComponentInfo("RAM", "Memory", "core.markdown.ram")]
public class RAM : Component<RamData>
{
    public override string Name => "RAM";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private RamData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(RamData data)
    {
        this.ClearIOs();
        this._data = data;
        this._data.Memory = data.Memory;

        if (this._data.Label is null)
        {
            this._data.Label = "";
        }

        this.RegisterIO("ADDRESS", data.AddressBits, ComponentSide.LEFT, "address");
        this.RegisterIO("ENABLE", 1, ComponentSide.BOTTOM, "enable");
        this.RegisterIO("CLOCK", 1, ComponentSide.BOTTOM, "clock");
        this.RegisterIO("LOAD", 1, ComponentSide.BOTTOM, "load");
        this.RegisterIO("CLEAR", 1, ComponentSide.BOTTOM, "clear");
        this.RegisterIO("DATA", 8, ComponentSide.RIGHT);

        this.TriggerSizeRecalculation();
    }

    private bool previousClk = false;
    private bool hasSelectedAddress = false;
    private uint currentlySelectedAddress = 0;
    public override void PerformLogic()
    {
        var a = this.GetIOFromIdentifier("ADDRESS").GetValues();
        var en = this.GetIOFromIdentifier("ENABLE").GetValues().First();
        var clk = this.GetIOFromIdentifier("CLOCK").GetValues().First();
        var ld = this.GetIOFromIdentifier("LOAD").GetValues().First();
        var clr = this.GetIOFromIdentifier("CLEAR").GetValues().First();
        var d = this.GetIOFromIdentifier("DATA");

        this.hasSelectedAddress = false;

        if (a.AnyUndefined())
        {
            return; // Do nothing
        }

        bool clockHigh = clk == LogicValue.HIGH;
        bool load = ld == LogicValue.HIGH;
        bool reset = clr == LogicValue.HIGH;

        var address = a.Reverse().GetAsUInt();
        this.currentlySelectedAddress = address;

        this.hasSelectedAddress = true;

        if (reset)
        {
            this._data.Memory[address] = 0;
        }

        if (en != LogicValue.HIGH)
        {
            return; // DO nothing
        }


        if (clockHigh && !previousClk)
        {
            if (load)
            {
                // Load from D into memory
                var dValues = d.GetValues();
                var dval = dValues.Reverse<LogicValue>().GetAsByte();
                this._data.Memory[address] = dval;
            }
        }

        if (!load)
        {
            var value = this._data.Memory[address];
            var valueAsBits = value.GetAsLogicValues(8);
            d.Push(valueAsBits);
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
        this.memoryEditor.DrawWindow($"Random Access Memory Editor##{id}", this._data.Memory, 1, this.currentlySelectedAddress, this.hasSelectedAddress, () =>
        {
            var currLabel = this._data.Label;
            if (ImGui.InputTextWithHint($"Label##{id}", "Label", ref currLabel, 16))
            {
                this._data.Label = currLabel;
            }

            var currAddressBits = this._data.AddressBits;
            if (ImGui.InputInt($"Address Bits##{id}", ref currAddressBits, 1, 1))
            {
                this._data.AddressBits = currAddressBits;
                this._data.Memory = new ByteAddressableMemory((int)Math.Pow(2, this._data.AddressBits), false);
                this.Initialize(this._data);
            }

            if (ImGui.Button($"Load From File##{id}"))
            {
                var fileDialog = new FileDialog(".", FileDialogType.SelectFile, (path) =>
                {
                    using (BinaryReader sr = new BinaryReader(File.Open(path, FileMode.Open)))
                    {
                        var data = sr.ReadBytes((int)sr.BaseStream.Length);
                        var addressBits = (int)Math.Ceiling(Math.Log(data.Length, 2));

                        this._data.AddressBits = addressBits;

                        this.Initialize(this._data);

                        this._data.Memory = new ByteAddressableMemory(data);
                    }

                }, ".bin");
                editor.OpenPopup(fileDialog);
            }
            ImGui.SameLine();
            if (ImGui.Button($"Dump To File##{id}"))
            {
                var fileDialog = new FileDialog(".", FileDialogType.SaveFile, (path) =>
                {
                    using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Open)))
                    {
                        bw.Write(this._data.Memory.Data);
                    }

                }, ".bin");
                editor.OpenPopup(fileDialog);
            }
            ImGui.SameLine();
            // Clear button
            if (ImGui.Button($"Clear##{id}"))
            {
                this._data.Memory = new ByteAddressableMemory((int)Math.Pow(2, this._data.AddressBits), false);
            }

            ImGui.PushFont(ImGui.GetIO().FontDefault);
        }, () =>
        {
            ImGui.PopFont();
        });
    }
}