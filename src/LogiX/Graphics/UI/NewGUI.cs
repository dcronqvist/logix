using System.Drawing;
using System.Numerics;
using LogiX.GLFW;
using LogiX.Rendering;

namespace LogiX.Graphics.UI;

public static class NewGUI
{
    public static GUIContext Context { get; private set; }
    private static Camera2D Camera { get; set; }

    public static string HotID { get; private set; }
    public static string ActiveID { get; private set; }

    public static Dictionary<int, List<IItemRenderCall>> WindowRenderCalls { get; private set; } = new();

    public static void Init(GUIContext context, Camera2D camera)
    {
        Context = context;
        Camera = camera;
    }

    public static void Begin()
    {
        foreach (var window in Context.Windows)
        {
            window.Reset();
        }

        foreach (var window in Context.WindowOrder)
        {
            WindowRenderCalls[window] = new();
        }
    }

    public static void End()
    {
        var list = Context.WindowOrder.ToArray();
        foreach (var index in list)
        {
            var window = Context.Windows[index];
            if (window.MouseOver)
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    Context.SetWindowAtFront(window);
                    Context.FocusWindow(window);

                    if (HotID == null)
                    {
                        // We can drag the window if we are not on a widget
                        Context.DraggingWindow = window;
                        Context.DragOffset = Input.GetMousePositionInWindow() - window.Position;
                    }

                    break;
                }
            }
        }

        if (Context.DraggingWindow != null)
        {
            if (Input.IsMouseButtonReleased(MouseButton.Left))
            {
                Context.DraggingWindow = null;
                Context.DragOffset = Vector2.Zero;
            }
            else
            {
                Context.DraggingWindow.Position = Input.GetMousePositionInWindow() - Context.DragOffset;
            }
        }

        foreach (var index in Context.WindowOrder.Reverse<int>())
        {
            var calls = WindowRenderCalls[index];

            foreach (var call in calls)
            {
                call.Render(Camera);
            }
        }

        if (!Context.Windows.Any(w => w.MouseOver))
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                Context.ResetFocusedWindow();
            }
            else
            {
                Context.HoveredWindow = null;
            }
        }
        else
        {
            Context.HoveredWindow = Context.Windows[Context.WindowOrder.First(w => Context.Windows[w].MouseOver)];
        }
    }

    private static bool TryBecomeHot(string id, GUIWindow window)
    {
        if (Context.DraggingWindow != null)
        {
            return false;
        }

        var list = Context.WindowOrder.ToArray();
        foreach (var index in list)
        {
            var w = Context.Windows[index];
            if (w.MouseOver && w != window)
            {
                return false;
            }

            if (w == window)
            {
                break;
            }
        }

        if (HotID == null)
        {
            HotID = id;
            return true;
        }

        return false;
    }

    private static bool TryBecomeActive(string id, GUIWindow window)
    {
        var list = Context.WindowOrder.ToArray();
        foreach (var index in list)
        {
            var w = Context.Windows[index];
            if (w.MouseOver && w != window)
            {
                return false;
            }

            if (w == window)
            {
                break;
            }
        }

        if (ActiveID == null)
        {
            ActiveID = id;
            Context.SetWindowAtFront(window);
            Context.FocusWindow(window);
            return true;
        }

        return false;
    }

    private static bool IsHot(string id)
    {
        return HotID == id;
    }

    private static bool IsActive(string id)
    {
        return ActiveID == id;
    }

    private static void RenderRectangle(RectangleF rect, ColorF color, int insertAt = -1)
    {
        var window = Context.CurrentWindow;
        var windowIndex = Context.Windows.IndexOf(window);

        if (!WindowRenderCalls.ContainsKey(windowIndex))
        {
            WindowRenderCalls.Add(windowIndex, new());
        }

        if (insertAt != -1)
        {
            WindowRenderCalls[windowIndex].Insert(insertAt, new RectangleRenderCall(rect, color));
        }
        else
        {
            WindowRenderCalls[windowIndex].Add(new RectangleRenderCall(rect, color));
        }
    }

    private static void RenderText(string text, Vector2 position, ColorF color, int insertAt = -1)
    {
        var window = Context.CurrentWindow;
        var windowIndex = Context.Windows.IndexOf(window);

        if (!WindowRenderCalls.ContainsKey(windowIndex))
        {
            WindowRenderCalls.Add(windowIndex, new());
        }

        if (insertAt != -1)
        {
            WindowRenderCalls[windowIndex].Insert(insertAt, new TextRenderCall(text, position, color, Context.Font));
        }
        else
        {
            WindowRenderCalls[windowIndex].Add(new TextRenderCall(text, position, color, Context.Font));
        }
    }

    private static void RenderLine(Vector2 start, Vector2 end, ColorF color, int insertAt = -1)
    {
        var window = Context.CurrentWindow;
        var windowIndex = Context.Windows.IndexOf(window);

        if (!WindowRenderCalls.ContainsKey(windowIndex))
        {
            WindowRenderCalls.Add(windowIndex, new());
        }

        if (insertAt != -1)
        {
            WindowRenderCalls[windowIndex].Insert(insertAt, new LineRenderCall(start, end, color));
        }
        else
        {
            WindowRenderCalls[windowIndex].Add(new LineRenderCall(start, end, color));
        }
    }

    // ------------------------------ SIZES ------------------------------

    private static void RegisterItemSize(Vector2 size)
    {
        float padding = 0f;
        var window = Context.CurrentWindow;

        var lineWidth = window.NextEmitPosition.X - window.EmitPositionStart.X;

        window.EmitPositionPreviousLine = window.NextEmitPosition + new Vector2(size.X + padding, 0f);
        window.NextEmitPosition = window.EmitPositionStart + new Vector2(0f, window.TotalEmitSize.Y + size.Y + padding);
        window.TotalEmitSize = new Vector2(Math.Max(size.X + lineWidth, window.TotalEmitSize.X), window.TotalEmitSize.Y + size.Y + padding);

        var topLeft = window.EmitPositionStart + new Vector2(0f, window.TotalEmitSize.Y - size.Y - padding);
        var bottomRight = window.EmitPositionPreviousLine + new Vector2(-padding, size.Y);

        window.LinesInWindow.Add(topLeft.CreateRect(bottomRight - topLeft));
    }

    public static void SameLine()
    {
        var window = Context.CurrentWindow;
        var lineHeight = window.NextEmitPosition.Y - window.EmitPositionPreviousLine.Y;
        var lineWidth = window.NextEmitPosition.X - window.EmitPositionPreviousLine.X;

        window.NextEmitPosition = window.EmitPositionPreviousLine;
        window.TotalEmitSize = new Vector2(window.TotalEmitSize.X, window.TotalEmitSize.Y - lineHeight);

        window.LinesInWindow.RemoveAt(window.LinesInWindow.Count - 1);
    }

    public static void SameLine(float hSpace)
    {
        SameLine();
        Spacer(hSpace);
        SameLine();
    }

    // ------------------------------ QUERIES ------------------------------

    public static bool AnyWindowHovered()
    {
        return Context.HoveredWindow != null;
    }

    // ------------------------------ WINDOWS ------------------------------

    public static bool BeginWindow(string label, Vector2 initialPosition)
    {
        var window = Context.GetWindow(label, initialPosition);

        Context.PushNextID(window.Name + "_expand_button");
        Context.PushNextSize(new Vector2(12f, 12f));
        if (NewGUI.Button(window.Expanded ? "-" : "+"))
        {
            window.Expanded = !window.Expanded;
        }

        NewGUI.SameLine();
        NewGUI.Spacer(5f);
        NewGUI.SameLine();
        NewGUI.Label(label);

        if (window.Expanded)
        {
            NewGUI.Spacer(5f);
        }

        return window.Expanded;
    }

    public static void EndWindow()
    {
        var window = Context.CurrentWindow;

        var windowRect = window.Position.CreateRect(window.TotalEmitSize).Inflate(5, 5, 5, 5);
        window.MouseOver = windowRect.Contains(Input.GetMousePositionInWindow());

        RenderRectangle(windowRect, ColorF.DarkGray * (Context.FocusedWindow == window ? 0.9f : 0.75f), 0);

        var topLine = window.LinesInWindow.First();
        var topLineAllWay = new RectangleF(topLine.X, topLine.Y, windowRect.Width, topLine.Height);

        RenderRectangle(topLineAllWay.Inflate(5, 5, -5, 5), ColorF.Purple * 0.5f, 1);

        Context.CurrentWindowIndex--;
    }

    // ------------------------------ SPACERS ------------------------------

    public static void Spacer(float size)
    {
        RegisterItemSize(new Vector2(size, size));
    }

    public static void SeparatorH()
    {
        var window = Context.CurrentWindow;
        var position = window.NextEmitPosition;

        float paddingAround = 5f;

        RegisterItemSize(new Vector2(window.TotalEmitSize.X, paddingAround * 2f));
        RenderLine(position + new Vector2(0f, paddingAround), position + new Vector2(window.TotalEmitSize.X, paddingAround), ColorF.Gray * 0.5f);
    }

    public static void SeparatorV()
    {
        var window = Context.CurrentWindow;
        var position = window.NextEmitPosition;

        float paddingAround = 5f;

        var lineHeight = window.LinesInWindow.SkipLast(2).Last().Height;
        RegisterItemSize(new Vector2(paddingAround * 2f, lineHeight));
        RenderLine(position + new Vector2(paddingAround, -1f), position + new Vector2(paddingAround, lineHeight - 1), ColorF.Gray * 0.5f);
    }

    // ------------------------------ WIDGETS ------------------------------

    public static void Label(string label)
    {
        var id = Context.GetNextID(label);
        var window = Context.CurrentWindow;
        var position = window.NextEmitPosition;

        var size = Context.MeasureString(label);
        var padded = size.Pad(8f);

        RegisterItemSize(padded);

        RenderText(label, position + padded / 2f - size / 2f, ColorF.White);
    }

    public static bool Button(string label)
    {
        var id = Context.GetNextID(label);
        var window = Context.CurrentWindow;
        var position = window.NextEmitPosition;

        var textSize = Context.MeasureString(label);

        var padded = textSize.Pad(8f);

        if (Context.TryGetPushedNextSize(out var size))
        {
            padded = size;
        }

        var rect = position.CreateRect(padded);

        RegisterItemSize(padded);

        bool pressed = false;

        if (IsActive(id))
        {
            if (Input.IsMouseButtonReleased(MouseButton.Left))
            {
                if (IsHot(id))
                {
                    pressed = true;
                }

                ActiveID = null;
            }
        }
        else if (IsHot(id))
        {
            if (Input.IsMouseButtonDown(MouseButton.Left))
            {
                TryBecomeActive(id, window);
            }
        }

        if (rect.Contains(Input.GetMousePositionInWindow()))
        {
            TryBecomeHot(id, window);
        }
        else
        {
            if (IsHot(id))
            {
                HotID = null;
            }
        }

        var color = (IsActive(id)) ? ColorF.LightGray : (IsHot(id) ? ColorF.Darken(ColorF.Gray, 1.2f) : ColorF.Gray);

        RenderRectangle(rect, color);
        RenderText(label, position + (padded - textSize) / 2f, ColorF.White);

        return pressed;
    }

    public static bool Checkbox(string label, ref bool value)
    {
        var old = value;
        Context.PushNextID($"{label}_checkbox_button");
        Context.PushNextSize(new Vector2(16, 16f));
        if (Button(value ? "X" : ""))
        {
            value = !value;
        }

        SameLine(5f);
        Label(label);

        return old != value;
    }
}