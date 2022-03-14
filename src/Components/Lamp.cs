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

    [ComponentProp("Side")]
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
            Raylib.DrawRectangleRec(rec, this.Values[i] == LogicValue.HIGH ? Color.BLUE : Color.LIGHTGRAY);
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new DescriptionLamp(this.Position, this.Rotation, this.UniqueID, this.Bits, this.Identifier, this.Side);
    }
}