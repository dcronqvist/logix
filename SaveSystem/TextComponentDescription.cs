using LogiX.Components;

namespace LogiX.SaveSystem;

public class TextComponentDescription : ComponentDescription
{
    [JsonProperty(PropertyName = "text")]
    public string Text { get; set; }

    public TextComponentDescription(Vector2 position, string text) : base(position, new List<IODescription>(), new List<IODescription>(), ComponentType.TextLabel)
    {
        this.Text = text;
    }

    public override Component ToComponent(bool preserveID)
    {
        TextComponent c = new TextComponent(this.Position);
        c.SetText(this.Text);

        if (preserveID)
            c.SetUniqueID(this.ID);

        return c;
    }
}