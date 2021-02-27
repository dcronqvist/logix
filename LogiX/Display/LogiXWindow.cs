using LogiX.Logging;
using LogiX.Settings;
using LogiX.Utils;
using Raylib_cs;
using System.Numerics;

namespace LogiX.Display
{
    class LogiXWindow : BaseWindow
    {
        Camera2D cam;

        Vector2 currentMousePos;
        Vector2 previousMousePos;

        public LogiXWindow() : base(new Vector2(1280, 720), "LogiX")
        {

        }

        public override void Initialize()
        {
            // Should initialize some basic settings and stuff
            // Load settings files etc.

            // Set window icon and stuff can also be done with this.
            SettingManager.LoadSettings();

            // Apply window size settings to window
            int windowWidth = int.Parse(SettingManager.GetSetting("window-width", "1440"));
            int windowHeight = int.Parse(SettingManager.GetSetting("window-height", "768"));
            Vector2 size = new Vector2(windowWidth, windowHeight);
            SetWindowSize(size, false);
            LogManager.AddEntry($"Creating LogiX window with width = {(int)size.X} and height = {(int)size.Y}");

            // Create new camera that's focused on (0,0).
            // This should maybe be done somewhere else. But is good for testing atm.
            cam = new Camera2D(size / 2f, Vector2.Zero, 0f, 1f);
        }

        public override void LoadContent()
        {
            // Load files for use in the program.
        }

        public void RenderGrid()
        {
            // TODO: Move this to more suitable spot
            int windowWidth = int.Parse(SettingManager.GetSetting("window-width", "1440"));
            int windowHeight = int.Parse(SettingManager.GetSetting("window-height", "768"));
            Vector2 size = new Vector2(windowWidth, windowHeight);
            Vector2 camPos = cam.target;
            Vector2 viewSize = size / cam.zoom;

            int pixelsBetweenLines = int.Parse(SettingManager.GetSetting("grid-size", "250"));

            // Render vertical lines
            for (int i = (int)((camPos.X - viewSize.X / 2f) / pixelsBetweenLines); i <= (int)((camPos.X + viewSize.X / 2f) / pixelsBetweenLines); i++)
            {
                float lineX = i * pixelsBetweenLines;
                float lineYstart = (camPos.Y - viewSize.Y / 2f);
                float lineYend = (camPos.Y + viewSize.Y / 2f);
                Raylib.DrawLine((int)lineX, (int)lineYstart, (int)lineX, (int)lineYend, Color.DARKGRAY.Opacity(0.3f));
            }

            // Render vertical lines
            for (int i = (int)((camPos.Y - viewSize.Y / 2f) / pixelsBetweenLines); i <= (int)((camPos.Y + viewSize.Y / 2f) / pixelsBetweenLines); i++)
            {
                float lineY = i * pixelsBetweenLines;
                float lineXstart = (camPos.X - viewSize.X / 2f);
                float lineXend = (camPos.X + viewSize.X / 2f);
                Raylib.DrawLine((int)lineXstart, (int)lineY, (int)lineXend, (int)lineY, Color.DARKGRAY.Opacity(0.3f));
            }
        }

        public override void Render()
        {
            Raylib.BeginDrawing();
            Raylib.BeginMode2D(cam);

            Raylib.ClearBackground(Color.LIGHTGRAY);
            RenderGrid();

            Raylib.EndMode2D();
            Raylib.DrawFPS(5, 5);
            Raylib.EndDrawing();
        }

        public override void Update()
        {
            // TODO: Move this to more suitable place
            currentMousePos = Raylib.GetMousePosition();

            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_MIDDLE_BUTTON))
            {
                cam.target -= (currentMousePos - previousMousePos) * 1f / cam.zoom;
            }

            previousMousePos = currentMousePos;
        }

        public override void Unload()
        {
            LogManager.DumpToFile();
        }
    }
}
