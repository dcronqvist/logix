using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics.UI;

namespace LogiX.Architecture.BuiltinComponents;

public class RomData : IComponentDescriptionData
{
    public ByteAddressableMemory Memory { get; set; }
    public int AddressBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new RomData()
        {
            AddressBits = 8,
            Memory = new ByteAddressableMemory(256, false),
        };
    }
}

[ScriptType("ROM"), ComponentInfo("ROM", "Memory")]
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

                        this._pathLoadedFrom = path;
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
            if (this._pathLoadedFrom is null)
                ImGui.BeginDisabled();

            if (ImGui.Button($"Reload##{id}"))
            {
                using (BinaryReader sr = new BinaryReader(File.Open(this._pathLoadedFrom, FileMode.Open)))
                {
                    var data = sr.ReadBytes((int)sr.BaseStream.Length);
                    var addressBits = (int)Math.Ceiling(Math.Log(data.Length, 2));

                    this._data.AddressBits = addressBits;

                    this.Initialize(this._data);

                    this._data.Memory = new ByteAddressableMemory(data);
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