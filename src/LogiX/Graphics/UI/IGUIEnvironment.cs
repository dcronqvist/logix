using System.Drawing;
using System.Numerics;
using LogiX.Rendering;

namespace LogiX.Graphics.UI;

public interface IGUIEnvironment
{
    Vector2 GetNextEmitPosition();
    Vector2 GetTotalEmitSize();
    Vector2 GetTotalEmitSizePreviousFrame();
    string GetID();
    void Reset();

    void RegisterItemSize(Vector2 size);
    void SameLine();
    bool MouseOver { get; set; }
    bool CanBeDragged();
    bool CanBeReordered();
    bool AlwaysOnTop();
    Vector2 GetTopLeft();
    void MoveTo(Vector2 position);

    bool Begin(int flags);
    void End();

    void DirectChildReturnedTrue();
}

[Flags]
public enum GUIWindowFlags : int
{
    None = 1 << 0,
    NoExpandButton = 1 << 1,
    NoTopBar = 1 << 2,
    NoDrag = 1 << 3
}

public class NewGUIWindow : IGUIEnvironment
{
    public Vector2 Position { get; set; }
    public string Name { get; set; }

    public Vector2 EmitPositionStart => this.Position; // Potentially add padding here as well
    public Vector2 NextEmitPosition { get; set; }
    public Vector2 EmitPositionPreviousLine { get; set; }
    public Vector2 TotalEmitSize { get; set; }
    public Vector2 TotalEmitSizePreviousFrame { get; set; }

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
        this.TotalEmitSizePreviousFrame = this.TotalEmitSize;
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

    int flags = -1;
    public bool Begin(int flags)
    {
        GUIWindowFlags windowFlags = (GUIWindowFlags)flags;
        this.flags = flags;

        if (!windowFlags.HasFlag(GUIWindowFlags.NoExpandButton) && !windowFlags.HasFlag(GUIWindowFlags.NoTopBar))
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
        }

        if (!windowFlags.HasFlag(GUIWindowFlags.NoTopBar))
        {
            NewGUI.Label(this.Name);

            if (this.Expanded)
            {
                NewGUI.Spacer(5f);
            }
        }

        return this.Expanded;
    }

    public void End()
    {
        GUIWindowFlags windowFlags = (GUIWindowFlags)this.flags;
        var windowRect = this.Position.CreateRect(this.TotalEmitSize).Inflate(5, 5, 5, 5);
        this.MouseOver = windowRect.Contains(Input.GetMousePositionInWindow());

        NewGUI.RenderRectangle(windowRect, NewGUI.BackgroundColor, 0);

        if (!windowFlags.HasFlag(GUIWindowFlags.NoTopBar))
        {
            var topLine = this.LinesInWindow.First();
            var topLineAllWay = new RectangleF(topLine.X, topLine.Y, windowRect.Width, topLine.Height);

            NewGUI.RenderRectangle(topLineAllWay.Inflate(5, 5, -5, 5), NewGUI.BackgroundColorAccent, 1);
        }
    }

    public Vector2 GetTotalEmitSize()
    {
        return this.TotalEmitSize;
    }

    public bool CanBeDragged()
    {
        return !((GUIWindowFlags)this.flags).HasFlag(GUIWindowFlags.NoDrag);
    }

    public Vector2 GetTopLeft()
    {
        return this.Position;
    }

    public void MoveTo(Vector2 position)
    {
        this.Position = position;
    }

    public Vector2 GetTotalEmitSizePreviousFrame()
    {
        return this.TotalEmitSizePreviousFrame;
    }

    public bool CanBeReordered()
    {
        return true;
    }

    public void DirectChildReturnedTrue()
    {

    }

    public bool AlwaysOnTop()
    {
        return false;
    }
}

[Flags]
public enum MainMenuBarFlags : int
{
    None = 1 << 0,
    TestFlag = 1 << 1,
}

public class MainMenuBar : IGUIEnvironment
{
    public Vector2 Position { get; set; }

    public Vector2 EmitPositionStart => this.Position + new Vector2(1, 4); // Potentially add padding here as well
    public Vector2 NextEmitPosition { get; set; }
    public Vector2 TotalEmitSize { get; set; }
    public Vector2 TotalEmitSizePreviousFrame { get; set; }

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
        this.TotalEmitSizePreviousFrame = this.TotalEmitSize;
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

    public bool Begin(int flags)
    {
        this.RegisterItemSize(new Vector2(2, 2));
        return true;
    }

