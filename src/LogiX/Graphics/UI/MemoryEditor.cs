using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;
using ImGuiNET;
using LogiX.GLFW;

namespace LogiX.Graphics.UI;

public class ByteAddressableMemory
{
    public byte[] Data { get; set; }

    [JsonConstructor]
    public ByteAddressableMemory(byte[] data)
    {
        this.Data = data;
    }

    public ByteAddressableMemory(int size, bool randomize = true)
    {
        this.Data = new byte[size];
        if (randomize)
        {
            this.Randomize();
        }
    }

    public byte this[int index]
    {
        get => this.Data[index];
        set => this.Data[index] = value;
    }

    public void Reset()
    {
        for (int i = 0; i < this.Data.Length; i++)
        {
            this.Data[i] = 0;
        }
    }

    public List<LogicValue> GetLogicValuesAtAddress(int address, int databits)
    {
        List<LogicValue> values = new List<LogicValue>(databits);
        for (int i = 0; i < databits / 8; i++)
        {
            values.AddRange(this.Data[address + i].GetAsLogicValues(8));
        }
        return values;
    }

    public void Randomize()
    {
        Random random = new Random();
        for (int i = 0; i < this.Data.Length; i++)
        {
            this.Data[i] = (byte)random.Next(0, 256);
        }
    }
}

public class MemoryEditor
{
    private int Cols = 8;
    private bool ReadOnly { get; set; }

    private int EditingAddress { get; set; }
    private bool Editing { get; set; }
    private string DataInputString;

    public MemoryEditor(bool readOnly)
    {
        this.EditingAddress = -1;
        this.Editing = false;
        this.ReadOnly = readOnly;
        this.DataInputString = "";
    }

    private struct Sizes
    {
        public int AddressDigitsCount;
        public float LineHeight;
        public float GlyphWidth;
        public float HexCellWidth;
        public float SpacingBetweenMidCols;
        public float PosHexStart;
        public float PosHexEnd;
        public float PosAsciiStart;
        public float PosAsciiEnd;
        public float WindowWidth;
    }

    private unsafe Sizes CalcSizes(ByteAddressableMemory memory)
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        Sizes size = new Sizes();
        size.AddressDigitsCount = (int)Math.Ceiling(Math.Log(memory.Data.Length, 16));
        size.LineHeight = ImGui.GetTextLineHeight();
        size.GlyphWidth = ImGui.CalcTextSize("F").X + 1;
        size.HexCellWidth = (float)(int)(size.GlyphWidth * 2.5f);
        size.SpacingBetweenMidCols = (float)(int)(size.HexCellWidth * 0.25f);
        size.PosHexStart = (size.AddressDigitsCount + 2) * size.GlyphWidth;
        size.PosHexEnd = size.PosHexStart + (size.HexCellWidth * Cols);
        size.PosAsciiStart = size.PosAsciiEnd = size.PosHexEnd;

        size.PosAsciiStart = size.PosHexEnd + size.GlyphWidth * 1;
        size.PosAsciiStart += (float)((Cols + 8 - 1) / 8) * size.SpacingBetweenMidCols;
        size.PosAsciiEnd = size.PosAsciiStart + (size.GlyphWidth * Cols);
        size.WindowWidth = size.PosAsciiEnd + style.ScrollbarSize + style.WindowPadding.X * 2 + size.GlyphWidth;

