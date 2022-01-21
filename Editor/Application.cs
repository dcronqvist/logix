using LogiX.SaveSystem;

namespace LogiX.Editor;

public enum ErrorModalType
{
    OK,
    OKCancel,
    YesNo
}

public enum ErrorModalResult
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
    public event WindowResizeCallback OnWindowResized;
    public Vector2 WindowSize { get; set; }
    private RenderTexture2D uiTexture;

    protected Modal currentModal;

    private bool encounteredError;
    private string lastErrorMessage;
    private ErrorModalType lastErrorModalType;
    private Action<ErrorModalResult>? lastErrorCallback;

    public void Run(int windowWidth, int windowHeight, string windowTitle, int initialTargetFPS, string iconFile = null)
    {
#if OSX
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_WINDOW_HIGHDPI);
#else
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE);
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
        igc.Load(windowWidth, windowHeight);

        LoadContent();

        // Main application loop
        while (!Raylib.WindowShouldClose())
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
            SubmitUI();

            HandleErrorModal();
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

        igc.Dispose();
        Raylib.CloseWindow();
        this.OnClose();
    }

    public void ModalError(string errorMessage, ErrorModalType type = ErrorModalType.OK, Action<ErrorModalResult> onResult = null)
    {
        this.encounteredError = true;
        this.lastErrorMessage = errorMessage;
        this.lastErrorModalType = type;
        this.lastErrorCallback = onResult;
    }

    private void HandleErrorModal()
    {
        if (encounteredError)
        {
            ImGui.OpenPopup("###Error");

            if (ImGui.BeginPopupModal("###Error", ref this.encounteredError, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text(this.lastErrorMessage);

                if (this.lastErrorModalType == ErrorModalType.OK)
                {
                    if (ImGui.Button("OK"))
                    {
                        ImGui.CloseCurrentPopup();
                        encounteredError = false;
                        this.lastErrorCallback?.Invoke(ErrorModalResult.OK);
                    }
                }
                else if (this.lastErrorModalType == ErrorModalType.OKCancel)
                {
                    if (ImGui.Button("OK"))
                    {
                        ImGui.CloseCurrentPopup();
                        encounteredError = false;
                        this.lastErrorCallback?.Invoke(ErrorModalResult.OK);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        ImGui.CloseCurrentPopup();
                        encounteredError = false;
                        this.lastErrorCallback?.Invoke(ErrorModalResult.Cancel);
                    }
                }
                else if (this.lastErrorModalType == ErrorModalType.YesNo)
                {
                    if (ImGui.Button("Yes"))
                    {
                        ImGui.CloseCurrentPopup();
                        encounteredError = false;
                        this.lastErrorCallback?.Invoke(ErrorModalResult.Yes);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No"))
                    {
                        ImGui.CloseCurrentPopup();
                        encounteredError = false;
                        this.lastErrorCallback?.Invoke(ErrorModalResult.No);
                    }
                }

                ImGui.EndPopup();
            }
        }
    }
}