    public void End()
    {
        var windowRect = this.Position.CreateRect(new Vector2(DisplayManager.GetWindowSizeInPixels().X, 23));
        this.MouseOver = windowRect.Contains(Input.GetMousePositionInWindow());

        NewGUI.RenderRectangle(windowRect, NewGUI.BackgroundColorAccent, 0);
    }

    public Vector2 GetTotalEmitSize()
    {
        return new Vector2(DisplayManager.GetWindowSizeInPixels().X, 23);
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

    public Vector2 GetTotalEmitSizePreviousFrame()
    {
        return this.TotalEmitSizePreviousFrame;
    }

    public bool CanBeReordered()
    {
        return false;
    }

    public void DirectChildReturnedTrue()
    {

    }

    public bool AlwaysOnTop()
    {
        return false;
    }
}

public class MenuEnv : IGUIEnvironment
{
    public bool MouseOver { get; set; } = false;
    public string Label { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 EmitPositionStart => this.Position + new Vector2(5, 5); // Potentially add padding here as well
    public Vector2 NextEmitPosition { get; set; }
    public Vector2 TotalEmitSize { get; set; }
    public Vector2 TotalEmitSizePreviousFrame { get; set; }

    public bool Expanded { get; set; } = false;

    public Vector2 ButtonSize { get; set; }
    public bool ButtonHot { get; set; } = false;

    public MenuEnv(string label, Vector2 position)
    {
        this.Label = label;
        this.Position = position;
    }

    public bool Begin(int flags)
    {
        this.Position = NewGUI.CurrentEnvironment.GetNextEmitPosition();
        NewGUI.PushNextItemID(this.Label + "_expand_button");
        if (NewGUI.Button(this.Label))
        {
            this.Expanded = !this.Expanded;
        }
        var buttonSize = NewGUI.GetPreviousItemSize();
        var buttonHot = NewGUI.IsPreviousItemHot();
        this.ButtonHot = buttonHot;

        this.RegisterItemSize(buttonSize);

        this.ButtonSize = buttonSize;

        return this.Expanded;
    }

    public bool CanBeDragged()
    {
        return false;
    }

    public void End()
    {
        //GUIWindowFlags windowFlags = (GUIWindowFlags)this.flags;
        var backgroundRect = (this.Position + new Vector2(5, 5)).CreateRect(this.TotalEmitSize).Inflate(5, 5, 5, 5);
        var renderRect = backgroundRect.Inflate(0, -ButtonSize.Y, 0, 0);
        this.MouseOver = renderRect.Contains(Input.GetMousePositionInWindow());

        if (!this.ButtonHot && !this.MouseOver)
        {
            this.Expanded = false;
        }

        if (this.Expanded)
        {
            NewGUI.RenderRectangle(renderRect, NewGUI.BackgroundColor, 0);
        }

        if (!this.Expanded)
        {
            this.TotalEmitSize = Vector2.Zero;
            this.TotalEmitSizePreviousFrame = Vector2.Zero;
            this.MouseOver = false;
        }
    }

    public string GetID()
    {
        return this.Label;
    }

    public Vector2 GetNextEmitPosition()
    {
        return this.NextEmitPosition;
    }

    public Vector2 GetTopLeft()
    {
        return this.EmitPositionStart;
    }

    public Vector2 GetTotalEmitSize()
    {
        return this.TotalEmitSize;
    }

    public Vector2 GetTotalEmitSizePreviousFrame()
    {
        return this.TotalEmitSizePreviousFrame;
    }

    public void MoveTo(Vector2 position)
    {
        this.Position = position;
    }

    public void RegisterItemSize(Vector2 size)
    {
        var lineWidth = this.NextEmitPosition.X - this.EmitPositionStart.X;

        this.NextEmitPosition = this.EmitPositionStart + new Vector2(0f, this.TotalEmitSize.Y + size.Y);
        this.TotalEmitSize = new Vector2(Math.Max(size.X + lineWidth, this.TotalEmitSize.X), this.TotalEmitSize.Y + size.Y);
    }

    public void Reset()
    {
        this.TotalEmitSizePreviousFrame = this.TotalEmitSize;
        this.TotalEmitSize = Vector2.Zero;
        this.NextEmitPosition = this.EmitPositionStart;
    }

    public void SameLine()
    {
        // Does nothing, menus are always vertical
    }

    public bool CanBeReordered()
    {
        return false;
    }

    public void DirectChildReturnedTrue()
    {
        this.Expanded = false;
    }

    public bool AlwaysOnTop()
    {
        return true;
    }
}