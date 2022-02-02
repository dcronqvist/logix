using LogiX.SaveSystem;
using System.Globalization;

namespace LogiX.Components;

public class MemoryComponent : Component
{
    public int AddressBits { get; set; }
    public int DataBits { get; set; }
    public bool MultibitAddress { get; set; }
    public bool MultibitOutput { get; set; }

    public List<LogicValue>[] Memory { get; set; }

    public override bool DrawIOIdentifiers => true;
    public override string Text => $"{1 << this.AddressBits}x{this.DataBits} Bit Memory";
    public override bool HasContextMenu => true;

    public bool previousClock = false;
    public MemoryEditor memEditor;

    private int currentAddress = 0;

    public MemoryComponent(int addressBits, bool multibitAddress, int dataBits, bool multibitOutput, Vector2 position) : base(multibitAddress ? Util.Listify(addressBits, dataBits, 1, 1, 1) : Util.NValues(1, addressBits + dataBits + 3), multibitOutput ? Util.Listify(dataBits) : Util.NValues(1, dataBits), position)
    {
        this.AddressBits = addressBits;
        this.DataBits = dataBits;

        this.MultibitAddress = multibitAddress;
        this.MultibitOutput = multibitOutput;

        this.Memory = new List<LogicValue>[1 << addressBits];

        if (multibitAddress)
        {
            this.Inputs[0].Identifier = $"A{addressBits - 1}-A0";
            this.Inputs[1].Identifier = $"D{dataBits - 1}-D0";
            this.Inputs[2].Identifier = "LOAD";
            this.Inputs[3].Identifier = "CLK";
            this.Inputs[4].Identifier = "R";
        }
        else
        {
            for (int i = 0; i < addressBits; i++)
            {
                this.Inputs[i].Identifier = $"A{i}";
            }
            for (int i = 0; i < dataBits; i++)
            {
                this.Inputs[addressBits + i].Identifier = $"D{i}";
            }
            this.Inputs[addressBits + dataBits].Identifier = "LOAD";
            this.Inputs[addressBits + dataBits + 1].Identifier = "CLK";
            this.Inputs[addressBits + dataBits + 2].Identifier = "R";
        }

        if (multibitOutput)
        {
            this.Outputs[0].Identifier = $"D{dataBits - 1}-D0";
        }
        else
        {
            for (int i = 0; i < dataBits; i++)
            {
                this.Outputs[i].Identifier = $"D{i}";
            }
        }
        this.memEditor = new MemoryEditor();

        this.ResetMemory();
    }

    public void ResetMemory()
    {
        for (int i = 0; i < this.Memory.Length; i++)
        {
            this.Memory[i] = Util.NValues(LogicValue.LOW, this.DataBits);
        }
    }

    public override void PerformLogic()
    {
        int address = 0;
        LogicValue[] data = new LogicValue[this.DataBits];
        bool loading = false;
        bool clk = false;
        bool r = false;
        if (this.MultibitAddress)
        {
            for (int i = 0; i < this.AddressBits; i++)
            {
                address += this.Inputs[0].Values[i] == LogicValue.HIGH ? (1 << (i)) : 0;
            }
            for (int i = 0; i < this.DataBits; i++)
            {
                data[i] = this.Inputs[1].Values[i];
            }
            loading = this.Inputs[2].Values[0] == LogicValue.HIGH;
            clk = this.Inputs[3].Values[0] == LogicValue.HIGH;
            r = this.Inputs[4].Values[0] == LogicValue.HIGH;
        }
        else
        {
            for (int i = 0; i < this.AddressBits; i++)
            {
                address += this.Inputs[i].Values[0] == LogicValue.HIGH ? (1 << (i)) : 0;
            }
            for (int i = 0; i < this.DataBits; i++)
            {
                data[i] = this.Inputs[this.AddressBits + i].Values[0];
            }
            loading = this.Inputs[this.AddressBits + this.DataBits].Values[0] == LogicValue.HIGH;
            clk = this.Inputs[this.AddressBits + this.DataBits + 1].Values[0] == LogicValue.HIGH;
            r = this.Inputs[this.AddressBits + this.DataBits + 2].Values[0] == LogicValue.HIGH;
        }

        this.currentAddress = address;
        if (r)
        {
            this.ResetMemory();
            if (this.MultibitOutput)
            {
                for (int i = 0; i < this.DataBits; i++)
                {
                    this.Outputs[0].Values[i] = this.Memory[address][i];
                }
            }
            else
            {
                for (int i = 0; i < this.DataBits; i++)
                {
                    this.Outputs[i].Values[0] = this.Memory[address][i];
                }
            }
            return;
        }

        if (loading && clk && !previousClock)
        {
            this.Memory[address] = data.ToList();
        }

        // Set output to data att current address
        if (this.MultibitOutput)
        {
            for (int i = 0; i < this.DataBits; i++)
            {
                this.Outputs[0].Values[i] = this.Memory[address][i];
            }
        }
        else
        {
            for (int i = 0; i < this.DataBits; i++)
            {
                this.Outputs[i].Values[0] = this.Memory[address][i];
            }
        }

        previousClock = clk;
    }

