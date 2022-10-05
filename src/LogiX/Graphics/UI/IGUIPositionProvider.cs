using System.Drawing;
using System.Numerics;

namespace LogiX.Graphics.UI;

public interface IGUIPositionProvider
{
    Vector2 GetNextEmitPosition();
    string GetID();
    void Reset();

    void RegisterItemSize(GUIContext context, Vector2 size);
    void SameLine(GUIContext context);
    bool IsMouseOver();
}

public class NewGUIWindow : IGUIPositionProvider
{
    public Vector2 Position { get; set; }
    public string Name { get; set; }

    public Vector2 EmitPositionStart => this.Position; // Potentially add padding here as well

    public Vector2 NextEmitPosition { get; set; }
    public Vector2 EmitPositionPreviousLine { get; set; }
    public Vector2 TotalEmitSize { get; set; }

    public bool Expanded { get; set; } = true;

    public List<RectangleF> LinesInWindow { get; private set; } = new();

    public NewGUIWindow(string name)
    {
        this.Name = name;
    }

    public NewGUIWindow(string name, Vector2 intialPosition)
    {
        this.Name = name;
        this.Position = intialPosition;
    }

    public Vector2 GetNextEmitPosition()
    {
        return this.NextEmitPosition;
    }

    public string GetID()
    {
        return this.Name;
    }

    public void Reset()
    {
        this.NextEmitPosition = this.EmitPositionStart;
        this.TotalEmitSize = Vector2.Zero;
        this.LinesInWindow.Clear();
    }

    public void RegisterItemSize(GUIContext context, Vector2 size)
    {
        var lineWidth = this.NextEmitPosition.X - this.EmitPositionStart.X;

        this.EmitPositionPreviousLine = this.NextEmitPosition + new Vector2(size.X, 0f);
        this.NextEmitPosition = this.EmitPositionStart + new Vector2(0f, this.TotalEmitSize.Y + size.Y);
        this.TotalEmitSize = new Vector2(Math.Max(size.X + lineWidth, this.TotalEmitSize.X), this.TotalEmitSize.Y + size.Y);

        var topLeft = this.EmitPositionStart + new Vector2(0f, this.TotalEmitSize.Y - size.Y);
        var bottomRight = this.EmitPositionPreviousLine + new Vector2(0f, size.Y);

        this.LinesInWindow.Add(topLeft.CreateRect(bottomRight - topLeft));
    }

    public void SameLine(GUIContext context)
    {
        var lineHeight = this.NextEmitPosition.Y - this.EmitPositionPreviousLine.Y;
        var lineWidth = this.NextEmitPosition.X - this.EmitPositionPreviousLine.X;

        this.NextEmitPosition = this.EmitPositionPreviousLine;
        this.TotalEmitSize = new Vector2(this.TotalEmitSize.X, this.TotalEmitSize.Y - lineHeight);

        this.LinesInWindow.RemoveAt(this.LinesInWindow.Count - 1);
    }

    public bool IsMouseOver()
    {
        return this.Position.CreateRect(this.TotalEmitSize).Contains(Input.GetMousePositionInWindow());
    }
}