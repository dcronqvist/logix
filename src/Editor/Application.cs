using LogiX.Editor.StateMachine;

namespace LogiX.Editor;

public enum ModalButtonsType
{
    OK,
    OKCancel,
    YesNo
}

public abstract class Application
{
    public abstract void Initialize();
    public abstract void LoadContent();
    public abstract void Update();
    public abstract void Render();
    public abstract void SubmitUI();
    public abstract void OnClose();

    public delegate void WindowResizeCallback(int x, int y);
    public event WindowResizeCallback? OnWindowResized;
    public Vector2 WindowSize { get; set; }
    private RenderTexture2D uiTexture;

    private bool modalRequested;
    private string? modalTitle;
    private Func<bool> modalSubmit;
    private List<(string, Action?)> modalButtons;
    private ImGuiWindowFlags modalFlags;

    public void Run(int windowWidth, int windowHeight, string windowTitle, int initialTargetFPS, string? iconFile = null)
    {
        ConfigFlags commonFlags = ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE;
#if OSX
        // Maybe this should eventually be moved to a settings page rather than being forced?
        Raylib.SetConfigFlags(commonFlags | ConfigFlags.FLAG_WINDOW_HIGHDPI); // On OSX, we need to enable high DPI because of Retina display
#else
        Raylib.SetConfigFlags(commonFlags);
#endif
        Initialize();

        Raylib.InitWindow(windowWidth, windowHeight, windowTitle);

        if (iconFile != null)
        {
            Image i = Raylib.LoadImage(iconFile);
            Raylib.ImageFormat(ref i, PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8A8);
            Raylib.SetWindowIcon(i);
        }

        this.uiTexture = Raylib.LoadRenderTexture(windowWidth, windowHeight);
        Raylib.SetTargetFPS(initialTargetFPS);
        WindowSize = new Vector2(windowWidth, windowHeight);

        Raylib.SetExitKey(KeyboardKey.KEY_NULL);
        ImguiController igc = new ImguiController();

        string assetDir = Directory.GetCurrentDirectory() + "/assets/";
        List<(string, string, float)> fonts = new List<(string, string, float)>() {
            ("opensans", "opensans.ttf", 16),
            ("opensans-20", "opensans.ttf", 20),
            ("opensans-bold", "opensans-bold.ttf", 16),
            ("opensans-bold-20", "opensans-bold.ttf", 20)
        };
        fonts = fonts.Select(x => (x.Item1, assetDir + x.Item2, x.Item3)).ToList();

        igc.Load(windowWidth, windowHeight, fonts, out Dictionary<string, ImFontPtr> fontsPtrs);
        Util.ImGuiFonts = fontsPtrs;

        LoadContent();
        Util.OpenSans = Raylib.LoadFontEx($"{Directory.GetCurrentDirectory()}/assets/opensans-bold.ttf", 100, Enumerable.Range(0, 1000).ToArray(), 1000);
        Raylib.SetTextureFilter(Util.OpenSans.texture, TextureFilter.TEXTURE_FILTER_TRILINEAR);

        // Main application loop
        while (!Raylib.WindowShouldClose())
        {
            try
            {
                // Feed the input events to our ImGui controller, which passes them through to ImGui.
                igc.Update(Raylib.GetFrameTime());

                if (Raylib.IsWindowResized())
                {
                    int x = Raylib.GetScreenWidth();
                    int y = Raylib.GetScreenHeight();
                    igc.Resize(x, y);
                    WindowSize = new Vector2(x, y);
                    Raylib.UnloadTexture(this.uiTexture.texture);
                    this.uiTexture = Raylib.LoadRenderTexture(x, y);
                    OnWindowResized?.Invoke(x, y);
                }

                UserInput.Begin();
                Update();

                Raylib.BeginTextureMode(this.uiTexture);

                Raylib.ClearBackground(Color.BLANK);
                ImGui.PushFont(fontsPtrs["opensans"]);
                SubmitUI();

                HandleModal();
                ImGui.PopFont();
                igc.Draw();

                Raylib.EndTextureMode();

                Raylib.BeginDrawing();

                Render();
                //Raylib.BeginBlendMode(BlendMode.BLEND_MULTIPLIED);
                Raylib.DrawTextureRec(this.uiTexture.texture, new Rectangle(0, 0, this.uiTexture.texture.width, -this.uiTexture.texture.height), Vector2.Zero, Color.WHITE);
                //Raylib.EndBlendMode();

                Raylib.EndDrawing();
                UserInput.End();

            }
            catch (NotImplementedException nie)
            {
                // Application ran into a feature that hasn't been implemented yet
                this.ModalError("Not implemented: " + nie.Message, ModalButtonsType.OK);
            }
            catch (Exception e)
            {
                // Application ran into uncaught exception
                this.ModalError("Uncaught error: " + e.Message, ModalButtonsType.OK);
            }
        }


        igc.Dispose();
        Raylib.CloseWindow();
        this.OnClose();
    }

