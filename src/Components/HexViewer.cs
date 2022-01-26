using LogiX.SaveSystem;

namespace LogiX.Components;

public class HexViewer : Component
{
    private int value;
    private string hexString;
    public override string Text => this.hexString;
    public override Vector2 Size
    {
        get
        {
            float maxIOs = Math.Max(this.Inputs.Count, this.Outputs.Count);
            Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, this.Text, 35, 1);

            return new Vector2(MathF.Max(measure.X + 20f, 50f), MathF.Max(maxIOs * 25f, measure.Y + 10f));
        }
    }

    public HexViewer(int bits, bool multibit, Vector2 position) : base(multibit ? Util.Listify(bits) : Util.NValues(1, bits), Util.EmptyList<int>(), position)
    {
        hexString = "";
    }

    public override void PerformLogic()
    {
        this.value = 0;

        if (this.Inputs.Count == 1)
        {
            ComponentInput ci = this.InputAt(0);

            for (int i = 0; i < ci.Bits; i++)
            {
                value += ci.Values[i] == LogicValue.HIGH ? (1 << (i)) : 0;
            }

            this.hexString = value.ToString("X" + (int)MathF.Ceiling(ci.Bits / 4f));
        }
        else
        {
            for (int i = 0; i < this.Inputs.Count; i++)
            {
                value += this.InputAt(i).Values[0] == LogicValue.HIGH ? (1 << (i)) : 0;
            }
            this.hexString = value.ToString("X" + (int)MathF.Ceiling(this.Inputs.Count / 4f));
        }
    }

    public override void Render(Vector2 mousePosInWorld)
    {
        if (this.DrawBoxNormal)
        {
            Raylib.DrawRectanglePro(this.Box, Vector2.Zero, 0f, this.BodyColor);
            Raylib.DrawRectangleLinesEx(this.Box, 1, Color.BLACK);
        }

        this.RenderIO(GetInputLinePositions, this.Inputs.Cast<ComponentIO>().ToList(), mousePosInWorld);
        this.RenderIO(GetOutputLinePositions, this.Outputs.Cast<ComponentIO>().ToList(), mousePosInWorld);

        if (this.TextVisible)
        {
            int fontSize = 35;

            Vector2 middleOfBox = new Vector2(this.Box.x, this.Box.y) + new Vector2(this.Box.width / 2f, this.Box.height / 2f);
            Vector2 textSize = Raylib.MeasureTextEx(Util.OpenSans, this.Text, fontSize, 1);

            Raylib.DrawTextEx(Util.OpenSans, this.Text, middleOfBox - textSize / 2f, fontSize, 1, Color.BLACK);
        }
    }

    public override ComponentDescription ToDescription()
    {
        if (this.Inputs.Count != 1)
        {
            return new GenIODescription(this.Position, Util.NValues(new IODescription(1), this.Inputs.Count), Util.EmptyList<IODescription>(), ComponentType.HexViewer);
        }
        else
        {
            return new GenIODescription(this.Position, Util.Listify(new IODescription(this.Inputs[0].Bits)), Util.EmptyList<IODescription>(), ComponentType.HexViewer);
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.EmptyGateAmount();
    }
}