    public override ComponentDescription ToDescription()
    {
        List<IODescription> inputs = this.Inputs.Select(i => new IODescription(i.Bits)).ToList();
        List<IODescription> outputs = this.Outputs.Select(i => new IODescription(i.Bits)).ToList();
        return new MemoryDescription(this.Position, this.Rotation, this.Memory, inputs, outputs);
    }

    public override void SubmitContextPopup(Editor.Editor editor)
    {
        base.SubmitContextPopup(editor);

        if (ImGui.Button("Dump Memory to File..."))
        {
            editor.SelectFolder(Util.FileDialogStartDir, folder =>
            {
                using (StreamWriter sw = new StreamWriter(folder + "/memory.txt"))
                {
                    for (int i = 0; i < this.Memory.Length; i++)
                    {
                        sw.WriteLine(Util.LogicValuesToBinaryString(this.Memory[i]));
                    }
                }
            });
        }

        if (ImGui.Button("Load Memory From File..."))
        {
            editor.SelectFile(Util.FileDialogStartDir, file =>
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    this.ResetMemory();
                    this.Memory = Util.ReadROM(file).ToArray();
                }
            }, ".txt");
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.GateAmount(("Memory", 1));
    }

    public override void OnSingleSelectedSubmitUI()
    {
        byte[] bytes = new byte[this.Memory.Length];
        bytes = this.Memory.Select(l => l.GetAsByte()).ToArray();
        ImGui.PushFont(ImGui.GetIO().FontDefault);
        this.memEditor.Draw("Memory Editor", bytes, bytes.Length, this.currentAddress, 0);
        ImGui.PopFont();

        List<List<LogicValue>> memory = new List<List<LogicValue>>();
        for (int i = 0; i < this.Memory.Length; i++)
        {
            memory.Add(bytes[i].GetAsLogicValues(this.DataBits));
        }
        this.Memory = memory.ToArray();
    }
}


// C# port of ocornut's imgui_memory_editor.h - https://gist.github.com/ocornut/0673e37e54aff644298b

// Mini memory editor for ImGui (to embed in your game/tools)
// v0.10
// Animated gif: https://cloud.githubusercontent.com/assets/8225057/9028162/3047ef88-392c-11e5-8270-a54f8354b208.gif
//
// You can adjust the keyboard repeat delay/rate in ImGuiIO.
// The code assume a mono-space font for simplicity! If you don't use the default font, use ImGui::PushFont()/PopFont() to switch to a mono-space font before caling this.
//
// Usage:
//   static MemoryEditor memory_editor;                                                     // save your state somewhere
//   memory_editor.Draw("Memory Editor", mem_block, mem_block_size, (size_t)mem_block);     // run

public class MemoryEditor
{
    bool AllowEdits;
    int Rows;
    int DataEditingAddr;
    bool DataEditingTakeFocus;
    byte[] DataInput = new byte[32];
    byte[] AddrInput = new byte[32];

    public MemoryEditor()
    {
        Rows = 8;
        DataEditingAddr = -1;
        DataEditingTakeFocus = false;
        AllowEdits = true;
    }

    static string FixedHex(int v, int count)
    {
        return v.ToString("X").PadLeft(count, '0');
    }

    static bool TryHexParse(byte[] bytes, out int result)
    {
        string input = System.Text.Encoding.UTF8.GetString(bytes).ToString();
        return int.TryParse(input, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out result);
    }

