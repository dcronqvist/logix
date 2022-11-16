using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics.UI;

namespace LogiX.Architecture.BuiltinComponents;

public class DiskData : IComponentDescriptionData
{
    public string FilePath { get; set; }
    public int BlockAddressBits { get; set; } // The addresses of the blocks
    public int BlockSize { get; set; } // How big a single block is in bytes

    public static IComponentDescriptionData GetDefault()
    {
        return new DiskData()
        {
            BlockAddressBits = 8,
            BlockSize = 1,
            FilePath = null
        };
    }
}

[ScriptType("DISK"), ComponentInfo("Disk", "Input/Output", "core.markdown.disk")]
public class Disk : Component<DiskData>
{
    public override string Name => "DISK";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private DiskData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(DiskData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("ADDR", data.BlockAddressBits, ComponentSide.LEFT);
        this.RegisterIO("BUSY", 1, ComponentSide.BOTTOM);
        this.RegisterIO("DATAWRITE", data.BlockSize * 8, ComponentSide.LEFT);
        this.RegisterIO("DATAREAD", data.BlockSize * 8, ComponentSide.RIGHT);
        this.RegisterIO("WRITE", 1, ComponentSide.TOP);
        this.RegisterIO("READ", 1, ComponentSide.TOP);

        this.TriggerSizeRecalculation();
    }

    private Mutex _mutex = new Mutex();
    private bool _busy = false;
    private byte[] _outputBuffer = new byte[0];

    private bool previousWrite = false;
    private bool previousRead = false;

    public override void PerformLogic()
    {
        // Inputs
        var addr = this.GetIOFromIdentifier("ADDR").GetValues();
        var datawrite = this.GetIOFromIdentifier("DATAWRITE").GetValues();
        var write = this.GetIOFromIdentifier("WRITE").GetValues().First();
        var read = this.GetIOFromIdentifier("READ").GetValues().First();

        // Output
        var busy = this.GetIOFromIdentifier("BUSY");
        var dataread = this.GetIOFromIdentifier("DATAREAD");

        if (addr.AnyUndefined() || datawrite.AnyUndefined() || write.IsUndefined() || read.IsUndefined() || this._data.FilePath is null || this._data.FilePath == "")
        {
            return; // Can't do anything
        }

        if (write == LogicValue.HIGH && read == LogicValue.HIGH)
        {
            return; // Can't do anything
        }

        if (write == LogicValue.HIGH && previousWrite == false)
        {
            // Write
            var address = addr.Reverse().GetAsUInt();
            var data = datawrite.Reverse().GetByteArray(this._data.BlockSize);
            _ = WriteAsync(data, address);
        }

        if (read == LogicValue.HIGH && previousRead == false)
        {
            // Read
            var address = addr.Reverse().GetAsUInt();
            _ = ReadAsync(address);
        }

        previousWrite = write == LogicValue.HIGH;
        previousRead = read == LogicValue.HIGH;

        // Output
        busy.Push(this._busy.ToLogicValue());

        if (this._outputBuffer.Length > 0)
        {
            dataread.Push(this._outputBuffer.GetLogicValues());
        }
    }

    private async Task WriteAsync(byte[] data, uint address)
    {
        _mutex.WaitOne();
        _busy = true;
        using (FileStream sw = File.Open(this._data.FilePath, FileMode.Open, FileAccess.Write, FileShare.Read))
        {
            sw.Seek(address, SeekOrigin.Begin);
            await sw.WriteAsync(data, 0, data.Length);
        }
        _busy = false;
        _mutex.ReleaseMutex();
    }

    private async Task ReadAsync(uint address)
    {
        _mutex.WaitOne();
        _busy = true;
        using (FileStream sr = File.Open(this._data.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            sr.Seek(address, SeekOrigin.Begin);
            _outputBuffer = new byte[this._data.BlockSize];
            await sr.ReadAsync(_outputBuffer, 0, this._data.BlockSize);
        }
        _busy = false;
        _mutex.ReleaseMutex();
    }

    private bool AssertFileHasCorrectSize(string path)
    {
        var fileInfo = new FileInfo(path);
        var fileSize = fileInfo.Length;
        var expectedSize = (1 << this._data.BlockAddressBits) * this._data.BlockSize;
        return fileSize == expectedSize;
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        var id = this.GetUniqueIdentifier();
        if (ImGui.Button($"Mount New File##{id}"))
        {
            var dialog = new FileDialog(".", FileDialogType.SaveFile, (path) =>
            {
                this._data.FilePath = path;

                // Create the file if it doesn't exist
                if (!File.Exists(path))
                {
                    var file = File.Create(path);
                    file.Write(new byte[(1 << this._data.BlockAddressBits) * this._data.BlockSize]);
                    file.Close();
                }

                this.Initialize(this._data);
            });

            editor.OpenPopup(dialog);
        }
        ImGui.SameLine();
        if (ImGui.Button($"Mount Existing File##{id}"))
        {
            var dialog = new FileDialog(".", FileDialogType.SelectFile, (path) =>
            {
                this._data.FilePath = path;

                // Create the file if it doesn't exist
                if (!AssertFileHasCorrectSize(path))
                {
                    throw new Exception("File size does not match the disk size");
                }

                this.Initialize(this._data);
            });

            editor.OpenPopup(dialog);
        }
        ImGui.SameLine();

        ImGui.TextDisabled($"File: {this._data.FilePath ?? "None"}");

        ImGui.Separator();
        var blockAddressBits = this._data.BlockAddressBits;
        if (ImGui.InputInt($"Block Address Bits##{id}", ref blockAddressBits, 1, 1))
        {
            this._data.BlockAddressBits = blockAddressBits;
            this.Initialize(this._data);
        }
        var blockSize = this._data.BlockSize;
        if (ImGui.InputInt($"Block Size##{id}", ref blockSize, 1, 1))
        {
            this._data.BlockSize = blockSize;
            this.Initialize(this._data);
        }
    }
}