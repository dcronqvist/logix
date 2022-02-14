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
    private uint value;
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

    public HexViewer(int bits, bool multibit, Vector2 position) : base(multibit ? Util.Listify(bits) : Util.NValues(1, bits), Util.EmptyList<int>(), position)
    {
        hexString = "";
    }

    public string GetHexadecimalStringWithMode(uint value, HexViewerMode mode, int bits)
    {
        switch (mode)
        {
            case HexViewerMode.Binary:
                return value.ToString("X" + bits / 4);
            case HexViewerMode.TwosComplement:
                // Convert from two's complement representation to hexadecima
                if (value >= (1 << (bits - 1)))
                {
                    return "-" + (-value - (1 << bits)).ToString("X" + bits / 4).TrimStart('F', 'E');
                }
                return ((value)).ToString("X" + bits / 4);
            case HexViewerMode.OnesComplement:
                // Convert from one's complement representation to hexadecima
                if (value >= (1 << (bits - 1)))
                {
                    return "-" + (-value - 1).ToString("X" + bits / 4).TrimStart('F', 'E');
                }
                return ((value)).ToString("X" + bits / 4);
            default:
                return "";
        }
    }

    public override void PerformLogic()
    {
        this.value = 0;

        if (this.Inputs.Count == 1)
        {
            ComponentInput ci = this.InputAt(0);

            for (int i = 0; i < ci.Bits; i++)
            {
                value += ci.Values[i] == LogicValue.HIGH ? (1u << (i)) : 0u;
            }

            this.hexString = this.GetHexadecimalStringWithMode(value, this.Mode, ci.Bits);
        }
        else
        {
            for (int i = 0; i < this.Inputs.Count; i++)
            {
                value += this.InputAt(i).Values[0] == LogicValue.HIGH ? (1u << (i)) : 0u;
            }
            this.hexString = this.GetHexadecimalStringWithMode(value, this.Mode, this.Inputs.Count);
        }
    }

    public override void OnSingleSelectedSubmitUI()
    {
        ImGui.Begin("HexViewer", ImGuiWindowFlags.AlwaysAutoResize);

        int mode = (int)this.Mode;
        ImGui.Combo("Mode", ref mode, "Binary\0One's Complement\0Two's Complement\0");
        this.Mode = (HexViewerMode)mode;

        ImGui.End();
    }

    public override void Render(Vector2 mousePosInWorld)
    {
        this.RenderBox();
        this.RenderIOs(mousePosInWorld);
        this.RenderComponentText(mousePosInWorld, 35, true);
    }

    public override ComponentDescription ToDescription()
    {
        if (this.Inputs.Count != 1)
        {
            return new GenIODescription(this.Position, this.Rotation, Util.NValues(new IODescription(1), this.Inputs.Count), Util.EmptyList<IODescription>(), ComponentType.HexViewer);
        }
        else
        {
            return new GenIODescription(this.Position, this.Rotation, Util.Listify(new IODescription(this.Inputs[0].Bits)), Util.EmptyList<IODescription>(), ComponentType.HexViewer);
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.EmptyGateAmount();
    }
}