    static void ReplaceChars(byte[] bytes, string input)
    {
        var address = System.Text.Encoding.ASCII.GetBytes(input);
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (i < address.Length) ? address[i] : (byte)0;
        }
    }

    public unsafe void Draw(string title, byte[] mem_data, int mem_size, int currentlySelectedMemoryAddress = -1, int base_display_addr = 0)
    {
        ImGui.SetNextWindowSize(new Vector2(500, 350), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin(title, ImGuiWindowFlags.NoNav))
        {
            ImGui.End();
            return;
        }

        float line_height = ImGuiNative.igGetTextLineHeight();
        int line_total_count = (mem_size + Rows - 1) / Rows;

        ImGuiNative.igSetNextWindowContentSize(new Vector2(0.0f, line_total_count * line_height));
        ImGui.BeginChild("##scrolling", new Vector2(0, -ImGuiNative.igGetFrameHeightWithSpacing()), false, 0);

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

        int addr_digits_count = 0;
        for (int n = base_display_addr + mem_size - 1; n > 0; n >>= 4)
            addr_digits_count++;

        float glyph_width = ImGui.CalcTextSize("F").X;
        // "FF " we include trailing space in the width to easily catch clicks everywhere
        float cell_width = glyph_width * 3;

        var clipper = new ImGuiListClipper2(line_total_count, line_height);
        int visible_start_addr = clipper.DisplayStart * Rows;
        int visible_end_addr = clipper.DisplayEnd * Rows;

        bool data_next = false;

        if (!AllowEdits || DataEditingAddr >= mem_size)
            DataEditingAddr = -1;

        int data_editing_addr_backup = DataEditingAddr;

        if (DataEditingAddr != -1)
        {
            if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.UpArrow)) && DataEditingAddr >= Rows)
            {
                DataEditingAddr -= Rows;
                DataEditingTakeFocus = true;
            }
            else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.DownArrow)) && DataEditingAddr < mem_size - Rows)
            {
                DataEditingAddr += Rows;
                DataEditingTakeFocus = true;
            }
            else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.LeftArrow)) && DataEditingAddr > 0)
            {
                DataEditingAddr -= 1;
                DataEditingTakeFocus = true;
            }
            else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.RightArrow)) && DataEditingAddr < mem_size - 1)
            {
                DataEditingAddr += 1;
                DataEditingTakeFocus = true;
            }
        }
        if ((DataEditingAddr / Rows) != (data_editing_addr_backup / Rows))
        {
            // Track cursor movements
            float scroll_offset = ((DataEditingAddr / Rows) - (data_editing_addr_backup / Rows)) * line_height;
            bool scroll_desired = (scroll_offset < 0.0f && DataEditingAddr < visible_start_addr + Rows * 2) || (scroll_offset > 0.0f && DataEditingAddr > visible_end_addr - Rows * 2);
            if (scroll_desired)
                ImGuiNative.igSetScrollYFloat(ImGuiNative.igGetScrollY() + scroll_offset);
        }

        // display only visible items
        for (int line_i = clipper.DisplayStart; line_i < clipper.DisplayEnd; line_i++)
        {
            int addr = line_i * Rows;
            ImGui.Text(FixedHex(base_display_addr + addr, addr_digits_count) + ": ");
            ImGui.SameLine();

            // Draw Hexadecimal
            float line_start_x = ImGuiNative.igGetCursorPosX();
            for (int n = 0; n < Rows && addr < mem_size; n++, addr++)
            {
                ImGui.SameLine(line_start_x + cell_width * n);

                if (DataEditingAddr == addr)
                {
                    // Display text input on current byte
                    ImGui.PushID(addr);

                    // FIXME: We should have a way to retrieve the text edit cursor position more easily in the API, this is rather tedious.
                    ImGuiInputTextCallback callback = (data) =>
                    {
                        int* p_cursor_pos = (int*)data->UserData;

                        if (ImGuiNative.ImGuiInputTextCallbackData_HasSelection(data) == 0)
                            *p_cursor_pos = data->CursorPos;
                        return 0;
                    };
                    int cursor_pos = -1;
                    bool data_write = false;
                    if (DataEditingTakeFocus)
                    {
                        ImGui.SetKeyboardFocusHere();
                        ReplaceChars(DataInput, FixedHex(mem_data[addr], 2));
                        ReplaceChars(AddrInput, FixedHex(base_display_addr + addr, addr_digits_count));
                    }
                    ImGui.PushItemWidth(ImGui.CalcTextSize("FF").X);

                    var flags = ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoHorizontalScroll | ImGuiInputTextFlags.AlwaysInsertMode | ImGuiInputTextFlags.CallbackAlways;

                    if (ImGui.InputText("##data", DataInput, 32, flags, callback, (IntPtr)(&cursor_pos)))
                        data_write = data_next = true;
                    else if (!DataEditingTakeFocus && !ImGui.IsItemActive())
                        DataEditingAddr = -1;

                    DataEditingTakeFocus = false;
                    ImGui.PopItemWidth();
                    if (cursor_pos >= 2)
                        data_write = data_next = true;
                    if (data_write)
                    {
                        int data;
                        if (TryHexParse(DataInput, out data))
                            mem_data[addr] = (byte)data;
                    }
                    ImGui.PopID();
                }
                else
                {
                    if (addr == currentlySelectedMemoryAddress)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Color.GREEN.ToVector4());
                    }
                    ImGui.Text(FixedHex(mem_data[addr], 2));
                    if (addr == currentlySelectedMemoryAddress)
                    {
                        ImGui.PopStyleColor();
                    }
                    if (AllowEdits && ImGui.IsItemHovered() && ImGui.IsMouseClicked(0))
                    {
                        DataEditingTakeFocus = true;
                        DataEditingAddr = addr;
                    }
                }
            }

            ImGui.SameLine(line_start_x + cell_width * Rows + glyph_width * 2);
            // separator line drawing replaced by printing a pipe char

            // Draw ASCII values
            addr = line_i * Rows;
            var asciiVal = new System.Text.StringBuilder(2 + Rows);
            asciiVal.Append("| ");
            for (int n = 0; n < Rows && addr < mem_size; n++, addr++)
            {
                int c = mem_data[addr];
                asciiVal.Append((c >= 32 && c < 128) ? Convert.ToChar(c) : '.');
            }
            ImGui.TextUnformatted(asciiVal.ToString());  // use unformatted, so string can contain the '%' character
        }
        // clipper.End();  // not implemented
        ImGui.PopStyleVar(2);

        ImGui.EndChild();

        if (data_next && DataEditingAddr < mem_size)
        {
            DataEditingAddr = DataEditingAddr + 1;
            DataEditingTakeFocus = true;
        }

        ImGui.Separator();

        ImGuiNative.igAlignTextToFramePadding();
        ImGui.PushItemWidth(50);
        ImGui.PushAllowKeyboardFocus(true);
        int rows_backup = Rows;
        if (ImGui.DragInt("##rows", ref Rows, 0.2f, 4, 32, "%.0f rows"))
        {
            if (Rows <= 0)
                Rows = 4;
            Vector2 new_window_size = ImGui.GetWindowSize();
            new_window_size.X += (Rows - rows_backup) * (cell_width + glyph_width);
            ImGui.SetWindowSize(new_window_size);
        }
        ImGui.PopAllowKeyboardFocus();
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.Text(string.Format(" Range {0}..{1} ", FixedHex(base_display_addr, addr_digits_count),
            FixedHex(base_display_addr + mem_size - 1, addr_digits_count)));
        ImGui.SameLine();
        ImGui.PushItemWidth(70);
        if (ImGui.InputText("##addr", AddrInput, 32, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.EnterReturnsTrue, null))
        {
            int goto_addr;
            if (TryHexParse(AddrInput, out goto_addr))
            {
                goto_addr -= base_display_addr;
                if (goto_addr >= 0 && goto_addr < mem_size)
                {
                    ImGui.BeginChild("##scrolling");
                    ImGui.SetScrollFromPosY(ImGui.GetCursorStartPos().Y + (goto_addr / Rows) * ImGuiNative.igGetTextLineHeight());
                    ImGui.EndChild();
                    DataEditingAddr = goto_addr;
                    DataEditingTakeFocus = true;
                }
            }
        }
        ImGui.PopItemWidth();

        ImGui.End();
    }
}