    public KeyboardKey GetPrimaryMod()
    {
#if OSX
        return KeyboardKey.KEY_LEFT_SUPER;
#else
        return KeyboardKey.KEY_LEFT_CONTROL;
#endif
    }

    public void Modal(string modalTitle, Func<bool> modalSubmit, ImGuiWindowFlags modalFlags, params (string, Action?)[] modalButtons)
    {
        this.modalRequested = true;

        this.modalTitle = modalTitle;
        this.modalSubmit = modalSubmit;
        this.modalButtons = modalButtons.ToList();
        this.modalFlags = modalFlags;
    }

    public void ModalDefault(string modalTitle, string message, ModalButtonsType type, Action? ok = null, Action? cancel = null, Action? yes = null, Action? no = null, ImGuiWindowFlags? flags = null)
    {
        List<(string, Action?)> buttons = new List<(string, Action?)>();

        switch (type)
        {
            case ModalButtonsType.OK:
                buttons.Add(("OK", ok));
                break;
            case ModalButtonsType.OKCancel:
                buttons.Add(("OK", ok));
                buttons.Add(("Cancel", cancel));
                break;
            case ModalButtonsType.YesNo:
                buttons.Add(("Yes", yes));
                buttons.Add(("No", no));
                break;
        }

        this.Modal(modalTitle, () =>
        {
            ImGui.Text(message);
            return false;
        }, flags ?? ImGuiWindowFlags.AlwaysAutoResize, buttons.ToArray());
    }

    public void ModalMarkdown(string modalTitle, string message, ModalButtonsType type, Action? ok = null, Action? cancel = null, Action? yes = null, Action? no = null, ImGuiWindowFlags? flags = null)
    {
        List<(string, Action?)> buttons = new List<(string, Action?)>();

        switch (type)
        {
            case ModalButtonsType.OK:
                buttons.Add(("OK", ok));
                break;
            case ModalButtonsType.OKCancel:
                buttons.Add(("OK", ok));
                buttons.Add(("Cancel", cancel));
                break;
            case ModalButtonsType.YesNo:
                buttons.Add(("Yes", yes));
                buttons.Add(("No", no));
                break;
        }

        this.Modal(modalTitle, () =>
        {
            Util.RenderMarkdown(message);
            return false;
        }, flags ?? ImGuiWindowFlags.AlwaysAutoResize, buttons.ToArray());
    }

    public bool AppModalRequested()
    {
        return this.modalRequested;
    }

    public void ModalError(string errorMessage, ModalButtonsType type = ModalButtonsType.OK, Action? ok = null, Action? cancel = null, Action? yes = null, Action? no = null)
    {
        ModalDefault("Error", errorMessage, type, ok, cancel, yes, no);
    }

    private void HandleModal()
    {
        if (modalRequested)
        {
            ImGui.OpenPopup($"{this.modalTitle} ###{this.modalTitle}");

            ImGui.SetNextWindowPos(this.WindowSize / 2f, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            if (ImGui.BeginPopupModal($"{this.modalTitle} ###{this.modalTitle}", ref this.modalRequested, this.modalFlags))
            {
                if (this.modalSubmit())
                {
                    this.modalRequested = false;
                    ImGui.CloseCurrentPopup();
                }

                foreach ((string button, Action? callback) in this.modalButtons)
                {
                    if (ImGui.Button(button))
                    {
                        this.modalRequested = false;
                        ImGui.CloseCurrentPopup();
                        callback?.Invoke();
                    }
                    ImGui.SameLine();
                }

                ImGui.EndPopup();
            }
        }
    }
}