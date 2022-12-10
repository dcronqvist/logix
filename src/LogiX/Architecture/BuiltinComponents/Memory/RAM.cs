using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class RamData : INodeDescriptionData
{
    [NodeDescriptionProperty("Label", StringHint = "e.g. RAM_1", StringMaxLength = 16, StringRegexFilter = "^[a-zA-Z0-9_]*$")]
    public string Label { get; set; }

    [NodeDescriptionProperty("Size", IntMinValue = 1, IntMaxValue = 256, HelpTooltip = "The number of address bits.")]
    public int AddressBits { get; set; }

    // Handled internally
    public ByteAddressableMemory Memory { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new RamData()
        {
            AddressBits = 8,
            Memory = new ByteAddressableMemory(256, false),
            Label = ""
        };
    }
}

[ScriptType("RAM"), NodeInfo("RAM", "Memory", "core.markdown.ram")]
public class RAM : BoxNode<RamData>
{
    public override string Text => "RAM";
    public override float TextScale => 1f;

    private RamData _data;

    private LogicValue _prevCLK = LogicValue.LOW;
    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var address = pins.Get("ADDRESS").Read().Reverse();
        var clk = pins.Get("CLK").Read().First();
        var we = pins.Get("WE").Read().First();

        if (address.AnyUndefined() || clk.IsUndefined() || we.IsUndefined())
        {
            this.hasSelectedAddress = false;
            yield return (pins.Get("DATA"), LogicValue.Z.Multiple(8), 1);
            yield break;
        }

        var addressValue = address.GetAsUInt();
        this.currentlySelectedAddress = addressValue;
        this.hasSelectedAddress = true;

        if (clk == LogicValue.HIGH && this._prevCLK == LogicValue.LOW)
        {
            if (we == LogicValue.HIGH)
            {
                var data = pins.Get("DATA").Read().Reverse();
                this._data.Memory[addressValue] = data.GetAsByte();
            }
        }

        var output = this._data.Memory[addressValue];

        if (we == LogicValue.LOW)
        {
            yield return (pins.Get("DATA"), output.GetAsLogicValues(8), 1);
        }
        else
        {
            yield return (pins.Get("DATA"), LogicValue.Z.Multiple(8), 1);
        }

