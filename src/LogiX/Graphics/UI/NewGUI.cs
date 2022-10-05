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

    public static IGUIPositionProvider CurrentEnvironment { get; private set; }
    public static Dictionary<string, IGUIPositionProvider> Environments { get; private set; } = new();
    public static Dictionary<string, List<IItemRenderCall>> EnvironmentRenderCalls { get; private set; } = new();
    public static List<string> EnvironmentRenderOrder { get; private set; } = new();

    public static IGUIPositionProvider HoveredEnvironment { get; private set; }
    public static IGUIPositionProvider FocusedEnvironment { get; private set; }
    public static IGUIPositionProvider DraggingEnvironment { get; private set; }
    public static Vector2 DragOffset { get; private set; }

    public static Stack<string> PushedIDs { get; private set; } = new();
    public static Stack<Vector2> PushedSizes { get; private set; } = new();

    public static void Init(Font font)
    {
        Font = font;
    }

    public static void Begin()
    {
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
        var list = EnvironmentRenderOrder.ToArray();
        foreach (var envID in list)
        {
            var env = Environments[envID];
            if (env.MouseOver)
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    SetEnvironmentAtFront(env);
                    FocusEnvironment(env);

                    if (HotID == null && env.CanBeDragged())
                    {
                        DraggingEnvironment = env;
                        DragOffset = Input.GetMousePositionInWindow() - env.GetTopLeft();
                    }

                    break;
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
            var calls = EnvironmentRenderCalls[envID];

            foreach (var call in calls)
            {
                call.Render(Framebuffer.GetDefaultCamera());
            }
        }

        if (!Environments.Values.Any(e => e.MouseOver))
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                ResetFocusedEnvironment();
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
    }

    public static void SetEnvironmentAtFront(IGUIPositionProvider provider)
    {
        EnvironmentRenderOrder.Remove(provider.GetID());
        EnvironmentRenderOrder.Insert(0, provider.GetID());
    }

    public static void FocusEnvironment(IGUIPositionProvider provider)
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
            return PushedIDs.Pop();
        }

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

    private static bool BeginNewEnvironment(IGUIPositionProvider provider)
    {
        if (!Environments.ContainsKey(provider.GetID()))
        {
            Environments.Add(provider.GetID(), provider);
            EnvironmentRenderOrder.Add(provider.GetID());
            provider.Reset();
            CurrentEnvironment = provider;
            SetEnvironmentAtFront(provider);
            return provider.Begin();
        }
        else
        {
            var env = Environments[provider.GetID()];
            CurrentEnvironment = env;
            return env.Begin();
        }
    }

    private static void EndCurrentEnvironment()
    {
        CurrentEnvironment.End();
        CurrentEnvironment = null;
    }

    // ------------------------------ SIZES ------------------------------

    private static void RegisterItemSize(Vector2 size)
    {
        CurrentEnvironment.RegisterItemSize(size);
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

    // ------------------------------ WINDOWS ------------------------------

    public static bool BeginWindow(string label, Vector2 initialPosition)
    {
        return BeginNewEnvironment(new NewGUIWindow(label, initialPosition));
    }

    public static void EndWindow()
    {
        EndCurrentEnvironment();
    }

    // ------------------------------ MAIN MENU BAR ------------------------------

    public static bool BeginMainMenuBar()
    {
        return BeginNewEnvironment(new MainMenuBar());
    }

    public static void EndMainMenuBar()
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

        RegisterItemSize(padded);

        RenderText(label, position + padded / 2f - size / 2f, ColorF.White);
    }

    public static bool Button(string label)
    {
        var id = GetNextID(label);
        var env = CurrentEnvironment;
        var position = env.GetNextEmitPosition();

        var textSize = Font.MeasureString(label, 1f);

        var padded = textSize.Pad(8f);

        padded = GetNextSize(padded);

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

        var color = (IsActive(id)) ? ColorF.LightGray : (IsHot(id) ? ColorF.Darken(ColorF.Gray, 1.2f) : ColorF.Gray);

        RenderRectangle(rect, color);
        RenderText(label, position + (padded - textSize) / 2f, ColorF.White);

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

        return old != value;
    }
}