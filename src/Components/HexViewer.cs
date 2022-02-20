using LogiX.SaveSystem;

namespace LogiX.Components;

public enum HexViewerMode
{
    Binary,
    OnesComplement,
    TwosComplement,
}

public class HexViewer : Component
{
    private ulong value;
    private string hexString;
    public override string Text => this.hexString;
    public override Vector2 Size
    {
        get
        {
            float maxIOs = Math.Max(this.Inputs.Count, this.Outputs.Count);
            Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, this.Text, 35, 1);

            if (this.Rotation == 1 || this.Rotation == 3)
            {
                return new Vector2(MathF.Max(maxIOs * 25f, measure.Y + 10f), MathF.Max(measure.X + 20f, 50f));
            }

            return new Vector2(MathF.Max(measure.X + 20f, 50f), MathF.Max(maxIOs * 25f, measure.Y + 10f));
        }
    }
    public override string? Documentation => @"
# Hex Viewer

This component will display the input as a hexadecimal number. 

When configured as multibit, the most significant bit will be the leftmost bit, and the least significant bit will be the rightmost bit.

In a normal configuration, the most significant bit will be the one at the bottom, and the least significant bit will be the one at the top.
";
    public HexViewerMode Mode { get; set; } = HexViewerMode.Binary;
    public bool IncludesRepBits { get; set; }
    public bool Multibit { get; set; }
    private int bits;

    public HexViewer(int bits, bool multibit, bool includeRepresentationBits, Vector2 position) : base(multibit ? (includeRepresentationBits ? Util.Listify(2, bits) : Util.Listify(bits)) : (includeRepresentationBits ? Util.NValues(1, 2 + bits) : Util.NValues(1, bits)), Util.EmptyList<int>(), position)
    {
        hexString = "";
        this.IncludesRepBits = includeRepresentationBits;
        this.Multibit = multibit;
        this.bits = bits;
    }

    public string GetHexadecimalStringWithMode(ulong value, HexViewerMode mode, int bits)
    {
        switch (mode)
        {
            case HexViewerMode.Binary:
                return value.ToString("X" + bits / 4);
            case HexViewerMode.TwosComplement:
                // Convert from two's complement representation to hexadecima
                if (value >= (1UL << (bits - 1)))
                {
                    return "-" + (-((long)value) - (1L << (bits))).ToString("X" + bits / 4).Substring((16 - bits / 4), bits / 4);
                }
                return ((value)).ToString("X" + bits / 4);
            case HexViewerMode.OnesComplement:
                // Convert from one's complement representation to hexadecima
                if (value >= (1UL << (bits - 1)))
                {
                    return "-" + (-((long)value) - 1).ToString("X" + bits / 4).Substring((16 - bits / 4), bits / 4);
                }
                return ((value)).ToString("X" + bits / 4);
            default:
                return "-";
        }
    }

    public override void PerformLogic()
    {
        this.value = 0;

        HexViewerMode mode = this.Mode;

        if (this.Multibit)
        {
            if (this.IncludesRepBits)
            {
                byte modeValue = this.InputAt(0).Values.GetAsByte();
                mode = (HexViewerMode)modeValue;
                this.Mode = mode;
            }

            int inputBit = (this.IncludesRepBits ? 1 : 0);

            ComponentInput ci = this.InputAt(inputBit);

            for (int i = 0; i < ci.Bits; i++)
            {
                value += ci.Values[i] == LogicValue.HIGH ? (1UL << (i)) : 0UL;
            }

            this.hexString = this.GetHexadecimalStringWithMode(value, mode, ci.Bits);
        }
        else
        {
            if (this.IncludesRepBits)
            {
                byte modeValue = this.GetLogicValuesFromSingleBitInputs(0, 1).GetAsByte();
                mode = (HexViewerMode)modeValue;
                this.Mode = mode;
            }

            int inputBit = (this.IncludesRepBits ? 2 : 0);
            for (int i = inputBit; i < this.Inputs.Count; i++)
            {
                value += this.InputAt(i).Values[0] == LogicValue.HIGH ? (1UL << (i)) : 0UL;
            }
            this.hexString = this.GetHexadecimalStringWithMode(value, this.Mode, this.Inputs.Count);
        }
    }

    public override void SubmitContextPopup(Editor.Editor editor)
    {
        int mode = (int)this.Mode;
        ImGui.Combo("Mode", ref mode, "Binary\0One's Complement\0Two's Complement\0");
        if (!this.IncludesRepBits)
        {
            this.Mode = (HexViewerMode)mode;
        }
        base.SubmitContextPopup(editor);
    }

    public override void Render(Vector2 mousePosInWorld)
    {
        this.RenderBox();
        this.RenderIOs(mousePosInWorld);
        this.RenderComponentText(mousePosInWorld, 35, true);
    }

    public override ComponentDescription ToDescription()
    {
        return new HexViewerDescription(this.Position, this.Rotation, this.bits, this.Multibit, this.IncludesRepBits, this.Inputs.Select(x => new IODescription(x.Bits)).ToList(), this.Outputs.Select(x => new IODescription(x.Bits)).ToList());
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.EmptyGateAmount();
    }
}