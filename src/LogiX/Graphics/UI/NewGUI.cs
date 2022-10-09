using System.Drawing;
using System.Numerics;
using LogiX.GLFW;
using LogiX.Rendering;

namespace LogiX.Graphics.UI;

public static class NewGUI
{
    private static Font Font { get; set; }

    public static string HotID { get; private set; }
    public static string ActiveID { get; private set; }
    public static string KeyboardFocusID { get; private set; }

    public static Stack<IGUIEnvironment> CurrentEnvironmentStack { get; private set; } = new();
    public static IGUIEnvironment CurrentEnvironment => CurrentEnvironmentStack.Peek();
    public static Dictionary<string, IGUIEnvironment> Environments { get; private set; } = new();
    public static Dictionary<string, List<IItemRenderCall>> EnvironmentRenderCalls { get; private set; } = new();
    public static List<string> EnvironmentRenderOrder { get; private set; } = new();

    public static Stack<Vector2> ItemPositionStack { get; private set; } = new();
    public static Stack<Vector2> ItemSizeStack { get; private set; } = new();

    public static IGUIEnvironment HoveredEnvironment { get; private set; }
    public static IGUIEnvironment FocusedEnvironment { get; private set; }
    public static IGUIEnvironment DraggingEnvironment { get; private set; }
    public static IGUIEnvironment CurrentlyOpenMenu { get; private set; }
    public static Vector2 DragOffset { get; private set; }

    public static Stack<string> VisibleIDs { get; private set; } = new();
    public static Stack<string> PushedIDs { get; private set; } = new();
    public static Stack<Vector2> PushedSizes { get; private set; } = new();

    public static ColorF BackgroundColor => ColorF.DarkGray * 0.8f;
    public static ColorF BackgroundColorAccent => ColorF.Purple;
    public static ColorF ItemColor => ColorF.Lerp(ColorF.Purple, ColorF.White, 0.25f);
    public static ColorF ItemHoverColor => ColorF.Lerp(ColorF.Purple, ColorF.White, 0.4f);
    public static ColorF ItemActiveColor => ColorF.Lerp(ColorF.Purple, ColorF.White, 0.6f);
    public static ColorF TextColor => ColorF.White;

    public static Queue<char> InputBuffer { get; private set; } = new();

    public static void Init(Font font)
    {
        Font = font;

        Input.OnChar += (sender, e) =>
        {
            if (KeyboardFocusID is not null)
            {
                InputBuffer.Enqueue(e);
            }
        };

        Input.OnBackspace += (sender, e) =>
        {
            if (KeyboardFocusID is not null)
            {
                InputBuffer.Enqueue('\b');
            }
        };

        Input.OnEnterPressed += (sender, e) =>
        {
            if (KeyboardFocusID is not null)
            {
                InputBuffer.Enqueue('\r');
            }
        };


        Input.OnKeyPressOrRepeat += (sender, e) =>
        {
            if (e.Item1 == Keys.Left)
            {
                _caretPosition = Math.Max(0, _caretPosition - 1);
            }
            if (e.Item1 == Keys.Right)
            {
                _caretPosition++;
            }
        };
    }

    public static void Begin()
    {
        ItemSizeStack.Clear();
        VisibleIDs.Clear();
        ItemPositionStack.Clear();
        ItemSizeStack.Clear();

        foreach (var env in Environments.Values)
        {
            env.Reset();
        }

        foreach (var envRenders in EnvironmentRenderCalls.Values)
        {
            envRenders.Clear();
        }
    }