        return size;
    }

    public void DrawWindow(string title, ByteAddressableMemory memory, int bytesPerAddress, int currentlySelectedAddress = -1, Action beforeContent = null, Action afterContent = null)
    {
        var s = CalcSizes(memory);
        ImGui.SetNextWindowSize(new Vector2(s.WindowWidth, s.WindowWidth * 0.60f), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(0, 0), new Vector2(s.WindowWidth, s.WindowWidth * 2f));

        if (ImGui.Begin(title, ImGuiWindowFlags.NoScrollbar))
        {
            if (beforeContent is not null)
            {
                beforeContent();
            }
            DrawContents(memory, bytesPerAddress, currentlySelectedAddress);
            if (afterContent is not null)
            {
                afterContent();
            }
        }
        ImGui.End();
    }

    public unsafe void DrawContents(ByteAddressableMemory memory, int bytesPerAddress, int currentlySelectedAddress)
    {
        var s = CalcSizes(memory);
        var style = ImGui.GetStyle();
        this.Cols = 8;

        float heightSeparator = style.ItemSpacing.Y;
        float footerHeight = 0f;

        ImGui.BeginChild("##scrolling", new Vector2(0, -footerHeight), false, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoNav);
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

        int lineTotalCount = (memory.Data.Length + Cols - 1) / Cols;
        ImGuiListClipper clipper = new ImGuiListClipper();
        ImGuiNative.ImGuiListClipper_Begin(&clipper, lineTotalCount, s.LineHeight);

        Vector2 windowPos = ImGui.GetWindowPos();
        drawList.AddLine(new Vector2(windowPos.X + s.PosAsciiStart - s.GlyphWidth, windowPos.Y), new Vector2(windowPos.X + s.PosAsciiStart - s.GlyphWidth, windowPos.Y + 9999), ImGui.GetColorU32(ImGuiCol.Border));

        uint colorText = ImGui.GetColorU32(ImGuiCol.Text);
        uint colorDisabled = ImGui.GetColorU32(ImGuiCol.TextDisabled);

        while (ImGuiNative.ImGuiListClipper_Step(&clipper) != 0)
        {
            for (int line = clipper.DisplayStart; line < clipper.DisplayEnd; line++)
            {
                var addr = line * Cols;

                ImGui.Text($"{(addr / bytesPerAddress).ToString("X" + s.AddressDigitsCount)}");

                for (int n = 0; n < Cols && addr < memory.Data.Length; n++, addr++)
                {
                    float bytePosX = s.PosHexStart + s.HexCellWidth * n;
                    bytePosX += (float)(n / 8) * s.SpacingBetweenMidCols;
                    ImGui.SameLine(bytePosX);

                    if (addr >= currentlySelectedAddress && addr < currentlySelectedAddress + bytesPerAddress)
                    {
                        var currentPos = ImGui.GetCursorScreenPos();
                        drawList.AddRectFilled(currentPos - new Vector2(s.SpacingBetweenMidCols / 2f, 0), currentPos + new Vector2(s.GlyphWidth * 2 + 1, s.LineHeight), ImGui.GetColorU32(Constants.COLOR_SELECTED.ToVector4()));
                    }

                    if (this.Editing && this.EditingAddress == addr)
                    {
                        // EDITING THIS ONE
                        ImGui.SetKeyboardFocusHere(0);
                        ImGui.PushItemWidth(ImGui.CalcTextSize("FF").X);
                        if (ImGui.InputText("##data", ref this.DataInputString, 2, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.AlwaysOverwrite | ImGuiInputTextFlags.NoHorizontalScroll | ImGuiInputTextFlags.AutoSelectAll))
                        {
                            if (this.DataInputString.Length == 2)
                            {
                                memory.Data[addr] = Convert.ToByte(this.DataInputString, 16);
                                this.Editing = false;
                                this.EditingAddress = -1;
                                this.DataInputString = "";
                            }
                        }
                    }
                    else
                    {
                        // JUST DISPLAY I GUESS?
                        ImGui.Text($"{memory.Data[addr]:X2}");

                        if (!ReadOnly && ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            Console.WriteLine($"Want to edit {addr:X}");
                            this.Editing = true;
                            this.EditingAddress = addr;
                            this.DataInputString = $"{memory.Data[addr]:X2}";
                        }
                    }
                }

                ImGui.SameLine(s.PosAsciiStart);
                Vector2 pos = ImGui.GetCursorScreenPos();
                addr = line * Cols;

                for (int n = 0; n < Cols && addr < memory.Data.Length; n++, addr++)
                {
                    byte c = memory.Data[addr];
                    if (c < 32 || c >= 128)
                        c = 46;

                    bool displayC = c != 46;

                    // Assume c to be ASCII
                    drawList.AddText(new Vector2(pos.X + s.GlyphWidth * n, pos.Y), displayC ? colorText : colorDisabled, new string((char)c, 1));
                }
                ImGui.NewLine();
            }
        }
        ImGui.PopStyleVar(2);
        ImGui.EndChild();

        ImGui.SetCursorPosX(s.WindowWidth);
    }
}