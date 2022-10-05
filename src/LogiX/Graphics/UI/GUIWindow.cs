using System.Drawing;
using System.Numerics;
using LogiX.Rendering;

namespace LogiX.Graphics.UI;

public interface IItemRenderCall
{
    public void Render(Camera2D camera);
}

public class RectangleRenderCall : IItemRenderCall
{
    public RectangleF Rect { get; set; }
    public ColorF Color { get; set; }

    public RectangleRenderCall(RectangleF rect, ColorF color)
    {
        this.Rect = rect;
        this.Color = color;
    }

    public void Render(Camera2D camera)
    {
        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        PrimitiveRenderer.RenderRectangle(shader, this.Rect, Vector2.Zero, 0f, this.Color, camera);
    }
}

public class LineRenderCall : IItemRenderCall
{
    public Vector2 Start { get; set; }
    public Vector2 End { get; set; }
    public ColorF Color { get; set; }

    public LineRenderCall(Vector2 start, Vector2 end, ColorF color)
    {
        this.Start = start;
        this.End = end;
        this.Color = color;
    }

    public void Render(Camera2D camera)
    {
        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        PrimitiveRenderer.RenderLine(shader, this.Start, this.End, 1, this.Color, camera);
    }
}

public class TextRenderCall : IItemRenderCall
{
    public string Text { get; set; }
    public Vector2 Position { get; set; }
    public ColorF Color { get; set; }
    public Font Font { get; set; }

    public TextRenderCall(string text, Vector2 position, ColorF color, Font font)
    {
        this.Text = text;
        this.Position = position;
        this.Color = color;
        this.Font = font;
    }

    public void Render(Camera2D camera)
    {
        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");
        TextRenderer.RenderText(shader, this.Font, this.Text, this.Position, 1f, this.Color, camera);
    }
}

public class GUIWindow
{
    public Vector2 Position { get; set; }
    public string Name { get; set; }

    public Vector2 EmitPositionStart => this.Position; // Potentially add padding here as well

    public Vector2 NextEmitPosition { get; set; }
    public Vector2 EmitPositionPreviousLine { get; set; }
    public Vector2 TotalEmitSize { get; set; }
    public bool MouseOver { get; set; }

    public bool Expanded { get; set; } = true;

    public List<RectangleF> LinesInWindow { get; private set; } = new();

    public GUIWindow(string name)
    {
        this.Name = name;
    }

    public GUIWindow(string name, Vector2 intialPosition)
    {
        this.Name = name;
        this.Position = intialPosition;
    }

    public void Reset()
    {
        this.NextEmitPosition = EmitPositionStart;
        this.TotalEmitSize = Vector2.Zero;
        this.MouseOver = false;
        this.LinesInWindow.Clear();
    }
}