using LogiX.SaveSystem;

namespace LogiX.Components;

public class Lamp : Component
{
    public LogicValue[] Values { get; set; }

    private int _bits;
    [ComponentProp("Bits", IntMin = 1)]
    public int Bits
    {
        get => _bits;
        set
        {
            if (this.Values is null)
            {
                this.Values = new LogicValue[value];
            }

            if (value > this._bits)
            {
                // Increasing amount
                this.Values = this.Values.Take(this._bits).Concat(Util.NValues(LogicValue.LOW, value - this._bits)).ToArray();
            }
            else
            {
                // Decreasing amount
                this.Values = this.Values.Take(value).ToArray();
            }

            _bits = value;
            this.GetIO(0).UpdateBitWidth(value);
        }
    }

    [ComponentProp("Integrated Side")]
    public ComponentSide Side { get; set; }

    [ComponentProp("Identifier")]
    public string Identifier { get; set; }

    public int WIDTH_PER_BITS = Util.GridSizeX * 2;
    public override Vector2 Size => new Vector2(WIDTH_PER_BITS * this.Bits, WIDTH_PER_BITS);
    public override bool DisplayText => false;

    public Lamp(int bits, Vector2 position, string? uniqueID = null) : base(position, ComponentType.LAMP, uniqueID)
    {
        this.AddIO(bits, new IOConfig(ComponentSide.LEFT));
        this.Bits = bits;
    }

    public override void PerformLogic()
    {
        this.Values = this.GetIO(0).Values;
    }

    public override void Render()
    {
        this.RenderIOs();
        this.RenderRectangle();

        Vector2 basePos = new Vector2(this.GetRectangle().x, this.GetRectangle().y);

        for (int i = 0; i < this.Bits; i++)
        {
            Rectangle rec = new Rectangle(basePos.X + WIDTH_PER_BITS * i, basePos.Y, WIDTH_PER_BITS, WIDTH_PER_BITS).Inflate(-2);

            Raylib.DrawRectangleRec(rec, this.Values[i].ToColor(lowColor: Color.LIGHTGRAY));
        }

        // Render label on the opposite side of the IO
        IO io = this.GetIO(0);
        ComponentSide oppositeSide = Util.GetRotatedComponentSide(this.IOs[0].Item2.Side, (this.Rotation + 2) % 4);
        Vector2 endPos = Vector2.Zero;

        // Calculate position of IO
        if (oppositeSide == ComponentSide.LEFT)
        {
            // LEFT
            endPos = new Vector2(basePos.X - Util.GridSizeX, basePos.Y + Util.GridSizeY);
        }
        else if (oppositeSide == ComponentSide.TOP)
        {
            // TOP
            endPos = new Vector2(basePos.X + Util.GridSizeX, basePos.Y - Util.GridSizeY);
        }
        else if (oppositeSide == ComponentSide.RIGHT)
        {
            // RIGHT
            endPos = new Vector2(basePos.X + this.Bits * WIDTH_PER_BITS + Util.GridSizeX, basePos.Y + Util.GridSizeY);
        }
        else if (oppositeSide == ComponentSide.BOTTOM)
        {
            // BOTTOM
            endPos = new Vector2(basePos.X + Util.GridSizeX, basePos.Y + this.Size.Y + Util.GridSizeY);
        }

        // Render label
        int labelFontSize = 12;

        Vector2 textMeasure = Raylib.MeasureTextEx(Util.OpenSans, this.Identifier, labelFontSize, 0);

        Util.RenderTextRotated(endPos - new Vector2(oppositeSide == ComponentSide.LEFT ? textMeasure.X : 0, textMeasure.Y / 2f), Util.OpenSans, labelFontSize, 0, this.Identifier, 0f, Color.BLACK);
    }

    public override ComponentDescription ToDescription()
    {
        return new DescriptionLamp(this.Position, this.Rotation, this.UniqueID, this.Bits, this.Identifier, this.Side);
    }
}