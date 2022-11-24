using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics.UI;

namespace LogiX.Architecture.BuiltinComponents;

public class RomData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Address Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int AddressBits { get; set; }

    // Handled internally.
    public ByteAddressableMemory Memory { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new RomData()
        {
            AddressBits = 8,
            Memory = new ByteAddressableMemory(256, false),
        };
    }
}

[ScriptType("ROM"), ComponentInfo("ROM", "Memory", "core.markdown.rom")]
public class ROM : Component<RomData>
{
    public override string Name => "ROM";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private RomData _data;
    private string _pathLoadedFrom;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(RomData data)
    {
        this.ClearIOs();
        this._data = data;
        this._data.Memory = data.Memory;

        this.RegisterIO("ADDRESS", data.AddressBits, ComponentSide.LEFT, "address");
        this.RegisterIO("ENABLE", 1, ComponentSide.LEFT, "enable");
        this.RegisterIO("DATA", 8, ComponentSide.RIGHT); // ALWAYS A BYTE

        this.TriggerSizeRecalculation();
    }

    private bool hasSelectedAddress = false;
    private uint currentlySelectedAddress = 0;
    public override void PerformLogic()
    {
        var a = this.GetIOFromIdentifier("ADDRESS").GetValues();
        var en = this.GetIOFromIdentifier("ENABLE").GetValues().First();
        var q = this.GetIOFromIdentifier("DATA");

        this.hasSelectedAddress = false;

        if (a.AnyUndefined())
        {
            return; // Do nothing
        }

        var address = a.Reverse().GetAsUInt();
        this.currentlySelectedAddress = address;

        this.hasSelectedAddress = true;
        var value = this._data.Memory[address];
        var valueAsBits = value.GetAsLogicValues(8);

        if (en.IsUndefined() || en == LogicValue.LOW)
        {
            return;
        }

        q.Push(valueAsBits);
    }

    private MemoryEditor memoryEditor = new MemoryEditor(false);
    public override void SubmitUISelected(Editor editor, int componentIndex)
    {

    }

    public override void CompleteSubmitUISelected(Editor editor, int componentIndex)
    {
        var id = this.GetUniqueIdentifier();
        this.memoryEditor.DrawWindow($"Read Only Memory Editor##{id}", this._data.Memory, 1, this.currentlySelectedAddress, this.hasSelectedAddress, () =>
        {
            this.SubmitUISelected(editor, componentIndex);

            if (ImGui.Button($"Load From File##{id}"))
            {
                var fileDialog = new FileDialog(FileDialog.LastDirectory, FileDialogType.SelectFile, (path) =>
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
                var fileDialog = new FileDialog(FileDialog.LastDirectory, FileDialogType.SaveFile, (path) =>
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
}