using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class TextCompData : INodeDescriptionData
{
    [NodeDescriptionProperty("Text", StringMultiline = true)]
    public string Text { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new TextCompData()
        {
            Text = ""
        };
    }
}

[ScriptType("TEXTCOMP"), NodeInfo("Text", "Common", "logix_core:docs/components/text.md")]
public class TextComp : BoxNode<TextCompData>
{
    public override string Text => "";
    public override float TextScale => 1f;

    private TextCompData _data;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        yield break;
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield break; // No pins
    }

    public override Vector2i GetSize()
    {
        var font = Constants.NODE_FONT_REAL;

        var textScale = 1f;
        var gridSize = Constants.GRIDSIZE;
        var lines = this._data.Text.Split('\n');
        var lineMeasures = lines.Select(l => font.MeasureString(l, textScale));

        var maxTextWidth = lineMeasures.Max(l => l.X);
        var sumHeight = lineMeasures.Sum(l => l.Y);

        var textWidth = Utilities.CeilToMultipleOf(maxTextWidth, gridSize);
        var textHeight = Utilities.CeilToMultipleOf(sumHeight, gridSize);

        var size = new Vector2(Math.Max(textWidth, Constants.GRIDSIZE), Math.Max(textHeight, Constants.GRIDSIZE));

        return new Vector2i((int)size.X.CeilToMultipleOf(Constants.GRIDSIZE) / Constants.GRIDSIZE + 2, (int)size.Y.CeilToMultipleOf(Constants.GRIDSIZE) / Constants.GRIDSIZE + 2);
    }

    public override Vector2i GetSizeRotated()
    {
        return this.GetSize();
    }

    public override void Initialize(TextCompData data)
    {
        this._data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break;
    }

    public override void Render(PinCollection pins, Camera2D camera)
    {
        base.Render(pins, camera);

        var font = Constants.NODE_FONT_REAL;

        var textScale = 1f;
        var gridSize = Constants.GRIDSIZE;
        var lines = this._data.Text.Split('\n');
        var lineMeasures = lines.Select(l => font.MeasureString(l, textScale));

        var maxTextWidth = lineMeasures.Max(l => l.X);
        var sumHeight = lineMeasures.Sum(l => l.Y);

        var textWidth = Utilities.CeilToMultipleOf(maxTextWidth, gridSize);
        var textHeight = Utilities.CeilToMultipleOf(sumHeight, gridSize);

        var textSize = new Vector2(textWidth, textHeight);

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var size = this.GetSize();
        var realSize = size.ToVector2(Constants.GRIDSIZE);
        var rect = pos.CreateRect(realSize);

        var textLines = this._data.Text.Split('\n');
        var middle = new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        var lineSize = textSize.Y / textLines.Length;

        for (int i = 0; i < textLines.Length; i++)
        {
            var line = textLines[i];
            var linePos = new Vector2(middle.X - textSize.X / 2, middle.Y - textSize.Y / 2 + i * lineSize);
            //TextRenderer.RenderText(font, line, linePos, 1f, 0f, ColorF.Black);
        }
    }
}