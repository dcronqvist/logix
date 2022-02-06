using LogiX.SaveSystem;

namespace LogiX.Editor;

public enum ModalButtonsType
{
    OK,
    OKCancel,
    YesNo
}

public enum ModalResult
{
    OK,
    Cancel,
    Yes,
    No
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
    private string? lastModalTitle;
    private string? lastModalMessage;
    private ModalButtonsType lastModalType;
    private Action<ModalResult>? lastModalCallback;

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
        igc.Load(windowWidth, windowHeight, out ImFontPtr font);

        LoadContent();

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
                ImGui.PushFont(font);
                SubmitUI();

                HandleModal();
                ImGui.PopFont();
                igc.Draw();

                Raylib.EndTextureMode();

                Raylib.BeginDrawing();

                Render();
                Raylib.BeginBlendMode(BlendMode.BLEND_ALPHA);
                Raylib.DrawTextureRec(this.uiTexture.texture, new Rectangle(0, 0, this.uiTexture.texture.width, -this.uiTexture.texture.height), Vector2.Zero, Color.WHITE);
                Raylib.EndBlendMode();

                Raylib.EndDrawing();
                UserInput.End();

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

    public void Modal(string modalTitle, string modalMessage, ModalButtonsType type = ModalButtonsType.OK, Action<ModalResult>? onResult = null)
    {
        this.modalRequested = true;
        this.lastModalTitle = modalTitle;
        this.lastModalMessage = modalMessage;
        this.lastModalType = type;
        this.lastModalCallback = onResult;
    }

    public void ModalError(string errorMessage, ModalButtonsType type = ModalButtonsType.OK, Action<ModalResult>? onResult = null)
    {
        Modal("Error", errorMessage, type, onResult);
    }

    private void HandleModal()
    {
        if (modalRequested)
        {
            ImGui.OpenPopup($"###{this.lastModalTitle}");

            if (ImGui.BeginPopupModal($"###{this.lastModalTitle}", ref this.modalRequested, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text(this.lastModalMessage);

                if (this.lastModalType == ModalButtonsType.OK)
                {
                    if (ImGui.Button("OK"))
                    {
                        ImGui.CloseCurrentPopup();
                        modalRequested = false;
                        this.lastModalCallback?.Invoke(ModalResult.OK);
                    }
                }
                else if (this.lastModalType == ModalButtonsType.OKCancel)
                {
                    if (ImGui.Button("OK"))
                    {
                        ImGui.CloseCurrentPopup();
                        modalRequested = false;
                        this.lastModalCallback?.Invoke(ModalResult.OK);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        ImGui.CloseCurrentPopup();
                        modalRequested = false;
                        this.lastModalCallback?.Invoke(ModalResult.Cancel);
                    }
                }
                else if (this.lastModalType == ModalButtonsType.YesNo)
                {
                    if (ImGui.Button("Yes"))
                    {
                        ImGui.CloseCurrentPopup();
                        modalRequested = false;
                        this.lastModalCallback?.Invoke(ModalResult.Yes);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No"))
                    {
                        ImGui.CloseCurrentPopup();
                        modalRequested = false;
                        this.lastModalCallback?.Invoke(ModalResult.No);
                    }
                }

                ImGui.EndPopup();
            }
        }
    }
}