// Not a proper translation, because ImGuiListClipper uses imgui's internal api.
// Thus SetCursorPosYAndSetupDummyPrevLine isn't reimplemented, but SetCursorPosY + SetNextWindowContentSize seems to be working well instead.
// TODO expose clipper through newer cimgui version
internal class ImGuiListClipper2
{
    public float StartPosY;
    public float ItemsHeight;
    public int ItemsCount, StepNo, DisplayStart, DisplayEnd;

    public ImGuiListClipper2(int items_count = -1, float items_height = -1.0f)
    {
        Begin(items_count, items_height);
    }

    public unsafe void Begin(int count, float items_height = -1.0f)
    {
        StartPosY = ImGuiNative.igGetCursorPosY();
        ItemsHeight = items_height;
        ItemsCount = count;
        StepNo = 0;
        DisplayEnd = DisplayStart = -1;
        if (ItemsHeight > 0.0f)
        {
            int dispStart, dispEnd;
            ImGuiNative.igCalcListClipping(ItemsCount, ItemsHeight, &dispStart, &dispEnd);
            DisplayStart = dispStart;
            DisplayEnd = dispEnd;
            if (DisplayStart > 0)
                //SetCursorPosYAndSetupDummyPrevLine(StartPosY + DisplayStart * ItemsHeight, ItemsHeight); // advance cursor
                ImGuiNative.igSetCursorPosY(StartPosY + DisplayStart * ItemsHeight);
            StepNo = 2;
        }
    }
}