        this._prevCLK = clk;
        yield break;
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("ADDRESS", this._data.AddressBits, true, new Vector2i(0, 1));
        yield return new PinConfig("DATA", 8, false, new Vector2i(this.GetSize().X, 1));
        yield return new PinConfig("CLK", 1, true, new Vector2i(1, this.GetSize().Y));
        yield return new PinConfig("WE", 1, true, new Vector2i(1, 0));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 2);
    }

    public override void Initialize(RamData data)
    {
        this._data = data;
        this._data.Memory = data.Memory;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        return this.Evaluate(pins).Select(x => (x.Item1, x.Item2));
    }

    private string _pathLoadedFrom;
    private bool hasSelectedAddress = false;
    private uint currentlySelectedAddress = 0;
    private MemoryEditor memoryEditor = new MemoryEditor(false);
    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        base.SubmitUISelected(editor, componentIndex);
    }

    public override void CompleteSubmitUISelected(Editor editor, int componentIndex)
    {
        var id = this.ID.ToString();
        this.memoryEditor.DrawWindow($"Read Only Memory Editor##{id}", this._data.Memory, 1, this.currentlySelectedAddress, this.hasSelectedAddress, () =>
        {
            this.SubmitUISelected(editor, componentIndex);

            if (ImGui.Button($"Load From File##{id}"))
            {
                var fileDialog = new FileDialog(FileDialog.LastDirectory, "Load ROM from file", FileDialogType.SelectFile, (path) =>
                {
                    using (BinaryReader sr = new BinaryReader(File.Open(path, FileMode.Open)))
                    {
                        var data = sr.ReadBytes((int)sr.BaseStream.Length);
                        var addressBits = (int)Math.Ceiling(Math.Log(data.Length, 2));

                        var setAddressBits = new CModifyComponentDataProp(this.ID, this._data.GetType().GetProperty(nameof(this._data.AddressBits)), addressBits);
                        var setMemory = new CModifyComponentDataProp(this.ID, this._data.GetType().GetProperty(nameof(this._data.Memory)), new ByteAddressableMemory(data));
                        var multi = new CMulti("Load ROM from file", setAddressBits, setMemory);

                        editor.Execute(multi, editor);
                        this._pathLoadedFrom = path;
                    }

                }, ".bin");
                editor.OpenPopup(fileDialog);
            }
            ImGui.SameLine();
            if (ImGui.Button($"Dump To File##{id}"))
            {
                var fileDialog = new FileDialog(FileDialog.LastDirectory, "Dump ROM contents to file", FileDialogType.SaveFile, (path) =>
                {
                    using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Open)))
                    {
                        bw.Write(this._data.Memory.Data);
                    }

                }, ".bin");
                editor.OpenPopup(fileDialog);
            }
            ImGui.SameLine();
            if (this._pathLoadedFrom is null)
                ImGui.BeginDisabled();

            if (ImGui.Button($"Reload##{id}"))
            {
                using (BinaryReader sr = new BinaryReader(File.Open(this._pathLoadedFrom, FileMode.Open)))
                {
                    var data = sr.ReadBytes((int)sr.BaseStream.Length);
                    var addressBits = (int)Math.Ceiling(Math.Log(data.Length, 2));

                    var setAddressBits = new CModifyComponentDataProp(this.ID, this._data.GetType().GetProperty(nameof(this._data.AddressBits)), addressBits);
                    var setMemory = new CModifyComponentDataProp(this.ID, this._data.GetType().GetProperty(nameof(this._data.Memory)), new ByteAddressableMemory(data));
                    var multi = new CMulti("Reload ROM from file", setAddressBits, setMemory);

                    editor.Execute(multi, editor);
                }
            }

            if (this._pathLoadedFrom is null)
                ImGui.EndDisabled();

            ImGui.PushFont(ImGui.GetIO().FontDefault);
        }, () =>
        {
            ImGui.PopFont();
        });
    }

    // public override string Name => "RAM";
    // public override bool DisplayIOGroupIdentifiers => true;
    // public override bool ShowPropertyWindow => true;

    // private RamData _data;

    // public override IComponentDescriptionData GetDescriptionData()
    // {
    //     return _data;
    // }

    // public override void Initialize(RamData data)
    // {
    //     this.ClearIOs();
    //     this._data = data;
    //     this._data.Memory = data.Memory;

    //     if (this._data.Label is null)
    //     {
    //         this._data.Label = "";
    //     }

    //     this.RegisterIO("ADDRESS", data.AddressBits, ComponentSide.LEFT, "address");
    //     this.RegisterIO("ENABLE", 1, ComponentSide.BOTTOM, "enable");
    //     this.RegisterIO("CLOCK", 1, ComponentSide.BOTTOM, "clock");
    //     this.RegisterIO("LOAD", 1, ComponentSide.BOTTOM, "load");
    //     this.RegisterIO("CLEAR", 1, ComponentSide.BOTTOM, "clear");
    //     this.RegisterIO("DATA", 8, ComponentSide.RIGHT);

    //     this.TriggerSizeRecalculation();
    // }

    // private bool previousClk = false;
    // private bool hasSelectedAddress = false;
    // private uint currentlySelectedAddress = 0;
    // public override void PerformLogic()
    // {
    //     var a = this.GetIOFromIdentifier("ADDRESS").GetValues();
    //     var en = this.GetIOFromIdentifier("ENABLE").GetValues().First();
    //     var clk = this.GetIOFromIdentifier("CLOCK").GetValues().First();
    //     var ld = this.GetIOFromIdentifier("LOAD").GetValues().First();
    //     var clr = this.GetIOFromIdentifier("CLEAR").GetValues().First();
    //     var d = this.GetIOFromIdentifier("DATA");

    //     this.hasSelectedAddress = false;

    //     if (a.AnyUndefined())
    //     {
    //         return; // Do nothing
    //     }

    //     bool clockHigh = clk == LogicValue.HIGH;
    //     bool load = ld == LogicValue.HIGH;
    //     bool reset = clr == LogicValue.HIGH;

    //     var address = a.Reverse().GetAsUInt();
    //     this.currentlySelectedAddress = address;

    //     this.hasSelectedAddress = true;

    //     if (reset)
    //     {
    //         this._data.Memory[address] = 0;
    //     }

    //     if (en != LogicValue.HIGH)
    //     {
    //         return; // DO nothing
    //     }


    //     if (clockHigh && !previousClk)
    //     {
    //         if (load)
    //         {
    //             // Load from D into memory
    //             var dValues = d.GetValues();
    //             var dval = dValues.Reverse<LogicValue>().GetAsByte();
    //             this._data.Memory[address] = dval;
    //         }
    //     }

    //     if (!load)
    //     {
    //         var value = this._data.Memory[address];
    //         var valueAsBits = value.GetAsLogicValues(8);
    //         d.Push(valueAsBits);
    //     }
    //     previousClk = clockHigh;
    // }

    // private MemoryEditor memoryEditor = new MemoryEditor(false);
    // public override void CompleteSubmitUISelected(Editor editor, int componentIndex)
    // {
    //     var id = this.GetUniqueIdentifier();
    //     this.memoryEditor.DrawWindow($"Random Access Memory Editor##{id}", this._data.Memory, 1, this.currentlySelectedAddress, this.hasSelectedAddress, () =>
    //     {
    //         base.SubmitUISelected(editor, componentIndex);

    //         var avail = ImGui.GetContentRegionAvail();
    //         var padding = ImGui.GetStyle().ItemInnerSpacing;
    //         var buttonSize = new Vector2(avail.X / 3 - padding.X, 0);

    //         if (ImGui.Button($"Load From File##{id}", buttonSize))
    //         {
    //             var fileDialog = new FileDialog(FileDialog.LastDirectory, "Load RAM from file", FileDialogType.SelectFile, (path) =>
    //             {
    //                 using (BinaryReader sr = new BinaryReader(File.Open(path, FileMode.Open)))
    //                 {
    //                     var data = sr.ReadBytes((int)sr.BaseStream.Length);
    //                     var addressBits = (int)Math.Ceiling(Math.Log(data.Length, 2));
    //                     editor.Execute(new CModifyComponentDataProp(this.ID, this._data.GetType().GetProperty(nameof(this._data.AddressBits)), addressBits), editor);
    //                     this._data.Memory = new ByteAddressableMemory(data);
    //                 }

    //             }, ".bin");
    //             editor.OpenPopup(fileDialog);
    //         }
    //         ImGui.SameLine();
    //         if (ImGui.Button($"Dump To File##{id}", buttonSize))
    //         {
    //             var fileDialog = new FileDialog(FileDialog.LastDirectory, "Dump RAM contents to file", FileDialogType.SaveFile, (path) =>
    //             {
    //                 using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Open)))
    //                 {
    //                     bw.Write(this._data.Memory.Data);
    //                 }

    //             }, ".bin");
    //             editor.OpenPopup(fileDialog);
    //         }
    //         ImGui.SameLine();
    //         // Clear button
    //         if (ImGui.Button($"Clear##{id}", buttonSize))
    //         {
    //             this._data.Memory = new ByteAddressableMemory((int)Math.Pow(2, this._data.AddressBits), false);
    //         }

    //         ImGui.PushFont(ImGui.GetIO().FontDefault);
    //     }, () =>
    //     {
    //         ImGui.PopFont();
    //     });
    // }
}