using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class RomData : INodeDescriptionData
{
    [NodeDescriptionProperty("Address Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int AddressBits { get; set; }

    [NodeDescriptionProperty("Word Size", IntMinValue = 1, IntMaxValue = 16, HelpTooltip = "The number of bytes per word in the ROM.")]
    public int WordSize { get; set; }

    // Handled internally.
    public WordAddressableMemory Memory { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new RomData()
        {
            AddressBits = 8,
            WordSize = 1,
            Memory = new WordAddressableMemory(256, false),
        };
    }
}

[ScriptType("ROM"), NodeInfo("ROM", "Memory", "logix_core:docs/components/rom.md")]
public class ROM : BoxNode<RomData>
{
    public override string Text => "ROM";
    public override float TextScale => 1f;

    private RomData _data;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var address = pins.Get("ADDRESS").Read(this._data.AddressBits).Reverse();
        var enable = pins.Get("ENABLE").Read(1).First();

        if (address.AnyUndefined() || enable.IsUndefined())
        {
            this.hasSelectedAddress = false;
            yield return (pins.Get("DATA"), LogicValue.Z.Multiple(this._data.WordSize * 8), 1);
            yield break;
        }

        var addressValue = address.GetAsUInt();
        this.currentlySelectedAddress = addressValue;
        this.hasSelectedAddress = true;
        var enabled = enable.GetAsBool();

        if (enabled)
        {
            var data = this._data.Memory.GetBytes(addressValue, this._data.WordSize);
            yield return (pins.Get("DATA"), data.GetAsLogicValues(this._data.WordSize * 8, false), 1);
        }
        else
        {
            yield return (pins.Get("DATA"), LogicValue.Z.Multiple(this._data.WordSize * 8), 1);
        }

        yield break;
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("ADDRESS", this._data.AddressBits, true, new Vector2i(0, 1));
        yield return new PinConfig("ENABLE", 1, true, new Vector2i(1, 0));
        yield return new PinConfig("DATA", this._data.WordSize * 8, false, new Vector2i(this.GetSize().X, 1));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 2);
    }

    public override void Initialize(RomData data)
    {
        if (this._data is not null && data.AddressBits != this._data.AddressBits && data.Memory == this._data.Memory)
        {
            data.Memory = new WordAddressableMemory((int)Math.Pow(2, data.AddressBits), false);
        }

        this._data = data;
        this._data.Memory = data.Memory;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break;
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

        var addressesToHighlight = new List<(uint, ColorF)>();

        if (this.hasSelectedAddress)
        {
            addressesToHighlight.Add((this.currentlySelectedAddress, Constants.COLOR_SELECTED));
        }

        this.memoryEditor.DrawWindow($"Read Only Memory Editor##{id}", this._data.Memory, this._data.WordSize, addressesToHighlight.ToArray(), () =>
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
                        var setMemory = new CModifyComponentDataProp(this.ID, this._data.GetType().GetProperty(nameof(this._data.Memory)), new WordAddressableMemory(data));
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
                    var setMemory = new CModifyComponentDataProp(this.ID, this._data.GetType().GetProperty(nameof(this._data.Memory)), new WordAddressableMemory(data));
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
}