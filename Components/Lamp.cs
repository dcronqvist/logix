using LogiX.SaveSystem;

namespace LogiX.Components;

public class Lamp : Component
{
    public List<LogicValue> Values { get; private set; }
    public override Vector2 Size => new Vector2(this.Inputs[0].Bits * (30 + 2) + 2, 34);
    public override bool TextVisible => false;
    public string ID { get; set; }

    public Lamp(int bits, Vector2 position, string id = "") : base(Util.Listify(bits), Util.EmptyList<int>(), position)
    {
        this.Values = Util.NValues(LogicValue.LOW, bits);
        this.ID = id;
    }

    public override void PerformLogic()
    {
        this.Values = this.Inputs[0].Values;
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

        Vector2 rightMiddle = new Vector2(this.Box.x + this.Box.width, this.Box.y + this.Box.height / 2f);

        if (Raylib.CheckCollisionPointRec(mousePosInWorld, this.Box))
        {
            // Display label
            int fontSize = 20;
            Vector2 measure = Raylib.MeasureTextEx(Raylib.GetFontDefault(), this.ID, fontSize, 1);
            Raylib.DrawTextEx(Raylib.GetFontDefault(), this.ID, rightMiddle + new Vector2(10, measure.Y / -2f), fontSize, 1, Color.BLACK);
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new SLDescription(this.Position, Util.Listify(new IODescription(this.Inputs[0].Bits)), Util.EmptyList<IODescription>(), ComponentType.Lamp, this.ID);
    }

    public override void OnSingleSelectedSubmitUI()
    {
        ImGui.Begin("Lamp", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoNavInputs);

        string id = this.ID;
        ImGui.SetNextItemWidth(100);
        ImGui.PushID(this.uniqueID);
        ImGui.InputText("Name", ref id, 13);
        ImGui.PopID();
        this.ID = id;

        ImGui.End();
    }
}