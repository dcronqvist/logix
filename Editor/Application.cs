using LogiX.SaveSystem;

namespace LogiX.Editor;

public abstract class Application
{
    public abstract void Initialize();
    public abstract void LoadContent();
    public abstract void Update();
    public abstract void Render();
    public abstract void SubmitUI();

    public delegate void WindowResizeCallback(int x, int y);
    public event WindowResizeCallback OnWindowResized;
    private RenderTexture2D uiTexture;

    protected FileDialog filePicker;
    private int fileDialogType;
    private string filePickerSelectedFile;
    private Action<string> onFilePickerSelect;
    private Action onFilePickerCancel;

    private bool encounteredError;
    private string lastErrorMessage;

    public void Run(int windowWidth, int windowHeight, string windowTitle, int initialTargetFPS)
    {
#if OSX
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_WINDOW_HIGHDPI);
#else
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE);
#endif

        Initialize();

        Raylib.InitWindow(windowWidth, windowHeight, windowTitle);
        this.uiTexture = Raylib.LoadRenderTexture(windowWidth, windowHeight);
        Raylib.SetTargetFPS(initialTargetFPS);

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
                OnWindowResized?.Invoke(x, y);
            }

            UserInput.Begin();
            Update();

            Raylib.BeginTextureMode(this.uiTexture);
            Raylib.BeginBlendMode(BlendMode.BLEND_ALPHA);

            Raylib.ClearBackground(Color.BLANK);
            SubmitUI();
            HandleFileDialog();
            HandleErrorModal();
            igc.Draw();

            Raylib.EndTextureMode();

            Raylib.BeginDrawing();

            Render();
            Raylib.DrawTextureRec(this.uiTexture.texture, new Rectangle(0, 0, this.uiTexture.texture.width, -this.uiTexture.texture.height), Vector2.Zero, Color.WHITE);

            Raylib.EndDrawing();
            UserInput.End();
        }

        igc.Dispose();
        Raylib.CloseWindow();
    }

    public void SelectFile(string startDirectory, Action<string> onSelect, Action onCancel, params string[] fileExtensions)
    {
        this.filePicker = new FileDialog(startDirectory, fileExtensions);
        this.onFilePickerCancel = onCancel;
        this.onFilePickerSelect = onSelect;
        this.filePickerSelectedFile = "";
        this.fileDialogType = 0;
    }

    public void SelectFolder(string startDirectory, Action<string> onSelect, Action onCancel)
    {
        this.filePicker = new FileDialog(startDirectory);
        this.onFilePickerCancel = onCancel;
        this.onFilePickerSelect = onSelect;
        this.filePickerSelectedFile = "";
        this.fileDialogType = 1;
    }

    protected void ModalError(string errorMessage)
    {
        this.encounteredError = true;
        this.lastErrorMessage = errorMessage;
    }

    private void HandleErrorModal()
    {
        if (encounteredError)
        {
            ImGui.OpenPopup("###Error");

            if (ImGui.BeginPopupModal("###Error", ref this.encounteredError, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text(this.lastErrorMessage);

                if (ImGui.Button("OK"))
                {
                    this.encounteredError = false;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }
    }

    private void HandleFileDialog()
    {
        if (this.filePicker != null)
        {
            filePicker.Open();

            if (fileDialogType == 0)
            {
                if (filePicker.Pick(ref this.filePickerSelectedFile, out bool selected))
                {
                    if (selected)
                    {
                        // perform "select"
                        this.onFilePickerSelect.Invoke(this.filePickerSelectedFile);
                    }
                    else
                    {
                        // perform "cancel"
                        this.onFilePickerCancel.Invoke();
                    }

                    this.filePicker = null;
                }
            }
            else if (fileDialogType == 1)
            {
                if (filePicker.SelectFolder(ref this.filePickerSelectedFile, out bool selected))
                {
                    if (selected)
                    {
                        // perform "select"
                        this.onFilePickerSelect.Invoke(this.filePickerSelectedFile);
                    }
                    else
                    {
                        // perform "cancel"
                        this.onFilePickerCancel.Invoke();
                    }

                    this.filePicker = null;
                }
            }

        }
    }
}