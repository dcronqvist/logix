using LogiX.SaveSystem;

namespace LogiX.Components;

public class TextComponent : Component
{
    private string text;
    public override string Text => this.text;
    public override Vector2 Size
    {
        get
        {
            Vector2 textSize = Raylib.MeasureTextEx(Util.OpenSans, this.text, 18, 1);

            return new Vector2(textSize.X + 10, textSize.Y + 10);
        }
    }

    public TextComponent(Vector2 position) : base(Util.EmptyList<int>(), Util.EmptyList<int>(), position)
    {
        this.text = "Text";
    }

    public void SetText(string text)
    {
        this.text = text;
    }

    public override void PerformLogic()
    {
        // Do nothing
    }

    public override ComponentDescription ToDescription()
    {
        return new TextComponentDescription(this.Position, this.text);
    }

    public override void OnSingleSelectedSubmitUI()
    {
        ImGui.Begin("Text", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoNavInputs);

        string text = this.text;
        ImGui.PushID(this.uniqueID);
        ImGui.InputTextMultiline("Text", ref text, 100, new Vector2(200, 200));
        ImGui.PopID();
        this.text = text;

        ImGui.End();
    }
}