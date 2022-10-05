using System.Drawing;
using System.Numerics;
using LogiX.Rendering;

namespace LogiX.Graphics.UI;

public interface IGUIPositionProvider
{
    Vector2 GetNextEmitPosition();
    Vector2 GetTotalEmitSize();
    string GetID();
    void Reset();

    void RegisterItemSize(Vector2 size);
    void SameLine();
    bool MouseOver { get; set; }
    bool CanBeDragged();
    Vector2 GetTopLeft();
    void MoveTo(Vector2 position);

    bool Begin();
    void End();
}

public class NewGUIWindow : IGUIPositionProvider
{
    public Vector2 Position { get; set; }
    public string Name { get; set; }

    public Vector2 EmitPositionStart => this.Position; // Potentially add padding here as well
    public Vector2 NextEmitPosition { get; set; }
    public Vector2 EmitPositionPreviousLine { get; set; }
    public Vector2 TotalEmitSize { get; set; }

    public bool MouseOver { get; set; } = false;
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
        //this.MouseOver = false;
    }

    public void RegisterItemSize(Vector2 size)
    {
        var lineWidth = this.NextEmitPosition.X - this.EmitPositionStart.X;

        this.EmitPositionPreviousLine = this.NextEmitPosition + new Vector2(size.X, 0f);
        this.NextEmitPosition = this.EmitPositionStart + new Vector2(0f, this.TotalEmitSize.Y + size.Y);
        this.TotalEmitSize = new Vector2(Math.Max(size.X + lineWidth, this.TotalEmitSize.X), this.TotalEmitSize.Y + size.Y);

        var topLeft = this.EmitPositionStart + new Vector2(0f, this.TotalEmitSize.Y - size.Y);
        var bottomRight = this.EmitPositionPreviousLine + new Vector2(0f, size.Y);

        this.LinesInWindow.Add(topLeft.CreateRect(bottomRight - topLeft));
    }

    public void SameLine()
    {
        var lineHeight = this.NextEmitPosition.Y - this.EmitPositionPreviousLine.Y;
        var lineWidth = this.NextEmitPosition.X - this.EmitPositionPreviousLine.X;

        this.NextEmitPosition = this.EmitPositionPreviousLine;
        this.TotalEmitSize = new Vector2(this.TotalEmitSize.X, this.TotalEmitSize.Y - lineHeight);

        this.LinesInWindow.RemoveAt(this.LinesInWindow.Count - 1);
    }

    public bool Begin()
    {
        NewGUI.PushNextItemSize(new Vector2(12f));
        NewGUI.PushNextItemID(this.Name + "_expand_button");
        if (NewGUI.Button(this.Expanded ? "-" : "+"))
        {
            this.Expanded = !this.Expanded;
        }

        NewGUI.SameLine();
        NewGUI.Spacer(5f);
        NewGUI.SameLine();
        NewGUI.Label(this.Name);

        if (this.Expanded)
        {
            NewGUI.Spacer(5f);
        }

        return this.Expanded;
    }

    public void End()
    {
        var windowRect = this.Position.CreateRect(this.TotalEmitSize).Inflate(5, 5, 5, 5);
        this.MouseOver = windowRect.Contains(Input.GetMousePositionInWindow());

        NewGUI.RenderRectangle(windowRect, ColorF.DarkGray * 0.8f, 0);

        var topLine = this.LinesInWindow.First();
        var topLineAllWay = new RectangleF(topLine.X, topLine.Y, windowRect.Width, topLine.Height);

        NewGUI.RenderRectangle(topLineAllWay.Inflate(5, 5, -5, 5), ColorF.Purple * 0.5f, 1);
    }

    public Vector2 GetTotalEmitSize()
    {
        return this.TotalEmitSize;
    }

    public bool CanBeDragged()
    {
        return true;
    }

    public Vector2 GetTopLeft()
    {
        return this.Position;
    }

    public void MoveTo(Vector2 position)
    {
        this.Position = position;
    }
}

public class MainMenuBar : IGUIPositionProvider
{
    public Vector2 Position { get; set; }

    public Vector2 EmitPositionStart => this.Position + new Vector2(0, 3); // Potentially add padding here as well
    public Vector2 NextEmitPosition { get; set; }
    public Vector2 TotalEmitSize { get; set; }

    public bool MouseOver { get; set; } = false;

    public MainMenuBar()
    {

    }

    public Vector2 GetNextEmitPosition()
    {
        return this.NextEmitPosition;
    }

    public string GetID()
    {
        return "MainMenuBar";
    }

    public void Reset()
    {
        this.Position = Vector2.Zero;
        this.NextEmitPosition = this.EmitPositionStart;
        this.TotalEmitSize = Vector2.Zero;
        //this.MouseOver = false;
    }

    public void RegisterItemSize(Vector2 size)
    {
        // var lineWidth = this.NextEmitPosition.X - this.EmitPositionStart.X;

        // this.EmitPositionPreviousLine = this.NextEmitPosition + new Vector2(size.X, 0f);
        // this.NextEmitPosition = this.EmitPositionStart + new Vector2(0f, this.TotalEmitSize.Y + size.Y);
        // this.TotalEmitSize = new Vector2(Math.Max(size.X + lineWidth, this.TotalEmitSize.X), this.TotalEmitSize.Y + size.Y);

        // var topLeft = this.EmitPositionStart + new Vector2(0f, this.TotalEmitSize.Y - size.Y);
        // var bottomRight = this.EmitPositionPreviousLine + new Vector2(0f, size.Y);

        // this.LinesInWindow.Add(topLeft.CreateRect(bottomRight - topLeft));

        // Everything always is put to the right of the previous item, no rows or anything, so SameLine() will do nothing.
        this.NextEmitPosition += new Vector2(size.X, 0f);
        this.TotalEmitSize = new Vector2(this.TotalEmitSize.X + size.X, Math.Max(size.Y, this.TotalEmitSize.Y));
    }

    public void SameLine()
    {

    }

    public bool Begin()
    {
        NewGUI.Spacer(2f);
        return true;
    }

    public void End()
    {
        var windowRect = this.Position.CreateRect(new Vector2(DisplayManager.GetWindowSizeInPixels().X, 23));
        this.MouseOver = windowRect.Contains(Input.GetMousePositionInWindow());

        NewGUI.RenderRectangle(windowRect, ColorF.Purple * 0.8f, 0);
    }

    public Vector2 GetTotalEmitSize()
    {
        return this.TotalEmitSize;
    }

    public bool CanBeDragged()
    {
        return false;
    }

    public Vector2 GetTopLeft()
    {
        return this.Position;
    }

    public void MoveTo(Vector2 position)
    {
        this.Position = position;
    }
}