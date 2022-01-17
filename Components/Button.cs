using LogiX.Editor;
using LogiX.SaveSystem;

namespace LogiX.Components;

public class Button : Component
{
    public List<LogicValue> Values { get; set; }
    public override Vector2 Size => new Vector2(this.Outputs[0].Bits * (30 + 2) + 2, 34);
    public override bool TextVisible => false;
    public override bool DrawBoxNormal => false;

    public Button(int bits, Vector2 position) : base(Util.EmptyList<int>(), Util.Listify(bits), position)
    {
        this.Values = Util.NValues(LogicValue.LOW, bits);
    }

    public override void PerformLogic()
    {
        this.Outputs[0].SetValues(this.Values);
    }

    public override void Update(Vector2 mousePosInWorld)
    {
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_RIGHT_BUTTON))
        {
            if (Raylib.CheckCollisionPointCircle(mousePosInWorld, this.Position, 17f))
            {
                // TOGGLE VALUE
                this.Values[0] = LogicValue.HIGH;
            }
        }
        else
        {
            this.Values[0] = LogicValue.LOW;
        }

        base.Update(mousePosInWorld);
    }

    public override void Render(Vector2 mousePosInWorld)
    {
        base.Render(mousePosInWorld);

        Raylib.DrawCircleV(this.Position, 17f, Color.BLACK);
        Raylib.DrawCircleV(this.Position, 16f, this.Values[0] == LogicValue.LOW ? new Color(240, 240, 240, 255) : Color.BLUE);
    }

    public override ComponentDescription ToDescription()
    {
        return new GenIODescription(this.Position, Util.EmptyList<IODescription>(), Util.Listify(new IODescription(this.Outputs[0].Bits)), ComponentType.Button);
    }

    public override void OnSingleSelectedSubmitUI()
    {

    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.EmptyGateAmount();
    }
}