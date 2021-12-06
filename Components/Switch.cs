using LogiX.Editor;

namespace LogiX.Components;

public class Switch : Component
{
    public List<LogicValue> Values { get; private set; }
    public override Vector2 Size => new Vector2(this.Outputs[0].Bits * (30 + 2) + 2, 34);
    public override bool TextVisible => false;

    public Switch(int bits, Vector2 position) : base(Util.EmptyList<int>(), Util.Listify(bits), position)
    {
        this.Values = Util.NValues(LogicValue.LOW, bits);
    }

    public override void PerformLogic()
    {
        this.Outputs[0].SetValues(this.Values);
    }

    public override void Update(Vector2 mousePosInWorld)
    {
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
        {
            for (int i = 0; i < this.Values.Count; i++)
            {
                Vector2 pos = new Vector2(this.Box.x, this.Box.y) + new Vector2(2 + 32 * i, 2);
                Rectangle rec = new Rectangle(pos.X, pos.Y, 30f, 30f);
                if (Raylib.CheckCollisionPointRec(mousePosInWorld, rec))
                {
                    // TOGGLE VALUE
                    this.ToggleValue(i);
                }
            }


        }

        base.Update(mousePosInWorld);
    }

    public override void Render(Vector2 mousePosInWorld)
    {
        base.Render(mousePosInWorld);

        // ADDITIONAL STUFF, ONE THING PER BIT
        for (int i = 0; i < this.Values.Count; i++)
        {
            Vector2 offset = new Vector2(2 + 32 * i, 2);
            Rectangle rec = new Rectangle(offset.X, offset.Y, 30f, 30f);
            Raylib.DrawRectangleV(new Vector2(this.Box.x, this.Box.y) + offset, new Vector2(30f), this.Values[i] == LogicValue.LOW ? new Color(240, 240, 240, 255) : Color.BLUE);
        }
    }

    public void ToggleValue(int index)
    {
        this.Values[index] = this.Values[index] == LogicValue.LOW ? LogicValue.HIGH : LogicValue.LOW;
    }
}