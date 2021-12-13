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

    public void Run(int windowWidth, int windowHeight, string windowTitle, int initialTargetFPS)
    {
#if OSX
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_WINDOW_HIGHDPI);
#else
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_WINDOW_RESIZABLE);
#endif

        Initialize();

        Raylib.InitWindow(windowWidth, windowHeight, windowTitle);
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

            SubmitUI();

            Raylib.BeginDrawing();

            Render();

            igc.Draw();

            Raylib.EndDrawing();
            UserInput.End();
        }

        igc.Dispose();
        Raylib.CloseWindow();
    }
}