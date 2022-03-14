using LogiX.SaveSystem;

namespace LogiX.Components;

public class Switch : Component
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

    public Switch(int bits, Vector2 position, string? uniqueID = null) : base(position, ComponentType.SWITCH, uniqueID)
    {
        this.AddIO(bits, new IOConfig(ComponentSide.RIGHT));
        this.Bits = bits;
    }

    public override void PerformLogic()
    {
        this.GetIO(0).PushValues(this.Values);
    }

    public void PushUnknown()
    {
        this.GetIO(0).PushValues(Util.NValues(LogicValue.UNKNOWN, this.Bits).ToArray());
    }

    public override void Interact(Editor.Editor editor)
    {
        Vector2 basePos = new Vector2(this.GetRectangle().x, this.GetRectangle().y);

        for (int i = 0; i < this.Bits; i++)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
            {
                Rectangle rec = new Rectangle(basePos.X + WIDTH_PER_BITS * i, basePos.Y, WIDTH_PER_BITS, WIDTH_PER_BITS).Inflate(-2);

                if (Raylib.CheckCollisionPointRec(editor.GetWorldMousePos(), rec))
                {
                    this.Values[i] = this.Values[i] == LogicValue.LOW ? LogicValue.HIGH : LogicValue.LOW;
                }
            }
        }
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
        return new DescriptionSwitch(this.Position, this.Rotation, this.UniqueID, this.Bits, this.Identifier, this.Side);
    }
}