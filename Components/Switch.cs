using LogiX.Editor;
using LogiX.SaveSystem;

namespace LogiX.Components;

public class Switch : Component
{
    public List<LogicValue> Values { get; set; }
    public override Vector2 Size => new Vector2(this.Outputs[0].Bits * (30 + 2) + 2, 34);
    public override bool TextVisible => false;
    public string ID { get; set; }

    public Switch(int bits, Vector2 position, string id = "") : base(Util.EmptyList<int>(), Util.Listify(bits), position)
    {
        this.Values = Util.NValues(LogicValue.LOW, bits);
        this.ID = id;
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

        Vector2 leftMiddle = new Vector2(this.Box.x, this.Box.y + this.Box.height / 2f);

        if (this.ID != "")
        {
            // Display label
            int fontSize = 22;
            Vector2 measure = Raylib.MeasureTextEx(Util.OpenSans, this.ID, fontSize, 1);
            Raylib.DrawTextEx(Util.OpenSans, this.ID, leftMiddle + new Vector2(-10 - measure.X, measure.Y / -2f), fontSize, 1, Color.BLACK);
        }
    }

    public void ToggleValue(int index)
    {
        this.Values[index] = this.Values[index] == LogicValue.LOW ? LogicValue.HIGH : LogicValue.LOW;
    }

    public override ComponentDescription ToDescription()
    {
        return new SLDescription(this.Position, Util.EmptyList<IODescription>(), Util.Listify(new IODescription(this.Outputs[0].Bits)), ComponentType.Switch, this.ID);
    }

    public override void OnSingleSelectedSubmitUI()
    {
        ImGui.Begin("Lamp", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoNavInputs);

        string id = this.ID;
        ImGui.SetNextItemWidth(100);
        ImGui.PushID(this.uniqueID);
        ImGui.InputText("Name", ref id, 13, ImGuiInputTextFlags.AlwaysInsertMode);
        ImGui.PopID();
        this.ID = id;

        ImGui.End();
    }
}