    public static void End()
    {
        _caretBlinkCounter += GameTime.DeltaTime;
        if (_caretBlinkCounter > _caretBlinkTime * 2f)
        {
            _caretBlinkCounter = 0f;
        }

        if (Input.IsKeyPressed(Keys.Escape) && KeyboardFocusID is not null)
        {
            KeyboardFocusID = null;
        }

        var list = EnvironmentRenderOrder.ToArray();
        foreach (var envID in list)
        {
            if (Environments.TryGetValue(envID, out var env))
            {
                if (env.MouseOver)
                {
                    if (Input.IsMouseButtonPressed(MouseButton.Left))
                    {
                        if (env.CanBeReordered())
                        {
                            SetEnvironmentAtFront(env);
                            FocusEnvironment(env);
                        }

                        if (HotID == null && env.CanBeDragged())
                        {
                            DraggingEnvironment = env;
                            DragOffset = Input.GetMousePositionInWindow() - env.GetTopLeft();
                        }

                        break;
                    }
                }
            }
        }

        if (DraggingEnvironment is not null)
        {
            if (Input.IsMouseButtonReleased(MouseButton.Left))
            {
                DraggingEnvironment = null;
                DragOffset = Vector2.Zero;
            }
            else
            {
                DraggingEnvironment.MoveTo(Input.GetMousePositionInWindow() - DragOffset);
            }
        }

        foreach (var envID in EnvironmentRenderOrder.Reverse<string>())
        {
            if (EnvironmentRenderCalls.TryGetValue(envID, out var calls))
            {
                foreach (var call in calls)
                {
                    call.Render(Framebuffer.GetDefaultCamera());
                }
            }
        }

        if (!Environments.Values.Any(e => e.MouseOver))
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                ResetFocusedEnvironment();
                KeyboardFocusID = null;
            }
            else
            {
                HoveredEnvironment = null;
            }
        }
        else
        {
            var first = EnvironmentRenderOrder.First(e => Environments[e].MouseOver);
            HoveredEnvironment = Environments[first];
        }

        if (HotID != null && !VisibleIDs.Contains(HotID))
        {
            HotID = null;
        }

        if (ActiveID != null && !VisibleIDs.Contains(ActiveID))
        {
            ActiveID = null;
        }
    }

    public static void SetEnvironmentAtFront(IGUIEnvironment provider)
    {
        EnvironmentRenderOrder.Remove(provider.GetID());
        EnvironmentRenderOrder.Insert(0, provider.GetID());
    }

    public static void FocusEnvironment(IGUIEnvironment provider)
    {
        FocusedEnvironment = provider;
    }

    public static void ResetFocusedEnvironment()
    {
        FocusedEnvironment = null;
    }

    public static bool TryBecomeHot(string id)
    {
        // if (Context.DraggingWindow != null)
        // {
        //     return false;
        // }

        var list = EnvironmentRenderOrder.ToArray();
        foreach (var envID in list)
        {
            var currEnv = Environments[envID];
            if (currEnv.MouseOver && currEnv != CurrentEnvironment)
            {
                return false;
            }

            if (currEnv == CurrentEnvironment)
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

    public static bool TryBecomeActive(string id)
    {
        var list = EnvironmentRenderOrder.ToArray();
        foreach (var envID in list)
        {
            var currEnv = Environments[envID];

            if (currEnv.MouseOver && currEnv != CurrentEnvironment)
            {
                return false;
            }

            if (currEnv == CurrentEnvironment)
            {
                break;
            }
        }

        if (ActiveID == null)
        {
            ActiveID = id;
            KeyboardFocusID = null;
            SetEnvironmentAtFront(CurrentEnvironment);
            FocusEnvironment(CurrentEnvironment);
            return true;
        }

        return false;
    }

    public static bool TryGetKeyboardFocus(string id)
    {
        var list = EnvironmentRenderOrder.ToArray();
        foreach (var envID in list)
        {
            var currEnv = Environments[envID];

            if (currEnv.MouseOver && currEnv != CurrentEnvironment)
            {
                return false;
            }

            if (currEnv == CurrentEnvironment)
            {
                break;
            }
        }

        if (KeyboardFocusID == null)
        {
            KeyboardFocusID = id;
            SetEnvironmentAtFront(CurrentEnvironment);
            FocusEnvironment(CurrentEnvironment);
            return true;
        }

        return false;
    }

    public static bool IsHot(string id)
    {
        return HotID == id;
    }

    public static bool IsActive(string id)
    {
        return ActiveID == id;
    }

    public static bool HasKeyboardFocus(string id)
    {
        return KeyboardFocusID == id;
    }

    public static void RenderRectangle(RectangleF rect, ColorF color, int insertAt = -1)
    {
        var env = CurrentEnvironment;

        if (!EnvironmentRenderCalls.ContainsKey(env.GetID()))
        {
            EnvironmentRenderCalls.Add(env.GetID(), new());
        }

        if (insertAt != -1)
        {
            EnvironmentRenderCalls[env.GetID()].Insert(insertAt, new RectangleRenderCall(rect, color));
        }
        else
        {
            EnvironmentRenderCalls[env.GetID()].Add(new RectangleRenderCall(rect, color));
        }
    }

    public static void RenderText(string text, Vector2 position, ColorF color, int insertAt = -1)
    {
        var env = CurrentEnvironment;

        if (!EnvironmentRenderCalls.ContainsKey(env.GetID()))
        {
            EnvironmentRenderCalls.Add(env.GetID(), new());
        }

        if (insertAt != -1)
        {
            EnvironmentRenderCalls[env.GetID()].Insert(insertAt, new TextRenderCall(text, position, color, Font));
        }
        else
        {
            EnvironmentRenderCalls[env.GetID()].Add(new TextRenderCall(text, position, color, Font));
        }
    }

    public static void RenderLine(Vector2 start, Vector2 end, ColorF color, int insertAt = -1)
    {
        var env = CurrentEnvironment;

        if (!EnvironmentRenderCalls.ContainsKey(env.GetID()))
        {
            EnvironmentRenderCalls.Add(env.GetID(), new());
        }

        if (insertAt != -1)
        {
            EnvironmentRenderCalls[env.GetID()].Insert(insertAt, new LineRenderCall(start, end, color));
        }
        else
        {
            EnvironmentRenderCalls[env.GetID()].Add(new LineRenderCall(start, end, color));
        }
    }

    // ------------------------------ PUSHING NEXT DATA ------------------------------

    public static void PushNextItemID(string id)
    {
        PushedIDs.Push(id);
    }

    public static string GetNextID(string id)
    {
        if (PushedIDs.Count > 0)
        {
            var i = PushedIDs.Pop();
            VisibleIDs.Push(i);
            return i;
        }

        VisibleIDs.Push(id);
        return id;
    }

    public static void PushNextItemSize(Vector2 size)
    {
        PushedSizes.Push(size);
    }

    public static Vector2 GetNextSize(Vector2 size)
    {
        if (PushedSizes.Count > 0)
        {
            return PushedSizes.Pop();
        }

        return size;
    }

    // ------------------------------ ENVIRONMENTS ------------------------------

    private static bool BeginNewEnvironment(IGUIEnvironment provider, int flags, bool beginPartOfEnv)
    {
        if (beginPartOfEnv)
        {
            if (!Environments.ContainsKey(provider.GetID()))
            {
                Environments.Add(provider.GetID(), provider);
                EnvironmentRenderOrder.Add(provider.GetID());
                provider.Reset();
                CurrentEnvironmentStack.Push(provider);
                SetEnvironmentAtFront(provider);
                return provider.Begin(flags);
            }
            else
            {
                var env = Environments[provider.GetID()];
                CurrentEnvironmentStack.Push(env);
                bool begin = env.Begin(flags);
                if (env.AlwaysOnTop())
                {
                    SetEnvironmentAtFront(env);
                }
                return begin;
            }
        }
        else
        {
            if (!Environments.ContainsKey(provider.GetID()))
            {
                Environments.Add(provider.GetID(), provider);
                EnvironmentRenderOrder.Add(provider.GetID());
                provider.Reset();
                bool begin = provider.Begin(flags);
                CurrentEnvironmentStack.Push(provider);
                SetEnvironmentAtFront(provider);
                return begin;
            }
            else
            {
                var env = Environments[provider.GetID()];
                bool begin = env.Begin(flags);
                CurrentEnvironmentStack.Push(env);
                if (env.AlwaysOnTop())
                {
                    SetEnvironmentAtFront(env);
                }
                return begin;
            }
        }
    }

    private static void EndCurrentEnvironment()
    {
        CurrentEnvironment.End();
        CurrentEnvironmentStack.Pop();
    }

    // ------------------------------ SIZES & POSITIONS ------------------------------

    private static void RegisterItemSize(Vector2 size)
    {
        CurrentEnvironment.RegisterItemSize(size);
        ItemSizeStack.Push(size);
    }

    public static Vector2 GetPreviousItemSize()
    {
        return ItemSizeStack.Peek();
    }

    public static void PushItemPosition(Vector2 position)
    {
        ItemPositionStack.Push(position);
    }

    public static Vector2 GetPreviousItemPosition()
    {
        return ItemPositionStack.Peek();
    }

    public static void SameLine()
    {
        CurrentEnvironment.SameLine();
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
        return HoveredEnvironment != null;
    }

    public static bool AnyItemActive()
    {
        return ActiveID != null;
    }

    public static bool IsPreviousItemHot()
    {
        return HotID == VisibleIDs.Peek();
    }

    public static bool IsPreviousItemActive()
    {
        return ActiveID == VisibleIDs.Peek();
    }

    public static ColorF GetItemColor(string id)
    {
        if (id == ActiveID)
        {
            return ItemActiveColor;
        }
        else if (id == HotID)
        {
            return ItemHoverColor;
        }
        else
        {
            return ItemColor;
        }
    }

    // ------------------------------ WINDOWS ------------------------------

    public static bool BeginWindow(string label, Vector2 initialPosition, GUIWindowFlags flags = GUIWindowFlags.None)
    {
        return BeginNewEnvironment(new NewGUIWindow(label, initialPosition), (int)flags, true);
    }

    public static void EndWindow()
    {
        EndCurrentEnvironment();
    }

    // ------------------------------ MAIN MENU BAR ------------------------------

    public static bool BeginMainMenuBar(MainMenuBarFlags flags = MainMenuBarFlags.None)
    {
        return BeginNewEnvironment(new MainMenuBar(), (int)flags, false);
    }

    public static void EndMainMenuBar()
    {
        EndCurrentEnvironment();
    }

    public static bool BeginMenu(string label)
    {
        var menuBegin = BeginNewEnvironment(new MenuEnv(label, CurrentEnvironment.GetNextEmitPosition()), 0, false);
        if (menuBegin)
        {
            CurrentlyOpenMenu = CurrentEnvironment;
        }
        return menuBegin;
    }

    public static void EndMenu()
    {
        EndCurrentEnvironment();
    }

    public static bool BeginContextMenu(ref bool open)
    {
        if (open)
        {
            bool begin = BeginNewEnvironment(new ContextMenu(), 0, true);
            var contextMenu = CurrentEnvironment as ContextMenu;
            if (!begin)
            {
                EndCurrentEnvironment();
                contextMenu.Expanded = true;
                open = false;
            }
            return begin;
        }
        else
        {
            return false;
        }
    }

    public static void EndContextMenu()
    {
        EndCurrentEnvironment();
    }

    // ------------------------------ SPACERS ------------------------------

    public static void Spacer(float size)
    {
        RegisterItemSize(new Vector2(size, size));
    }

    // ------------------------------ WIDGETS ------------------------------

    public static void Label(string label)
    {
        var env = CurrentEnvironment;
        var id = GetNextID(label);
        var position = env.GetNextEmitPosition();

        var size = Font.MeasureString(label, 1f);
        var padded = size.Pad(8f);

        padded = GetNextSize(padded);

        PushItemPosition(position);
        RegisterItemSize(padded);

        RenderText(label, position + padded / 2f - size / 2f, NewGUI.TextColor);
    }

    public static bool Button(string label)
    {
        var id = GetNextID(label);
        var env = CurrentEnvironment;
        var position = env.GetNextEmitPosition();

        var textSize = Font.MeasureString(label, 1f);

        var padded = textSize.Pad(8f);

        padded = GetNextSize(padded);

        PushItemPosition(position);
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
                TryBecomeActive(id);
            }
        }

        if (rect.Contains(Input.GetMousePositionInWindow()))
        {
            TryBecomeHot(id);
        }
        else
        {
            if (IsHot(id))
            {
                HotID = null;
            }
        }

        var color = GetItemColor(id);

        RenderRectangle(rect, color);
        RenderText(label, position + (padded - textSize) / 2f, ColorF.White);

        if (pressed)
        {
            CurrentEnvironment.DirectChildReturnedTrue();
        }

        return pressed;
    }

    public static bool Checkbox(string label, ref bool value)
    {
        var old = value;
        PushNextItemID($"{label}_checkbox_button");
        PushNextItemSize(new Vector2(16, 16f));
        if (Button(value ? "X" : ""))
        {
            value = !value;
        }

        SameLine(5f);
        Label(label);

        var pressed = old != value;
        if (pressed)
        {
            CurrentEnvironment.DirectChildReturnedTrue();
        }
        return pressed;
    }

    public static bool MenuItem(string label)
    {
        // Basically like a button, but with a different styling
        var id = GetNextID(label);
        var env = CurrentEnvironment;
        var position = env.GetNextEmitPosition();

        var textSize = Font.MeasureString(label, 1f);

        var padded = textSize.Pad(8f);

        padded = new Vector2(Math.Max(CurrentEnvironment.GetTotalEmitSizePreviousFrame().X, padded.X), padded.Y);
        padded = GetNextSize(padded);

        PushItemPosition(position);
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
                TryBecomeActive(id);
            }
        }

        if (rect.Contains(Input.GetMousePositionInWindow()))
        {
            TryBecomeHot(id);
        }
        else
        {
            if (IsHot(id))
            {
                HotID = null;
            }
        }

        var color = (IsActive(id)) ? ItemActiveColor : (IsHot(id) ? ItemHoverColor : ColorF.Transparent);

        RenderRectangle(rect, color);
        RenderText(label, position + new Vector2(3, textSize.Y / 2f), TextColor);

        if (pressed)
        {
            CurrentEnvironment.DirectChildReturnedTrue();
        }

        return pressed;
    }

    public static bool Expandable(string label, ref bool open)
    {
        label = $"{(open ? "-" : "+")} {label}";
        var id = GetNextID(label);
        var env = CurrentEnvironment;
        var position = env.GetNextEmitPosition();

        var textSize = Font.MeasureString(label, 1f);

        var padded = textSize.Pad(8f);

        //padded = new Vector2(Math.Max(CurrentEnvironment.GetTotalEmitSizePreviousFrame().X, padded.X), padded.Y);
        padded = GetNextSize(padded);

        PushItemPosition(position);
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
                TryBecomeActive(id);
            }
        }

        if (rect.Contains(Input.GetMousePositionInWindow()))
        {
            TryBecomeHot(id);
        }
        else
        {
            if (IsHot(id))
            {
                HotID = null;
            }
        }

        var color = (IsActive(id)) ? ItemActiveColor : (IsHot(id) ? ItemHoverColor : ColorF.Transparent);

        RenderRectangle(rect, color);
        RenderText(label, position + new Vector2(3, textSize.Y / 2f), ColorF.White);

        if (pressed)
        {
            CurrentEnvironment.DirectChildReturnedTrue();
            open = !open;
        }

        return open;
    }

    private static float _caretBlinkTime = 0.5f;
    private static float _caretBlinkCounter;
    private static bool _caretVisible => _caretBlinkCounter < _caretBlinkTime;
    public static int _caretPosition = -1;

    public static bool InputText(string label, ref string value, Func<bool> validation = null)
    {
        // Basically like a button, but with a different styling
        var id = GetNextID(label);
        var env = CurrentEnvironment;
        var position = env.GetNextEmitPosition();

        var textSize = Font.MeasureString(value, 1f);

        var padded = textSize.Pad(12f);
        padded = new Vector2(Math.Max(padded.X, 100), padded.Y);
        padded = GetNextSize(padded);

        PushItemPosition(position);
        var rect = position.CreateRect(padded);

        RegisterItemSize(padded);

        bool pressed = false;

        if (IsActive(id))
        {
            if (Input.IsMouseButtonReleased(MouseButton.Left))
            {
                if (IsHot(id))
                {
                    if (TryGetKeyboardFocus(id))
                    {
                        _caretPosition = value.Length;
                        _caretBlinkCounter = 0f;
                    }
                }

                ActiveID = null;
            }
        }
        else if (IsHot(id))
        {
            if (Input.IsMouseButtonDown(MouseButton.Left))
            {
                TryBecomeActive(id);
            }
        }

        if (rect.Contains(Input.GetMousePositionInWindow()))
        {
            TryBecomeHot(id);
        }
        else
        {
            if (IsHot(id))
            {
                HotID = null;
            }
        }

        if (HasKeyboardFocus(id))
        {
            if (_caretPosition == -1)
            {
                _caretPosition = value.Length;
                _caretBlinkCounter = 0f;
            }

            while (InputBuffer.Count > 0)
            {
                var c = InputBuffer.Dequeue();
                if (c == '\b')
                {
                    if (value.Length > 0 && _caretPosition > 0)
                    {
                        value = value.Remove(_caretPosition - 1, 1);
                        _caretPosition = Math.Max(0, _caretPosition - 1);
                    }
                }
                else if (c == '\r')
                {
                    //ReleaseKeyboardFocus(id);
                    KeyboardFocusID = null;
                    ActiveID = null;
                    HotID = null;

                    if (validation is null)
                    {
                        pressed = true;
                    }
                    else
                    {
                        pressed = validation();
                    }
                }
                else
                {
                    value = value.Insert(_caretPosition, c.ToString());
                    _caretPosition = Math.Min(value.Length, _caretPosition + 1);
                }
            }

            _caretPosition = Math.Min(value.Length, _caretPosition);
        }

        var color = (IsActive(id)) ? ItemActiveColor : (IsHot(id) ? ItemHoverColor : BackgroundColor);

        var textPos = position + new Vector2(5, rect.Height / 2f) - new Vector2(0, textSize.Y / 2f);
        if (HasKeyboardFocus(id))
        {
            var outlineColor = validation is null ? ItemActiveColor : (validation() ? ItemActiveColor : ColorF.Red);
            RenderRectangle(rect, outlineColor);
            RenderRectangle(rect.Inflate(-2), color);

            var caretMeasure = Font.MeasureString(value.Substring(0, _caretPosition), 1f);
            RenderText("_", textPos + new Vector2(caretMeasure.X, 0), _caretVisible ? ColorF.White : ColorF.Transparent);
        }
        else
        {
            RenderRectangle(rect, ColorF.Darken(color, 1.5f));
            RenderRectangle(rect.Inflate(-2), color);
        }
        RenderText(value.Length == 0 ? label : value, textPos, value.Length == 0 ? TextColor * 0.4f : TextColor);

        if (pressed)
        {
            CurrentEnvironment.DirectChildReturnedTrue();
        }

        return pressed;
    }
}